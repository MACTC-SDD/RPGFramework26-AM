using RPGFramework.Enums;

namespace RPGFramework.Geography
{
    public class DoorBaseBase
    {
        public ExitType ExitType { get; private set; }
        public object RequiredKeyId { get; private set; }

        public ExitResult TryUse(Player player)
        {
            switch (ExitType)
            {
                case ExitType.Open:
                case ExitType.Door:
                    return ExitResult.Success;

                case ExitType.LockedDoor:
                    if (RequiredKeyId != null && player.HasItem(RequiredKeyId))
                    {
                        ExitType = ExitType.Door; // unlock permanently
                        return ExitResult.Success;
                    }
                    return ExitResult.Locked;

                case ExitType.Impassable:
                    return ExitResult.Blocked;

                default:
                    return ExitResult.Blocked;
            }
        }
    }
}