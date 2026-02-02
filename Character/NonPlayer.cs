
using RPGFramework.Commands;
using RPGFramework.Enums;
using RPGFramework.Geography;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices.Swift;
using System.Transactions;

namespace RPGFramework
{
    /// <summary>
    /// Represents a character in the game that is not controlled by a player.
    /// </summary>
    /// <remarks>Non-player characters (NPCs) may serve various roles such as quest givers, merchants, or
    /// enemies.</remarks>
    internal class NonPlayer : Character
    {
        //additional variables from NPCs
        public List<DialogGroup> DialogGroups { get; protected set; } = new List<DialogGroup>();
        public int CurrentAggressionLevel { get; protected set; } = 0;
        public int MaxAggressionLevel { get; protected set; } = 10;
        public int MinAgressionLevel { get; protected set; } = 0;
        public bool Spawned { get; set; } = false;
        public NonPlayerType NpcType { get; protected set; } = NonPlayerType.Default;
        public CharacterState CurrentState { get; protected set; } = CharacterState.Idle;
        public NonPlayer()
        {
        }

        public void IncrementAgressionLevel(int amount)
        {
            if(amount + CurrentAggressionLevel > MaxAggressionLevel)
            {
                CurrentAggressionLevel = MaxAggressionLevel;
                return;
            }
            else if(amount + CurrentAggressionLevel < MinAgressionLevel)
            {
                CurrentAggressionLevel = MinAgressionLevel;
                return;
            }
            CurrentAggressionLevel += amount;
        }

        #region ---- Behavior Methods ----

        //npc leaves the room into a random room.
        private void NpcLeaveRoom(Character npc)
        {
            // Implement leave room logic here
            List<Exit> exits = GetExits();
            int exitId = random.Next(exits.Count);
            Direction choice = exits.ElementAt(exitId).ExitDirection;

            Navigation.Move(npc,choice);
            GetRoom();
            return;
        }

        //Plays when the npc is in an idle state
        public void PerformIdleBehavior()
        {
            // Implement idle behavior logic here
            int speakingChance = random.Next(1, 20);
            NpcSpeakingChance(speakingChance);
            //save for last option (so it cant talk after it leaves)
            int LeavingChance = random.Next(1, 20);
            NpcMovementChance(LeavingChance);
        }

        //moves if the npc gets a high enough random number.
        private void NpcMovementChance(int number)
        {
            if (Tags.Contains("Wanderer"))
            {
                number += 2;
            }
            if(number >= 18)
            {
                NpcLeaveRoom(this);

            }
            return;
        }

        private void NpcSpeakingChance(int number)
        {
            if (Tags.Contains("Talkative"))
            {
                number += 2;
            }
            if (number >= 15)
            {
                NpcSpeakRandomly();
            }
            return;
        }

        private void NpcSpeakRandomly()
        {
            if (DialogGroups.Count == 0)
            {
                return;
            }
            int index = random.Next(DialogGroups.Count);
            string selectedLine = DialogGroups[index].GetRandomDialogLine();
            Comm.SendToRoom(GetRoom(), $"{Name} says: \"{selectedLine}\"");
        }
        public void AddDialogGroup(DialogGroup group)
        {
            DialogGroups.Add(group);
        }
        public void RemoveDialogGroup(DialogGroup group)
        {
            DialogGroups.Remove(group);
        }
        public bool HasDialogGroup(string groupName)
        {
            return DialogGroups.Any(g => string.Equals(g.GroupName, groupName, StringComparison.OrdinalIgnoreCase));
        }
        public DialogGroup GetDialogGroup(string groupName)
        {
            return DialogGroups.FirstOrDefault(g => string.Equals(g.GroupName, groupName, StringComparison.OrdinalIgnoreCase));
        }
        #endregion
    }
}
