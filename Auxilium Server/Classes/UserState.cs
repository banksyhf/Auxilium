using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auxilium_Server.Classes
{
    class UserState
    {
        public string Username;
        public bool Authenticated;
        public ushort UserID;
        public byte Channel;
    }
}
