using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auxilium_Server.Classes
{
    class UserState
    {
        public ushort UserID;
        public string Username;

        public int Percentage;
        public int ExperienceRequired;
        public int Points;
        public byte Rank;

        public bool Mute;
        public byte Channel;

        public bool Idle;
        public DateTime LastAction;
        public DateTime LastPayout;

        public byte PacketCount;
        public DateTime LastPacket;

        public bool Authenticated;

        public void AddPoints(int points)
        {
            Points = Math.Min(Points + points, 2812000);

            if (Rank >= 37)
            {
                Percentage = 100;
                ExperienceRequired = 0;
                return;
            }

            int NextRank = ((Rank / 2) + 1) * (Rank + 1) * 4000;

            if (Points >= NextRank)
            {
                Rank += 1;
            }

            Percentage = (int)Math.Round(((double)Points / (double)NextRank) * 100);

            ExperienceRequired = NextRank - Points;
        }

        public bool IsFlooding()
        {
            PacketCount += 1;
            if (PacketCount == 10)
                return true;

            if ((DateTime.Now - LastPacket).TotalSeconds >= 3)
                PacketCount = 0;

            LastPacket = DateTime.Now;
            return false;
        }
    }
}
