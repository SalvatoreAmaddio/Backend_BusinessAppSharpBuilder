using Backend.Exceptions;
using Backend.Model;
using Backend.Utils;
using MySqlConnector;
using System.Data.Common;

namespace Backend.Database
{
    /// <summary>
    /// This structure provides bits of information used for building a Connection String for a MySQL database.
    /// This structure is used by the <see cref="MySQLDatabase"/>'s constructor.
    /// </summary>
    public struct MySQLConnectionStringInfo(string username, string server = "localhost", string port = "3306") 
    {
        public string UsernName { get; set; } = username;
        public string Port { get; set; } = port;
        public string Server { get; set; } = server;

    }

    /// <summary>
    /// This class is a concrete definition of <see cref="AbstractDatabase"/> meant for dealing with a MySQL database.
    /// </summary>
    public class MySQLDatabase(ISQLModel Model) : AbstractDatabase(Model)
    {
        public string UsernName { get; set; } = string.Empty;
        public string Port { get; set; } = "3306";
        public string Server { get; set; } = "localhost";
        private string MySQL_SECRET_KEY => $"{Server}_MySQL_{UsernName}_SECRET_KEY";
        private string MySQL_IV => $"{Server}_MySQL_{UsernName}_IV";
        private string MySQL_USER => $"{Server}_MySQL_{UsernName}";

        public MySQLDatabase(ISQLModel Model, MySQLConnectionStringInfo info) : this(Model) 
        {
            UsernName = info.UsernName;
            Port = info.Port;
            Server = info.Server;
        }

        /// <summary>
        /// Encrypts the MySQL Username's Password and saves it in the local machine.
        /// <para/>
        /// See also: <seealso cref="Credential"/>, <seealso cref="CredentialManager"/>, and <seealso cref="Encrypter"/>
        /// </summary>
        /// <param name="pwd"></param>
        /// <exception cref="Exception"><see cref="UsernName"/> property is null or empty.</exception>
        public void EncryptPassword(string pwd) 
        {
            if (string.IsNullOrEmpty(UsernName) || string.IsNullOrEmpty(Server)) throw new Exception("No Username and/or Server was provided");
            Encrypter encrypter = new(pwd);
            encrypter.Encrypt();
            encrypter.ReplaceStoredKeyIV(MySQL_SECRET_KEY, MySQL_IV);
            CredentialManager.Replace(new(MySQL_USER, UsernName, encrypter.Encrypt()));
        }

        /// <summary>
        /// Decrypts the MySQL Username's Password saved in the local machine.
        /// <para/>
        /// See also: <seealso cref="Credential"/>, <seealso cref="CredentialManager"/>, and <seealso cref="Encrypter"/>
        /// </summary>
        /// <returns>A string which is the decrypted string.</returns>
        /// <exception cref="CredentialFailure">No Username's <see cref="Credential"/> object could be found.</exception>
        public string DecryptedPassword()
        {
            if (string.IsNullOrEmpty(UsernName) || string.IsNullOrEmpty(Server)) throw new Exception("No Username and/or Server was provided");
            Credential? credential = CredentialManager.Get(MySQL_USER);
            if (credential == null) throw new CredentialFailure("No Username Credential");
            Encrypter encrypter = new(credential.Password, MySQL_SECRET_KEY, MySQL_IV);
            return encrypter.Decrypt();
        }

        public override DbConnection CreateConnectionObject() => new MySqlConnection(ConnectionString());
        public override string ConnectionString() => $"Server={Server};Port={Port};Database={DatabaseName};Uid={UsernName};Pwd={DecryptedPassword()};";
        protected override string LastIDQry() => "SELECT LAST_INSERT_ID()";
    }
}