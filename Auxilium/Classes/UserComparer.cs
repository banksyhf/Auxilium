using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Auxilium.Classes
{
    class UserComparer : IComparer<User>
    {
        public int Compare(User x, User y)
        {
            int compareResult = 0;

            compareResult += x.Name.CompareTo(y.Name);
            compareResult += -(x.Rank.CompareTo(y.Rank) * 2);

            if (x.Idle && !y.Idle)
                compareResult += 42;
            else if(!x.Idle && y.Idle)
                compareResult += -42;

            return compareResult;
        }

    }
}
