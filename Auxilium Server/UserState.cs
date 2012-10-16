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

        //I feel ashamed. - Aeonhack
        public void AddPoints(int points)
        {
            Points = Math.Min(Points + points, 2812000);

            if (Rank >= 37)
                return;

            if (Points >= 2812000)
                Rank = 37;
            else if (Points >= 2664000)
                Rank = 36;
            else if (Points >= 2520000)
                Rank = 35;
            else if (Points >= 2380000)
                Rank = 34;
            else if (Points >= 2244000)
                Rank = 33;
            else if (Points >= 2112000)
                Rank = 32;
            else if (Points >= 1984000)
                Rank = 31;
            else if (Points >= 1860000)
                Rank = 30;
            else if (Points >= 1740000)
                Rank = 29;
            else if (Points >= 1624000)
                Rank = 28;
            else if (Points >= 1512000)
                Rank = 27;
            else if (Points >= 1404000)
                Rank = 26;
            else if (Points >= 1300000)
                Rank = 25;
            else if (Points >= 1200000)
                Rank = 24;
            else if (Points >= 1104000)
                Rank = 23;
            else if (Points >= 1012000)
                Rank = 22;
            else if (Points >= 924000)
                Rank = 21;
            else if (Points >= 840000)
                Rank = 20;
            else if (Points >= 760000)
                Rank = 19;
            else if (Points >= 684000)
                Rank = 18;
            else if (Points >= 612000)
                Rank = 17;
            else if (Points >= 544000)
                Rank = 16;
            else if (Points >= 480000)
                Rank = 15;
            else if (Points >= 420000)
                Rank = 14;
            else if (Points >= 364000)
                Rank = 13;
            else if (Points >= 312000)
                Rank = 12;
            else if (Points >= 264000)
                Rank = 11;
            else if (Points >= 220000)
                Rank = 10;
            else if (Points >= 180000)
                Rank = 9;
            else if (Points >= 144000)
                Rank = 8;
            else if (Points >= 112000)
                Rank = 7;
            else if (Points >= 84000)
                Rank = 6;
            else if (Points >= 60000)
                Rank = 5;
            else if (Points >= 40000)
                Rank = 4;
            else if (Points >= 24000)
                Rank = 3;
            else if (Points >= 12000)
                Rank = 2;
            else if (Points >=4000)
                Rank = 1;
        }

        public bool IsFlooding()
        {
            PacketCount += 1;
            if (PacketCount == 10)
                return true;

            if ((DateTime.Now - LastPacket).TotalSeconds == 3)
                PacketCount = 0;

            LastPacket = DateTime.Now;
            return false;
        }
    }
}
