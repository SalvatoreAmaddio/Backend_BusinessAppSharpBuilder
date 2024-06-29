using System.Diagnostics;

namespace Backend.Utils
{
    /// <summary>
    /// Represents a time-tracking utility that extends <see cref="Stopwatch"/>.
    /// Provides a formatted string representation of the elapsed time.
    /// </summary>
    public class TimeTracker : Stopwatch
    {
        /// <summary>
        /// Returns a string that represents the elapsed time in hours, minutes, and seconds.
        /// </summary>
        /// <returns>A string in the format "Elapsed Time: HH hours, MM minutes, SS seconds".</returns>
        public override string ToString()
        {
            TimeSpan elapsedTime = this.Elapsed;
            return $"Elapsed Time: {elapsedTime.Hours} hours, {elapsedTime.Minutes} minutes, {elapsedTime.Seconds} seconds";
        }
    }
}
