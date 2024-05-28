using System.Diagnostics;

namespace Backend.Utils
{
    public class TimeTracker : Stopwatch
    {
        public override string ToString()
        {
            TimeSpan elapsedTime = this.Elapsed;
            return $"Elapsed Time: {elapsedTime.Hours} hours, {elapsedTime.Minutes} minutes, {elapsedTime.Seconds} seconds";
        }
    }
}
