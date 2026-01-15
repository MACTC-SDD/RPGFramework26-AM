using RPGFramework.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework.Geography
{



    public class Door
    {
        public ExitType Type { get; private set; }
        public string RequiredKeyId { get; private set; }
        public string Destination { get; private set; }
    


    public Door(ExitType type, string destination, string requiredKeyId = null)
        {
            Type = type;
            RequiredKeyId = requiredKeyId;
            Destination = destination;
        }

        public ExitResult TryEnter(Player player)
        {
            switch (Type)
            {
                case ExitType.Open:
                    return ExitResult.Success;
                case ExitType.Door:
                    return ExitResult.Success;
                case ExitType.LockedDoor:
                    if (player.HasItem(RequiredKeyId))
                    {
                        return ExitResult.Success;
                    }
                    else
                    {
                        return ExitResult.MissingKey;
                    }
                case ExitType.Impassable:
                    return ExitResult.Blocked;
                default:
                    return ExitResult.Blocked;
            }
        }
    } }