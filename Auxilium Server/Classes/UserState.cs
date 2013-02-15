using System;
using System.Collections.Generic;

namespace Auxilium_Server.Classes
{
    class UserState
    {
        public ushort UserId;
        public string Username;
        public string Email;

        public int Percentage;
        public int ExperienceRequired;
        public int Points;
        public byte Rank;

        public List<Client> Mute;
        public byte Channel;

        public bool Idle;
        public DateTime LastAction;
        public DateTime LastPayout;

        public byte PacketCount;
        public DateTime LastPacket;

        public bool Authenticated;
        public bool Verified;

        public void AddPoints(int points)
        {
            Points = Math.Min(Points + points, 2812000);

            if (Rank >= 37)
            {
                Percentage = 100;
                ExperienceRequired = 0;
                return;
            }

            int nextRank = ((Rank / 2) + 1) * (Rank + 1) * 4000;

            int previousRank = (((Rank - 1) / 2) + 1) * Rank * 4000;

            if (Points >= nextRank)
            {
                Rank += 1;
            }

            Percentage = (int)Math.Round((((double)Points - (double)previousRank) / ((double)nextRank - (double)previousRank)) * 100);

            ExperienceRequired = nextRank - Points;
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
