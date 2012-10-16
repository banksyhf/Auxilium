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

        public bool Authenticated;

        //I feel ashamed. - Aeonhack
        public void AddPoints(int points)
        {
            Points = Math.Min(Points + points, 1482000);

            //Ranks 38-42 are reserved
            if (Rank > 37)
                return;
            if (Points >= 1406000)
                Rank = 37;
            else if (Points >= 1332000)
                Rank = 36;
            else if (Points >= 1260000)
                Rank = 35;
            else if (Points >= 1190000)
                Rank = 34;
            else if (Points >= 1122000)
                Rank = 33;
            else if (Points >= 1056000)
                Rank = 32;
            else if (Points >= 992000)
                Rank = 31;
            else if (Points >= 930000)
                Rank = 30;
            else if (Points >= 870000)
                Rank = 29;
            else if (Points >= 812000)
                Rank = 28;
            else if (Points >= 756000)
                Rank = 27;
            else if (Points >= 702000)
                Rank = 26;
            else if (Points >= 650000)
                Rank = 25;
            else if (Points >= 600000)
                Rank = 24;
            else if (Points >= 552000)
                Rank = 23;
            else if (Points >= 506000)
                Rank = 22;
            else if (Points >= 462000)
                Rank = 21;
            else if (Points >= 420000)
                Rank = 20;
            else if (Points >= 380000)
                Rank = 19;
            else if (Points >= 342000)
                Rank = 18;
            else if (Points >= 306000)
                Rank = 17;
            else if (Points >= 272000)
                Rank = 16;
            else if (Points >= 240000)
                Rank = 15;
            else if (Points >= 210000)
                Rank = 14;
            else if (Points >= 182000)
                Rank = 13;
            else if (Points >= 156000)
                Rank = 12;
            else if (Points >= 132000)
                Rank = 11;
            else if (Points >= 110000)
                Rank = 10;
            else if (Points >= 90000)
                Rank = 9;
            else if (Points >= 72000)
                Rank = 8;
            else if (Points >= 56000)
                Rank = 7;
            else if (Points >= 42000)
                Rank = 6;
            else if (Points >= 30000)
                Rank = 5;
            else if (Points >= 20000)
                Rank = 4;
            else if (Points >= 12000)
                Rank = 3;
            else if (Points >= 6000)
                Rank = 2;
            else if (Points >= 2000)
                Rank = 1;
        }

    }
}
