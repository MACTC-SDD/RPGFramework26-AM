using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework.Enums
{
    public enum ExitType
    {
        Open,
        Door,
        LockedDoor,
        Impassable,

    }
    public enum ExitResult
    {
        Success,
        Locked,
        Blocked,
        MissingKey,
    }
}
