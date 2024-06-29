using System.Data.Common;
using Backend.Enums;

namespace Backend.Events
{
    /// <summary>
    /// Occurs before a record is deleted.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    public delegate void BeforeRecordDeleteEventHandler(object? sender, EventArgs e);

    /// <summary>
    /// Occurs after a record is deleted.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    public delegate void AfterRecordDeleteEventHandler(object? sender, EventArgs e);

    /// <summary>
    /// Occurs when a database connection is opened.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="DatabaseEventArgs"/> that contains the event data.</param>
    public delegate void OnDatabaseConnectionOpen(object? sender, DatabaseEventArgs e);

    /// <summary>
    /// Occurs when the internet connection status changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="InternetConnectionStatusArgs"/> that contains the event data.</param>
    public delegate void InternetConnectionStatusHandler(object? sender, InternetConnectionStatusArgs e);

    /// <summary>
    /// Occurs before record navigation.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="AllowRecordMovementArgs"/> that contains the event data.</param>
    public delegate void BeforeRecordNavigationEventHandler(object? sender, AllowRecordMovementArgs e);

    /// <summary>
    /// Occurs after record navigation.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="AllowRecordMovementArgs"/> that contains the event data.</param>
    public delegate void AfterRecordNavigationEventHandler(object? sender, AllowRecordMovementArgs e);

    /// <summary>
    /// Provides data for the record movement event.
    /// </summary>
    public class AllowRecordMovementArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllowRecordMovementArgs"/> class.
        /// </summary>
        /// <param name="movement">The type of record movement.</param>
        public AllowRecordMovementArgs(RecordMovement movement)
        {
            Movement = movement;
        }

        /// <summary>
        /// Gets the type of record movement.
        /// </summary>
        public RecordMovement Movement { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the record movement should be canceled.
        /// </summary>
        public bool Cancel { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether the movement is to a new record.
        /// </summary>
        public bool NewRecord => Movement == RecordMovement.GoNew;

        /// <summary>
        /// Gets a value indicating whether the movement is to the first record.
        /// </summary>
        public bool GoFirst => Movement == RecordMovement.GoFirst;

        /// <summary>
        /// Gets a value indicating whether the movement is to the next record.
        /// </summary>
        public bool GoNext => Movement == RecordMovement.GoNext;

        /// <summary>
        /// Gets a value indicating whether the movement is to the last record.
        /// </summary>
        public bool GoLast => Movement == RecordMovement.GoLast;

        /// <summary>
        /// Gets a value indicating whether the movement is to the previous record.
        /// </summary>
        public bool GoPrevious => Movement == RecordMovement.GoPrevious;
    }

    /// <summary>
    /// An abstract base class for event data that includes messages.
    /// </summary>
    public abstract class AbstractEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets an array of messages associated with the event.
        /// </summary>
        public string[] Messages = new string[0];

        /// <summary>
        /// Gets a value indicating whether there are any messages.
        /// </summary>
        public bool HasMessages => Messages.Length > 0;

        /// <summary>
        /// Determines whether the message at the specified index matches the given value.
        /// </summary>
        /// <param name="index">The index of the message.</param>
        /// <param name="value">The value to compare with the message.</param>
        /// <returns>true if the message matches the value; otherwise, false.</returns>
        public bool MessageIs(int index, string value) => this[index].Equals(value);

        /// <summary>
        /// Gets the message at the specified index.
        /// </summary>
        /// <param name="index">The index of the message.</param>
        /// <returns>The message at the specified index.</returns>
        public string this[int index] => Messages[index];
    }

    /// <summary>
    /// Provides data for the database connection event.
    /// </summary>
    public class DatabaseEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventArgs"/> class.
        /// </summary>
        /// <param name="connection">The database connection associated with the event.</param>
        /// <param name="crud">The CRUD operation associated with the event.</param>
        public DatabaseEventArgs(DbConnection connection, CRUD crud)
        {
            Connection = connection;
            Crud = crud;
        }

        /// <summary>
        /// Gets the CRUD operation associated with the event.
        /// </summary>
        public CRUD Crud { get; }

        /// <summary>
        /// Gets the database connection associated with the event.
        /// </summary>
        public DbConnection Connection { get; }
    }

    /// <summary>
    /// Provides data for the internet connection status event.
    /// </summary>
    public class InternetConnectionStatusArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternetConnectionStatusArgs"/> class.
        /// </summary>
        /// <param name="isConnected">A value indicating whether the internet is connected.</param>
        public InternetConnectionStatusArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }

        /// <summary>
        /// Gets a value indicating whether the internet is connected.
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// Gets a message indicating the internet connection status.
        /// </summary>
        public string Message => IsConnected ? "" : "No Connection";
    }

}
