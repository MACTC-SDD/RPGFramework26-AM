using RPGFramework.Enums;
using System.ComponentModel;

namespace RPGFramework
{
    internal class Mob : NonPlayer
    {
        public int ChaseDistance { get; private set; } = 0;
        public int MaxRoomsToChase { get; private set; } = 5;

        // CODE REVIEW: Shelton - PR #18
        // We probable don't need a constructor that takes all these parameters since we can just
        // set the properties after creating the object with the default constructor.
        // I adjusted the constructors to not use base, but the reference properties that don't exist
        //    LocationID and DialogOptions are commented out.
        // I adjusted parameters to be all camelCase.
        // I don't think we want to use : base(...) here since NonPlayer doesn't have a matching constructor.
        public Mob()
        {

        }

        public Mob(string name, string desc, int level, Dictionary<string, string[]> dialogOptions, int locationId)
        {
            Name = name;
            Description = desc;
            Level = level;
            //LocationID = locationId; // Not sure what this is for (Area/Room?)
            //DialogOptions = dialogOptions;
        }

        public Mob(string name, string desc, int level, Dictionary<string, string[]> dialogOptions, int maxRoomsToChase,
            int locationID)
        {
            Name = name;
            Description = desc;
            Level = level;
            //LocationID = locationID; // Not sure what this is for (Area/Room?)
            //DialogOptions = dialogOptions;
            MaxRoomsToChase = maxRoomsToChase;
        }

        

        public bool CanChase()
        {
            return ChaseDistance < MaxRoomsToChase;
        }

    }
}