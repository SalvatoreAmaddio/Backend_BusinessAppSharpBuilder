using Backend.Model;
using Backend.Utils;
using System.Data.Common;
using System.Data.SQLite;
using System.Data;
using System.Reflection;
using Backend.Exceptions;
using Backend.Events;

namespace Backend.Database
{

    /// <summary>
    /// This class is a concrete definition of <see cref="AbstractDatabase"/> meant for dealing with a SQLite database.
    /// </summary>
    public class SQLiteDatabase : AbstractDatabase
    {

        /// <summary>
        /// Gets and Sets the Version of the Database.
        /// </summary>
        /// <value>A string telling which version of the database is used.</value>
        public string Version { get; set; } = "3";
        /// <summary>
        /// Gets and sets the path to the database, name included.
        /// <para><c>For Example:</c></para>
        /// <code>
        ///  SQLiteDatabase db = new(Employee);
        ///  db.DatabaseName = "Data/mydb.db";
        /// </code>
        /// </summary>
        public override string DatabaseName { get; set; } = "Data/mydb.db";

        public SQLiteDatabase(ISQLModel Model) : base(Model)
        {
            OnConnectionOpenEvent += OnConnectionOpen;
        }

        public SQLiteDatabase(ISQLModel Model, string dbName, string version = "3") : this(Model) 
        {
            DatabaseName = dbName;
            Version = version;
        }

        private void OnConnectionOpen(object? sender, DatabaseEventArgs e)
        {
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
        /// Attempts to create an instance of IDBConnection through the assembly.            
        /// </summary>
        /// <returns>A DbConnection object. If null, it is assumed the application is not published as single file.</returns>
        /// <exception cref="AssemblyCreateInstanceFailure">The Assembly failed to create the instance of an object.</exception>
        private DbConnection? Connector() 
        {
            Assembly? assembly = Sys.LoadedDLL.FirstOrDefault(s => s.Name.Contains("System.Data.SQLite"))?.Assembly;
            if (assembly == null) return null;
            IDbConnection? connection = (IDbConnection?)assembly.CreateInstance("System.Data.SQLite.SQLiteConnection") ?? throw new AssemblyCreateInstanceFailure("Failed to create a connection object from LoadedAssembly");
            connection.ConnectionString = ConnectionString();
            return (DbConnection)connection;
        }

        /// <summary>
        /// If the software is published as SingleFile, the SQLite connection will be established through external Assembly.
        /// On Application' startup, it is important that you call <see cref="Sys.LoadEmbeddedDll(string)"/> by passing the string "System.Data.SQLite" as its argument or call <see cref="Sys.LoadAllEmbeddedDll"/>.
        /// <para><c>IMPORTANT:</c></para> This method does not connect to the Database. 
        /// </summary>
        /// <returns>A DbConnection object</returns>
        public override DbConnection CreateConnectionObject()
        {
            DbConnection? connection = Connector();
            if (connection == null) return new SQLiteConnection(ConnectionString());
            return connection;
        }
    }
}