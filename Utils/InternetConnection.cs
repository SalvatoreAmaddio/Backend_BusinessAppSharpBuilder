using Backend.Events;
using System.Runtime.InteropServices;

namespace Backend.Utils
{
    /// <summary>
    /// This class follows the Singleton pattern. 
    /// It helps check if the computer is connected to the Internet or not.
    /// This class uses Windows Internet API. (WinINet)
    /// </summary>
    public sealed partial class InternetConnection
    {
        private static readonly Lazy<InternetConnection> lazyInstance = new(() => new InternetConnection());
        public static InternetConnection Event => lazyInstance.Value;

        [LibraryImport("wininet.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool InternetGetConnectedState(out int description, int reservedValue);

        /// <summary>
        /// Gets or Sets if this class should be checking for Iternet Connection or not.
        /// </summary>
        public static bool On { get; set; } = false;

        /// <summary>
        /// Occurs when the Internet Connection status has changed.
        /// </summary>
        public event InternetConnectionStatusHandler? InternetStatusChanged;

        /// <summary>
        /// This method check if the machine is connected to the Internet.
        /// </summary>
        /// <returns>
        /// true if the machine is connected to the internet <para/>
        /// <c>IMPORTANT:</c> Returns always true if <see cref="On"/> is set to false.
        /// </returns>
        public static bool IsConnected() => !On ? true : InternetGetConnectedState(out _, 0);

        /// <summary>
        /// This method check if the machine is connected to the Internet.
        /// </summary>
        /// <returns>
        /// A Task&lt;bool> <para/>
        /// <c>IMPORTANT:</c> Returns always Task&lt;true> if <see cref="On"/> is set to false.
        /// </returns>
        public static Task<bool> IsConnectedAsync() => Task.FromResult(IsConnected());

        /// <summary>
        /// This method perform an infinite loop that checks if the Internet Connection has changed.
        /// If a change occured, it triggers the <see cref="InternetStatusChanged"/> event.
        /// <para/>
        /// <c>IMPORTANT:</c> this method does not run if <see cref="On"/> is set to false.
        /// </summary>
        /// <returns>A Task</returns>
        public async static Task CheckingInternetConnection()
        {
            if (!On) return;
            bool initialStatusCheck = IsConnected();
            while (true)
            {
                await Task.Run(() =>
                {
                    bool nextStatusCheck = IsConnected();
                    if (initialStatusCheck != nextStatusCheck)
                    {
                        initialStatusCheck = nextStatusCheck;
                        lazyInstance.Value.InternetStatusChanged?.Invoke(lazyInstance.Value, new(initialStatusCheck));
                    }
                });
            }
        }
    }
}