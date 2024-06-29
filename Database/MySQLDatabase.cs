using Backend.Exceptions;
using Backend.Model;
using Backend.Utils;
using MySqlConnector;
using System.Data.Common;

namespace Backend.Database
{
    /// <summary>
    /// This structure provides bits of information used for building a Connection String for a MySQL database.
    /// This structure is used by the <see cref="MySQLDatabase{M}"/>'s constructor.
    /// </summary>
    public struct MySQLConnectionStringInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySQLConnectionStringInfo"/> struct.
        /// </summary>
        /// <param name="username">The username for the MySQL database connection.</param>
        /// <param name="server">The server address for the MySQL database connection. Defaults to "localhost".</param>
        /// <param name="port">The port for the MySQL database connection. Defaults to "3306".</param>
        public MySQLConnectionStringInfo(string username, string server = "localhost", string port = "3306")
        {
            UsernName = username;
            Server = server;
            Port = port;
        }

        /// <summary>
        /// Gets or sets the username for the MySQL database connection.
        /// </summary>
        public string UsernName { get; set; }

        /// <summary>
        /// Gets or sets the port for the MySQL database connection.
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// Gets or sets the server address for the MySQL database connection.
        /// </summary>
        public string Server { get; set; }
    }

    /// <summary>
    /// This class is a concrete definition of <see cref="AbstractDatabase{M}"/> meant for dealing with a MySQL database.
    /// </summary>
    /// <typeparam name="M">A type that implements the <see cref="ISQLModel"/> interface and has a parameterless constructor.</typeparam>
    public class MySQLDatabase<M> : AbstractDatabase<M> where M : ISQLModel, new()
    {
        /// <summary>
        /// Gets or sets the username for the MySQL database connection.
        /// </summary>
        public string UsernName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the port for the MySQL database connection.
        /// </summary>
        public string Port { get; set; } = "3306";

        /// <summary>
        /// Gets or sets the server address for the MySQL database connection.
        /// </summary>
        public string Server { get; set; } = "localhost";

        private string MySQL_SECRET_KEY => $"{Server}_MySQL_{UsernName}_SECRET_KEY";
        private string MySQL_IV => $"{Server}_MySQL_{UsernName}_IV";
        private string MySQL_USER => $"{Server}_MySQL_{UsernName}";

        /// <summary>
        /// Initializes a new instance of the <see cref="MySQLDatabase{M}"/> class.
        /// </summary>
        /// <param name="info">The connection string information for the MySQL database.</param>
        public MySQLDatabase(MySQLConnectionStringInfo info)
        {
            UsernName = info.UsernName;
            Port = info.Port;
            Server = info.Server;
        }

        /// <summary>
        /// Encrypts the MySQL Username's Password and saves it in the local machine.
        /// <para/>
        /// See also: <seealso cref="Credential"/>, <seealso cref="CredentialManager"/>, and <seealso cref="Encrypter"/>.
        /// </summary>
        /// <param name="pwd">The password to encrypt.</param>
        /// <exception cref="Exception">Thrown when <see cref="UsernName"/> or <see cref="Server"/> is null or empty.</exception>
        public void EncryptPassword(string pwd)
        {
            if (string.IsNullOrEmpty(UsernName) || string.IsNullOrEmpty(Server))
                throw new Exception("No Username and/or Server was provided");

            Encrypter encrypter = new(pwd);
            encrypter.Encrypt();
            encrypter.ReplaceStoredKeyIV(MySQL_SECRET_KEY, MySQL_IV);
            CredentialManager.Replace(new Credential(MySQL_USER, UsernName, encrypter.Encrypt()));
        }

        /// <summary>
        /// Decrypts the MySQL Username's Password saved in the local machine.
        /// <para/>
        /// See also: <seealso cref="Credential"/>, <seealso cref="CredentialManager"/>, and <seealso cref="Encrypter"/>.
        /// </summary>
        /// <returns>A string which is the decrypted password.</returns>
        /// <exception cref="CredentialFailure">Thrown when no Username <see cref="Credential"/> object could be found.</exception>
        /// <exception cref="Exception">Thrown when <see cref="UsernName"/> or <see cref="Server"/> is null or empty.</exception>
        public string DecryptedPassword()
        {
            if (string.IsNullOrEmpty(UsernName) || string.IsNullOrEmpty(Server))
                throw new Exception("No Username and/or Server was provided");

            Credential? credential = CredentialManager.Get(MySQL_USER);
            if (credential == null)
                throw new CredentialFailure("No Username Credential");

            Encrypter encrypter = new(credential.Password, MySQL_SECRET_KEY, MySQL_IV);
            return encrypter.Decrypt();
        }

        public override DbConnection CreateConnectionObject() => new MySqlConnection(ConnectionString());

        public override string ConnectionString() =>
            $"Server={Server};Port={Port};Database={DatabaseName};Uid={UsernName};Pwd={DecryptedPassword()};";

        protected override string LastIDQry() => "SELECT LAST_INSERT_ID()";
    }
}