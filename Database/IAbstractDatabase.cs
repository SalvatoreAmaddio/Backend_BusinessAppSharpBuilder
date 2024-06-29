using Backend.Model;
using System.Data.Common;
using Backend.Source;
using Backend.Exceptions;
using Backend.Enums;

namespace Backend.Database
{
    /// <summary>
    /// Interface that defines the properties and methods that a database class should implement.
    /// </summary>
    public interface IAbstractDatabase : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the type of the model associated with the database.
        /// </summary>
        Type ModelType { get; }

        /// <summary>
        /// Gets or sets the name of the database to connect to.
        /// </summary>
        /// <value>A string representing the name of the database.</value>
        string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISQLModel"/> that the database must refer to.
        /// Set this property to take advantage of auto-generated queries.
        /// </summary>
        /// <value>An object implementing <see cref="ISQLModel"/> or extending <see cref="AbstractSQLModel"/>.</value>
        ISQLModel Model { get; set; }

        /// <summary>
        /// Gets the record source containing the records yielded by 
        /// the <see cref="Retrieve(string?, List{QueryParameter}?)"/> 
        /// or the <see cref="RetrieveAsync(string?, List{QueryParameter}?)"/> methods.
        /// </summary>
        /// <value>A <see cref="MasterSource"/> object.</value>
        MasterSource MasterSource { get; }
        #endregion

        #region Connection
        /// <summary>
        /// Checks if a connection to the database can be made.
        /// <para><c>IMPORTANT:</c></para> The connection closes as soon as the method terminates.
        /// </summary>
        /// <returns>true if the connection was made; otherwise, false.</returns>
        bool AttemptConnection();
        /// <summary>
        /// Asynchronously checks if a connection to the database can be made.
        /// <para><c>IMPORTANT:</c></para> The connection closes as soon as the method terminates.
        /// </summary>
        /// <returns>A task representing the asynchronous operation that returns true if the connection was made; otherwise, false.</returns>
        Task<bool> AttemptConnectionAsync();

        /// <summary>
        /// Returns a string representing the connection string to the database.
        /// </summary>
        /// <returns>A string representing the connection string.</returns>
        string ConnectionString();

        /// <summary>
        /// Creates a <see cref="DbConnection"/> object that handles the connection to the database asynchronously.
        /// <para/>
        /// For example:
        /// <code>
        ///     return new SQLiteConnection(ConnectionString());
        /// </code>
        /// See also: <seealso cref="ConnectionString"/>
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a <see cref="DbConnection"/> object.</returns>
        Task<DbConnection> CreateConnectionObjectAsync();

        /// <summary>
        /// Creates a <see cref="DbConnection"/> object that handles the connection to the database.
        /// <para/>
        /// For example:
        /// <code>
        ///     return new SQLiteConnection(ConnectionString());
        /// </code>
        /// See also: <seealso cref="ConnectionString"/>
        /// </summary>
        /// <returns>A <see cref="DbConnection"/> object.</returns>
        DbConnection CreateConnectionObject();
        #endregion

        #region Execute Query
        /// <summary>
        /// Executes a SQL query asynchronously.
        /// </summary>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The list of parameters for the query, can be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteQueryAsync(string sql, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The list of parameters for the query, can be null.</param>
        void ExecuteQuery(string sql, List<QueryParameter>? parameters = null);
        #endregion

        /// <summary>
        /// Replaces the records in the <see cref="MasterSource"/>.
        /// </summary>
        /// <param name="newRecords">The new records to replace the existing ones.</param>
        void ReplaceRecords(IEnumerable<ISQLModel> newRecords);

        #region Retrieve Records
        /// <summary>
        /// Selects data from the database.
        /// <para/>
        /// Parameters:
        /// <list type="bullet">
        /// <item>
        /// <term><paramref name="sql"/></term>
        /// <description>If null, the <see cref="Model"/> property sets this parameter to the auto-generated SQL statement. Otherwise, you can provide your own select statement.</description>
        /// </item>
        /// <item>
        /// <term><paramref name="parameters"/></term>
        /// <description>If null, the <see cref="Model"/> property sets this parameter to the <see cref="ISQLModel.SetParameters(List{IParameterObject}?)"/> defined in your class. Otherwise, you can provide your own list of parameters.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="sql">The select statement, can be null.</param>
        /// <param name="parameters">The list of parameters, can be null.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ISQLModel"/> objects that can be used to create a <see cref="DataSource"/>.</returns>
        /// <exception cref="NoModelException">Thrown if the <see cref="Model"/> is null.</exception>
        IEnumerable<ISQLModel> Retrieve(string? sql = null, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Selects data from the database asynchronously.
        /// </summary>
        /// <param name="sql">The select statement, can be null.</param>
        /// <param name="parameters">The list of parameters, can be null.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="ISQLModel"/> objects.</returns>
        IAsyncEnumerable<ISQLModel> RetrieveAsync(string? sql = null, List<QueryParameter>? parameters = null);
        #endregion

        #region CRUD
        /// <summary>
        /// Performs a CRUD operation against the database.
        /// <para/>
        /// Parameters:
        /// <list type="bullet">
        /// <item>
        /// <term><paramref name="crud"/></term>
        /// <description>A <see cref="CRUD"/> enum that specifies the type of CRUD operation to perform.</description>
        /// </item>
        /// <item>
        /// <term><paramref name="sql"/></term>
        /// <description>If null, the <see cref="Model"/> property sets this parameter to the auto-generated SQL statement. Otherwise, you can provide your own CRUD statement.</description>
        /// </item>
        /// <item>
        /// <term><paramref name="parameters"/></term>
        /// <description>If null, the <see cref="Model"/> property sets this parameter to the <see cref="ISQLModel.SetParameters(List{IParameterObject}?)"/> defined in your class. Otherwise, you can provide your own list of parameters.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="crud">The CRUD operation to perform.</param>
        /// <param name="sql">The CRUD statement, can be null.</param>
        /// <param name="parameters">The list of parameters, can be null.</param>
        /// <exception cref="NoModelException">Thrown if the <see cref="Model"/> is null.</exception>
        void Crud(CRUD crud, string? sql = null, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Performs a CRUD operation against the database asynchronously.
        /// </summary>
        /// <param name="crud">The CRUD operation to perform.</param>
        /// <param name="sql">The CRUD statement, can be null.</param>
        /// <param name="parameters">The list of parameters, can be null.</param>
        /// <returns>A task representing the asynchronous operation, containing a boolean indicating the success of the operation.</returns>
        Task<bool> CrudAsync(CRUD crud, string? sql = null, List<QueryParameter>? parameters = null);
        #endregion

        #region Aggregate Queries
        /// <summary>
        /// Performs an aggregate query against the database. This method is meant to return one value only.
        /// <para/>
        /// Parameters:
        /// <list type="bullet">
        /// <item>
        /// <term><paramref name="parameters"/></term>
        /// <description>Null if your statement does not rely on parameters. This method won't use the <see cref="ISQLModel.SetParameters(List{IParameterObject}?)"/>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="sql">The statement to perform the aggregate function.</param>
        /// <param name="parameters">The list of parameters, can be null.</param>
        /// <returns>An object representing the result of an aggregate function.</returns>
        object? AggregateQuery(string sql, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Performs an aggregate query against the database asynchronously. This method is meant to return one value only.
        /// </summary>
        /// <param name="sql">The statement to perform the aggregate function.</param>
        /// <param name="parameters">The list of parameters, can be null.</param>
        /// <returns>A task representing the asynchronous operation, containing an object representing the result of the aggregate function.</returns>
        Task<object?> AggregateQueryAsync(string sql, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Performs a count query against the database. This method is meant to return one value only.
        /// <para/>
        /// Parameters:
        /// <list type="bullet">
        /// <item>
        /// <term><paramref name="sql"/></term>
        /// <description>If null but the <see cref="Model"/> property is set, it sets this parameter to the auto-generated SQL count statement. Otherwise, you can provide your own CRUD statement.</description>
        /// </item>
        /// <item>
        /// <term><paramref name="parameters"/></term>
        /// <description>Null if your statement does not rely on parameters. This method won't use the <see cref="ISQLModel.SetParameters(List{IParameterObject}?)"/>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="sql">The statement to perform the count function.</param>
        /// <param name="parameters">The list of parameters, can be null.</param>
        /// <returns>The number of records the query returned.</returns>
        long? CountRecords(string? sql = null, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Performs a count query against the database asynchronously. This method is meant to return one value only.
        /// </summary>
        /// <param name="sql">The statement to perform the count function.</param>
        /// <param name="parameters">The list of parameters, can be null.</param>
        /// <returns>A task representing the asynchronous operation, containing the number of records the query returned.</returns>
        Task<long?> CountRecordsAsync(string? sql = null, List<QueryParameter>? parameters = null);
        #endregion
    }

}