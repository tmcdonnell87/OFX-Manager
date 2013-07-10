using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFXManager
{
    public struct DateHelper
    {
        public static DateTime Tomorrow
        {
            get
            {
                return DateTime.Today.AddDays(1);
            }
        }

        public static DateTime MonthBeginning
        {
            get
            {
                return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }
        }

        public static DateTime RecentMonthStart
        {
            get
            {
                if (DateTime.Today.Day < 7) return MonthBeginning.AddMonths(-1);
                return MonthBeginning;
            }


        }
    }
}
