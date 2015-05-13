using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.DataModel
{
    public class JiraTaskTime
    {
        public DateTime DateWhen { get; set; }
        public TimeSpan TimeHowLong { get; set; }

        //public JiraTaskTime()
        //{
        //    DateTime DateWhen = DateTime.Now;
        //    TimeSpan TimeHowLong = new TimeSpan();
        //}
    }
}
