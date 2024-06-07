using Backend.Database;
using System.Data.Common;

namespace Backend.Events
{

    public delegate void OnDatabaseConnectionOpen(object? sender, DatabaseEventArgs e);
    public delegate void InternetConnectionStatusHandler(object? sender, InternetConnectionStatusArgs e);

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
