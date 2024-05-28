namespace Backend.Events
{

    public delegate void InternetConnectionStatusHandler(object? sender, InternetConnectionStatusArgs e);

    public class InternetConnectionStatusArgs : EventArgs
    { 
        public bool IsConnected { get; }
        public string Message => IsConnected ? "" : "No Connection";

        public InternetConnectionStatusArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
