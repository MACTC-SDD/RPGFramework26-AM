using RPGFramework.Display;
using RPGFramework.Enums;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RPGFramework.Geography
{
    internal static class MapRenderer
    {
        // North = y - 1, South = y + 1, East = x + 1, West = x - 1
        private static readonly Dictionary<Direction, (int dx, int dy)> Offsets = new()
        {
            { Direction.North, (0, -1) },
            { Direction.South, (0,  1) },
            { Direction.East,  (1,  0) },
            { Direction.West,  (-1, 0) },
        };

        public static void RenderLocalMap(Player player, int radius = 2)
        {
            var area = GameState.Instance.Areas[player.AreaId];
            var startRoom = player.GetRoom();

            // (x, y) -> LocalMapCell
            var cells = new Dictionary<(int x, int y), LocalMapCell>();

            // RoomId -> already visited
            var visited = new HashSet<int>();

            var queue = new Queue<(Room room, int x, int y, int dist)>();
            queue.Enqueue((startRoom, 0, 0, 0));
            visited.Add(startRoom.Id);

            while (queue.Count > 0)
            {
                var (room, x, y, dist) = queue.Dequeue();

                // Store cell for this room
                cells[(x, y)] = new LocalMapCell
                {
                    RoomId = room.Id,
                    IsPlayerHere = room.Id == startRoom.Id,
                    MapIcon = $"{room.MapColor}{room.MapIcon}[/]"
                };

                if (dist == radius)
                {
                    continue;
                }

                // Walk exits in cardinal directions only
                foreach (var exit in room.GetExits())
                {
                    if (!Offsets.TryGetValue(exit.ExitDirection, out var delta))
                        continue;

                    var nextRoom = area.Rooms[exit.DestinationRoomId];
                    if (visited.Contains(nextRoom.Id))
                        continue;

                    var nx = x + delta.dx;
                    var ny = y + delta.dy;

                    visited.Add(nextRoom.Id);
                    queue.Enqueue((nextRoom, nx, ny, dist + 1));
                }
            }

            RenderGrid(player, cells, radius);
        }

        private static void RenderGrid(Player player, Dictionary<(int x, int y), LocalMapCell> cells, int radius)
        {
            var table = new Table()
                .Centered()
                .Border(TableBorder.None)
                .HideHeaders();

            int size = radius * 2 + 1;
            // 
            for (int i = 0; i < size; i++)
            {
                table.AddColumn(new TableColumn(string.Empty).Centered());
            }

            // y: -radius (north, top) to +radius (south, bottom)
            for (int y = -radius; y <= radius; y++)
            {
                var row = new List<IRenderable>();

                for (int x = -radius; x <= radius; x++)
                {
                    if (cells.TryGetValue((x, y), out var cell))
                    {
                        // Player in this room
                        if (cell.IsPlayerHere)
                        {
                            row.Add(new Markup($"{DisplaySettings.YouAreHereMapIconColor} "
                            + $"{DisplaySettings.YouAreHereMapIcon} [/]").Centered());
                        }
                        else
                        {
                            // A discovered room
                            row.Add(new Markup(cell.MapIcon).Centered());
                        }
                    }
                    else
                    {
                        // Unknown / no room here
                        row.Add(new Markup(" ").Centered());
                    }
                }

                table.AddRow(row.ToArray());
            }

            // Optional title / legend
            var panel = new Panel(table)
            {
                Header = new PanelHeader($"Local Map ({size}x{size})"),
                Border = BoxBorder.Rounded
            };

            player.Write(panel);
            player.WriteLine("[bold black on yellow] @ [/]: You   [green]■[/]: Room   (blank): Unknown");
        }

        public sealed class LocalMapCell
        {
            public int RoomId { get; init; }
            public bool IsPlayerHere { get; init; }
            public string MapIcon { get; init; }
        }
    }
}
