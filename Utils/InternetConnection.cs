using Backend.Events;
using System.Runtime.InteropServices;

namespace Backend.Utils
{
    /// <summary>
    /// This class follows the Singleton pattern and helps check if the computer is connected to the Internet.
    /// It uses the Windows Internet API (WinINet).
    /// </summary>
    public sealed partial class InternetConnection
    {
        private static readonly Lazy<InternetConnection> lazyInstance = new(() => new InternetConnection());

        /// <summary>
        /// Gets the singleton instance of the <see cref="InternetConnection"/> class.
        /// </summary>
        public static InternetConnection Event => lazyInstance.Value;

        [LibraryImport("wininet.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool InternetGetConnectedState(out int description, int reservedValue);

        /// <summary>
        /// Gets or sets a value indicating whether this class should check for Internet connection.
        /// </summary>
        public static bool On { get; set; } = false;

        /// <summary>
        /// Occurs when the Internet connection status has changed.
        /// </summary>
        public event InternetConnectionStatusHandler? InternetStatusChanged;

        /// <summary>
        /// Checks if the machine is connected to the Internet.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the machine is connected to the Internet; otherwise, <c>false</c>.
        /// <para>
        /// <c>IMPORTANT:</c> Always returns <c>true</c> if <see cref="On"/> is set to <c>false</c>.
        /// </para>
        /// </returns>
        public static bool IsConnected() => !On ? true : InternetGetConnectedState(out _, 0);

        /// <summary>
        /// Asynchronously checks if the machine is connected to the Internet.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is <c>true</c> if the machine is connected to the Internet; otherwise, <c>false</c>.
        /// <para>
        /// <c>IMPORTANT:</c> Always returns <c>true</c> if <see cref="On"/> is set to <c>false</c>.
        /// </para>
        /// </returns>
        public static Task<bool> IsConnectedAsync() => Task.FromResult(IsConnected());

        /// <summary>
        /// Performs an infinite loop that checks if the Internet connection has changed.
        /// If a change occurs, it triggers the <see cref="InternetStatusChanged"/> event.
        /// <para>
        /// <c>IMPORTANT:</c> This method does not run if <see cref="On"/> is set to <c>false</c>.
        /// </para>
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
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