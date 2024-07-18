using Backend.Model;
using Backend.Utils;
using System.Data.Common;
using System.Data.SQLite;
using System.Data;
using System.Reflection;
using Backend.Exceptions;
using Backend.Events;
using Backend.Enums;

namespace Backend.Database
{
    /// <summary>
    /// This class is a concrete definition of <see cref="AbstractDatabase{M}"/> meant for dealing with a SQLite database.
    /// </summary>
    /// <typeparam name="M">A type that implements the <see cref="ISQLModel"/> interface and has a parameterless constructor.</typeparam>
    public class SQLiteDatabase<M> : AbstractDatabase<M> where M : ISQLModel, new()
    {
        /// <summary>
        /// Gets or sets the version of the database.
        /// </summary>
        /// <value>A string indicating which version of the database is used.</value>
        public string Version { get; set; } = "3";

        /// <summary>
        /// Gets or sets the path to the database, including the name.
        /// <para><c>For Example:</c></para>
        /// <code>
        /// SQLiteDatabase&lt;Student> db = new();
        /// db.DatabaseName = "Data\mydb.db";
        /// </code>
        /// </summary>
        public override string DatabaseName { get; set; } = "mydb.db";

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteDatabase{M}"/> class.
        /// </summary>
        public SQLiteDatabase()
        {
            OnConnectionOpenEvent += OnConnectionOpen;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteDatabase{M}"/> class with a specified database name and version.
        /// </summary>
        /// <param name="dbName">The name of the database.</param>
        /// <param name="version">The version of the database.</param>
        public SQLiteDatabase(string dbName, string version = "3")
        {
            DatabaseName = dbName;
            Version = version;
        }

        /// <summary>
        /// Handles the event when the database connection is opened.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The <see cref="DatabaseEventArgs"/> instance containing the event data.</param>
        private void OnConnectionOpen(object? sender, DatabaseEventArgs e)
        {
            using (var command = e.Connection.CreateCommand())
            {
                command.CommandText = "PRAGMA journal_mode=WAL;";
                command.ExecuteNonQuery();
            }

            if (e.Crud != CRUD.DELETE) return;

            using (var command = e.Connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }
        }

        public override string ConnectionString() => $"Data Source={DatabaseName};Version={Version};";

        protected override string LastIDQry() => "SELECT last_insert_rowid()";

        /// <summary>
        /// Attempts to create an instance of <see cref="DbConnection"/> through the assembly.
        /// </summary>
        /// <returns>A <see cref="DbConnection"/> object. If null, it is assumed the application is not published as a single file.</returns>
        /// <exception cref="AssemblyCreateInstanceFailure">Thrown when the assembly fails to create the instance of a connection object.</exception>
        private DbConnection? Connector()
        {
            Assembly? assembly = Sys.LoadedDLL.FirstOrDefault(s => s.Name.Contains("System.Data.SQLite"))?.Assembly;
            if (assembly == null) return null;

            IDbConnection? connection = (IDbConnection?)assembly.CreateInstance("System.Data.SQLite.SQLiteConnection") ?? throw new AssemblyCreateInstanceFailure("Failed to create a connection object from LoadedAssembly");
            connection.ConnectionString = ConnectionString();
            return (DbConnection)connection;
        }

        public override DbConnection CreateConnectionObject()
        {
            DbConnection? connection = Connector();
            if (connection == null) return new SQLiteConnection(ConnectionString());
            return connection;
        }

        /// <summary>
        /// Returns a string representation of the SQLite database.
        /// </summary>
        /// <returns>A string representing the SQLite database.</returns>
        public override string ToString() => $"SQLiteDatabase<{ModelType.Name}>";
    }
}