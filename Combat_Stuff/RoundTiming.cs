using System;
using System.Collections.Generic;
using System.Text;

namespace RPGFramework.Combat_Stuff
{
    //this will go in the game state area most likely
    internal class RoundTiming
    {
        public static int roundTime { get; set; } = 5;
        public static int maxRounds { get; set; } = 50;
        public static int currentRounds { get; set; } = 0;
        public void RoundTimer()
        {
            while (roundTime > 0)
            {
                roundTime--;
                Thread.Sleep(1000);
            }
            if (roundTime <= 0)
            {
                currentRounds++;
                if (currentRounds == maxRounds)
                {
                    return;
                }
                RoundTimer();
            }
        }
    } 
}