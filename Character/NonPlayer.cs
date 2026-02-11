using RPGFramework.Commands;
using RPGFramework.Enums;
using RPGFramework.Geography;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

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
        [JsonInclude] public List<DialogGroup> DialogGroups { get; protected set; } = [];
        public int CurrentAggressionLevel { get; protected set; } = 0;
        [JsonInclude] public int MaxAggressionLevel { get; protected set; } = 10;
        [JsonInclude] public int MinAgressionLevel { get; protected set; } = 0;
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

        public void PerformBehavior()
        {
            CheckAggressionLevel();
            switch (CurrentState)
            {
                case CharacterState.Idle:
                    PerformIdleBehavior();
                    break;
                case CharacterState.Aggressive:
                    PerformAggressiveBehavior();
                    break;
                default:
                    break;
            }
        }

        public void CheckPlayerDialogue(string playerDialogue)
        {
            foreach(var group in DialogGroups)
            {
                if(group.CheckKeywordsInText(playerDialogue))
                {
                    if(group.Category == DialogGroupCategory.Aggressive)
                    {
                        CheckAggressionTags();
                    }
                    string response = group.GetRandomDialogLine();
                    Comm.SendToRoomExcept(GetRoom(), $"{Name} says: \"{response}\"", this);
                    return;
                }
            }
            CheckAggressionLevel();
        }

        public void CheckAggressionTags()
        {
            if (Tags.Contains(NPCTag.Hostile))
            {
                IncrementAgressionLevel(3);
            }
            else if (Tags.Contains(NPCTag.Peaceful))
            {
                IncrementAgressionLevel(1);
            }
            else
            {
                IncrementAgressionLevel(2);
            }
            CheckAggressionLevel();

        }
        protected void CheckAggressionLevel()
        {
            if(CurrentAggressionLevel >= MaxAggressionLevel * 0.7)
            {
                CurrentState = CharacterState.Aggressive;
            }
            else
            {
                CurrentState = CharacterState.Idle;
            }
        }

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
        protected void PerformIdleBehavior()
        {
            // Implement idle behavior logic here
            int speakingChance = random.Next(1, 20);
            NpcSpeakingChance(speakingChance);
            //save for last option (so it cant talk after it leaves)
            int LeavingChance = random.Next(1, 20);
            NpcMovementChance(LeavingChance);
        }

        //Plays when the npc is in an aggressive state
        protected void PerformAggressiveBehavior()
        {
            int number = random.Next(1, 20);
            if (Tags.Contains(NPCTag.Talkative) || Tags.Contains(NPCTag.Hostile))
            {
                number += 2;
            }
            if(number >= 18)
            {
                //Implement Attack Logic later
            }
            if (number >= 12)
            {
                NpcSpeakRandomly("aggresive");
            }
                return;
        }

        //moves if the npc gets a high enough random number.
        private void NpcMovementChance(int number)
        {
            if (Tags.Contains(NPCTag.Wanderer))
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
            if (Tags.Contains(NPCTag.Talkative))
            {
                number += 2;
            }
            if (number >= 15)
            {
                NpcSpeakRandomly("Idle");
            }
            return;
        }

        private void NpcSpeakRandomly(string type)
        {
            if (DialogGroups.Count == 0)
            {
                return;
            }

            DialogGroup? typeGroup = GetDialogGroup(type);

            if(typeGroup == null || typeGroup.DialogLines.Count == 0)
            {
                return;
            }

            string selectedLine = typeGroup.GetRandomDialogLine();
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
        public bool HasDialogGroup(DialogGroupCategory groupCategory)
        {
            return DialogGroups.Any(o => o.Category == groupCategory);
        }

        public DialogGroup GetDialogGroup(string groupName)
        {
            DialogGroupCategory category;
            Enum.TryParse<DialogGroupCategory>(groupName, true, out category);
            foreach (var group in DialogGroups)
            {
                if (group.Category == category)
                {
                    return group;
                }
            }
            return null;
        }
        #endregion
    }
}
