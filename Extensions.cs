using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitor
{
    public static class Extensions
    {
        public static bool isMarkedForRemoval(Process process, uint maximumLifeTime)
        {
            if (process == null) throw new ArgumentNullException($"The given process does not exist!");
            if (maximumLifeTime == 0) throw new ArgumentNullException($"The maximum lifetime or monitor frequency passed it's not valid!");

            var processStartTime = process.StartTime;
            var currentTime = DateTime.Now;

            var passedTime = currentTime - processStartTime;
            var minutesPassed = passedTime.TotalMinutes;

            if (minutesPassed >= maximumLifeTime) return true;
            else return false;
        }
    }
}
