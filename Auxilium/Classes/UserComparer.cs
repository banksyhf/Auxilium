using System.Collections.Generic;

namespace Auxilium.Classes
{
    class UserComparer : IComparer<User>
    {
        public int Compare(User x, User y)
        {
            int compareResult = 0;

            compareResult += string.Compare(x.Name, y.Name, System.StringComparison.Ordinal);
            compareResult += -(x.Rank.CompareTo(y.Rank) * 2);

            if (x.Idle && !y.Idle)
                compareResult += 42;
            else if(!x.Idle && y.Idle)
                compareResult += -42;

            return compareResult;
        }

    }
}
