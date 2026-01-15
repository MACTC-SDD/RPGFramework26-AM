using System.Net.Sockets;
using System.Text;

/// <summary>
/// Represents a server-side Telnet connection that handles Telnet protocol negotiation and line-based input from a
/// connected client.
/// </summary>
/// <remarks>This class manages Telnet option negotiation, including support for suppressing go-ahead (SGA),
/// negotiating window size (NAWS), and handling client-side echo preferences. It provides methods to read input lines
/// from the client and to control server echo behavior.</remarks>
public sealed class TelnetConnection
{
    private readonly NetworkStream _stream;
    private readonly Encoding _encoding;

    // Telnet commands
    private const byte IAC = 255;
    private const byte DONT = 254;
    private const byte DO = 253;
    private const byte WONT = 252;
    private const byte WILL = 251;
    private const byte SB = 250;
    private const byte SE = 240;
    private const byte ESC = 27;

    // Telnet options
    private const byte OPT_ECHO = 1;
    private const byte OPT_SGA = 3;
    private const byte OPT_NAWS = 31;

    private readonly List<byte> _lineBuffer = new List<byte>(256);

    private enum State { Data, Iac, IacCommand, SubNegotiation, SubIac, AnsiEscape }
    private State _state = State.Data;
    private byte _pendingCommand;
    private byte _subOption;
    private readonly List<byte> _subData = new List<byte>(32);

    public int? TerminalWidth { get; private set; }
    public int? TerminalHeight { get; private set; }

    public string CurrentLineText =>
    _encoding.GetString(_lineBuffer.ToArray());

    public Action? OnlineCommitted; // Invoked when a full line is entered.

    public TelnetConnection(NetworkStream stream, Encoding? encoding = null)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _encoding = encoding ?? Encoding.UTF8;

        // Kick off negotiation (optional but recommended).
        // We want: SGA both ways, NAWS from client.
        SendIac(DO, OPT_SGA);
        SendIac(WILL, OPT_SGA);

        SendIac(DO, OPT_NAWS); // "Please tell me your window size"

        // We do NOT request ECHO by default because we want client-side echo.
        // Most clients will offer WILL ECHO anyway, and we accept it.
    }

    private void ClearCurrentLine()
    {
        _lineBuffer.Clear();
    }

    public string? ReadLine()
    {
        _lineBuffer.Clear();

        Span<byte> buf = stackalloc byte[512];

        while (true)
        {
            int read = _stream.Read(buf);
            if (read <= 0)
            {
                return null;
            }

            for (int i = 0; i < read; i++)
            {
                if (TryParseByte(buf[i], out string? line))
                {
                    return line;
                }
            }
        }
    }

    public async Task<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        _lineBuffer.Clear();

        byte[] buf = new byte[512];

        while (true)
        {
            int read = await _stream.ReadAsync(buf, 0, buf.Length, cancellationToken).ConfigureAwait(false);
            if (read <= 0)
            {
                return null;
            }

            for (int i = 0; i < read; i++)
            {
                if (TryParseByte(buf[i], out string? line))
                {
                    return line;
                }
            }
        }
    }

    /// <summary>
    /// Parses a single incoming byte. Returns true only when a full line has been completed (or connection ended).
    /// The completed line is returned in <paramref name="line"/> when true.
    /// </summary>
    private bool TryParseByte(byte b, out string? line)
    {
        line = null;

        switch (_state)
        {
            case State.Data:
                if (b == IAC)
                {
                    _state = State.Iac;
                    return false;
                }

                if (b == ESC)
                {
                    _state = State.AnsiEscape;
                    return false;
                }

                if (b == (byte)'\r')
                {
                    return false;
                }

                if (b == (byte)'\n')
                {
                    line = _encoding.GetString(_lineBuffer.ToArray());
                    ClearCurrentLine();

                    OnlineCommitted?.Invoke();
                    return true;
                }

                // Handle backspace/delete
                if (b == 0x08 || b == 0x7F)
                {
                    if (_lineBuffer.Count > 0)
                    {
                        _lineBuffer.RemoveAt(_lineBuffer.Count - 1);
                    }
                    return false;
                }

                _lineBuffer.Add(b);
                return false;

            case State.Iac:
                if (b == IAC)
                {
                    // Escaped IAC => literal 0xFF data
                    _lineBuffer.Add(IAC);
                    _state = State.Data;
                    return false;
                }

                if (b == DO || b == DONT || b == WILL || b == WONT)
                {
                    _pendingCommand = b;
                    _state = State.IacCommand;
                    return false;
                }

                if (b == SB)
                {
                    _pendingCommand = SB;
                    _state = State.IacCommand; // next byte is option
                    return false;
                }

                _state = State.Data; // ignore other commands
                return false;

            case State.IacCommand:
                if (_pendingCommand == SB)
                {
                    _subOption = b;
                    _subData.Clear();
                    _state = State.SubNegotiation;
                    return false;
                }

                HandleNegotiation(_pendingCommand, b);
                _state = State.Data;
                return false;

            case State.SubNegotiation:
                if (b == IAC)
                {
                    _state = State.SubIac;
                    return false;
                }

                _subData.Add(b);
                return false;

            case State.SubIac:
                if (b == SE)
                {
                    HandleSubNegotiation(_subOption, _subData);
                    _state = State.Data;
                    return false;
                }

                if (b == IAC)
                {
                    // Escaped IAC inside SB => literal 0xFF in subdata
                    _subData.Add(IAC);
                    _state = State.SubNegotiation;
                    return false;
                }

                // Some other IAC command in SB; ignore and continue SB
                _state = State.SubNegotiation;
                return false;

            case State.AnsiEscape:
                // Most sequences look like ESC [ ... <final>
                // We’ll ignore bytes until we hit a typical terminator.
                // Terminators are commonly in the range '@'..'~' (final byte of CSI sequences).
                if (b >= (byte)'@' && b <= (byte)'~')
                {
                    _state = State.Data;
                }
                return false;

            default:
                _state = State.Data;
                return false;
        }
    }

    private void HandleNegotiation(byte command, byte option)
    {
        // Goal:
        // - Accept SGA and NAWS where appropriate
        // - Prefer client-side echo (accept WILL ECHO, refuse DO ECHO)

        if (command == DO)
        {
            // Client asks server to enable option.
            // If client says DO ECHO, that means "Server, please echo" -> refuse.
            if (option == OPT_SGA)
            {
                SendIac(WILL, option);
            }
            else
            {
                SendIac(WONT, option);
            }
        }
        else if (command == WILL)
        {
            // Client says it will enable option.
            if (option == OPT_SGA || option == OPT_NAWS)
            {
                SendIac(DO, option);
            }
            else if (option == OPT_ECHO)
            {
                // Great: client will echo locally.
                SendIac(DO, option);
            }
            else
            {
                SendIac(DONT, option);
            }
        }
        else if (command == DONT || command == WONT)
        {
            // Acknowledgements / refusals; ignore
        }
    }

    private void HandleSubNegotiation(byte option, List<byte> data)
    {
        if (option == OPT_NAWS && data.Count >= 4)
        {
            // NAWS payload: width hi, width lo, height hi, height lo
            int w = (data[0] << 8) | data[1];
            int h = (data[2] << 8) | data[3];

            if (w > 0 && h > 0)
            {
                TerminalWidth = w;
                TerminalHeight = h;
            }
        }
    }

    public void SetServerEcho(bool enabled)
    {
        // If you ever want password input:
        // enabled == false: ask client to stop local echo (DONT ECHO) and server also won't echo.
        // enabled == true: ask client to echo locally again (DO ECHO).
        //
        // Note: Actual behavior varies by client, but PuTTY usually respects this.

        if (enabled)
        {
            SendIac(DO, OPT_ECHO);   // "Client, please echo locally"
            SendIac(WONT, OPT_ECHO); // "Server won't echo"
        }
        else
        {
            SendIac(DONT, OPT_ECHO); // "Client, stop echoing locally"
            SendIac(WONT, OPT_ECHO); // "Server won't echo either"
        }
    }

    private void SendIac(byte command, byte option)
    {
        byte[] msg = new byte[] { IAC, command, option };
        _stream.Write(msg, 0, msg.Length);
    }
}
