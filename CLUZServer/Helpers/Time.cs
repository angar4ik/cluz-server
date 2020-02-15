using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZServer.Helpers
{
    public static class Time
    {
        public static bool IsDay(Game g)
        {
            int number = g.TimeFrame;

            if (number % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
