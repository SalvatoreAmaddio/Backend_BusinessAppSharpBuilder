using System.Data.Common;
using Backend.Enums;

namespace Backend.Events
{

    public delegate void OnDatabaseConnectionOpen(object? sender, DatabaseEventArgs e);
    public delegate void InternetConnectionStatusHandler(object? sender, InternetConnectionStatusArgs e);
    public delegate void BeforeRecordNavigationEventHandler(object? sender, AllowRecordMovementArgs e);
    public delegate void AfterRecordNavigationEventHandler(object? sender, AllowRecordMovementArgs e);

    public enum RecordMovement
    {
        GoFirst = 1,
        GoLast = 2,
        GoNext = 3,
        GoPrevious = 4,
        GoNew = 5,
        GoAt = 6,
    }

    public class AllowRecordMovementArgs(RecordMovement movement) : EventArgs
    {
        public RecordMovement Movement { get; } = movement;

        /// <summary>
        /// If sets to True, the Form will not move to another record.
        /// </summary>
        public bool Cancel { get; set; } = false;
        public bool NewRecord => Movement == RecordMovement.GoNew;
        public bool GoFirst => Movement == RecordMovement.GoFirst;
        public bool GoNext => Movement == RecordMovement.GoNext;
        public bool GoLast => Movement == RecordMovement.GoLast;
        public bool GoPrevious => Movement == RecordMovement.GoPrevious;
    }

    public abstract class AbstractEventArgs : EventArgs
    {
        public string[] Messages = [];

        public bool HasMessages => Messages.Length > 0;

        public bool MessageIs(int index, string value) => this[index].Equals(value);

        public string this[int index]
        {
            get { return Messages[index]; }
        }
    }
    public class DatabaseEventArgs(DbConnection connection, CRUD crud) : EventArgs 
    { 
        public CRUD Crud { get; private set; } = crud;
        public DbConnection Connection { get; private set; } = connection;
    }
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
