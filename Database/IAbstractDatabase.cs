﻿using Backend.Model;
using System.Data.Common;
using Backend.Source;
using Backend.Exceptions;
using Backend.Enums;

namespace Backend.Database
{
    /// <summary>
    /// Interface that defines the properties and methods that a Database class shoud implement.
    /// </summary>
    public interface IAbstractDatabase : IDisposable
    {
        public Type ModelType { get; }

        /// <summary>
        /// Gets and Sets the name of the Database to connect to..
        /// </summary>
        /// <value>A string telling which name of the database.</value>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="ISQLModel"/> the database must refer to.
        /// Set this property to take advantage of auto-generated queries.
        /// </summary>
        /// <value>An object implementing <see cref="ISQLModel"/> or extending <see cref="AbstractSQLModel"/>.</value>
        public ISQLModel Model { get; set; }

        /// <summary>
        /// Gets and sets the Recordsource containing the records yield by 
        /// the <see cref="Retrieve(string?, List{QueryParameter}?)"/> 
        /// or the <see cref="RetrieveAsync(string?, List{QueryParameter}?)"/>.<para/>
        /// 
        /// </summary>
        /// <value>A Recordsource object</value>
        public MasterSource MasterSource { get; }

        public Task ExecuteQueryAsync(string sql, List<QueryParameter>? parameters = null);
        public void ExecuteQuery(string sql, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Relace the Records in the <see cref="Source.MasterSource"/>
        /// </summary>
        /// <param name="newRecords"></param>
        public void ReplaceRecords(IEnumerable<ISQLModel> newRecords);

        /// <summary>
        /// This method should return a string representing the connection string to the Database.
        /// </summary>
        /// <returns>A string</returns>
        public string ConnectionString();

        /// <summary>
        /// It creates a DBConnection object that handles the connection to the database.
        /// <para/>
        /// For Example:
        /// <code>
        ///     return new SQLiteConnection(ConnectionString());
        /// </code>
        /// see also the <seealso cref="ConnectionString"/>
        /// </summary>
        /// <returns>A <see cref="DbConnection"/> object</returns>
        public Task<DbConnection> CreateConnectionObjectAsync();

        /// <summary>
        /// It creates a DBConnection object that handles the connection to the database.
        /// <para/>
        /// For Example:
        /// <code>
        ///     return new SQLiteConnection(ConnectionString());
        /// </code>
        /// see also the <seealso cref="ConnectionString"/>
        /// </summary>
        /// <returns>A <see cref="DbConnection"/> object</returns>
        public DbConnection CreateConnectionObject();

        /// <summary>
        /// This method select the data from the database.
        /// <para/>
        /// Parameters: <para/>
        /// <list type="bullet">
        /// <item>
        ///<term><paramref name="sql"/></term>
        ///<description>if null, the <see cref="Model"/> property sets this parameter to the auto-generated SQL Statement. Otherwise, you can provide your own Select Statement.</description>
        ///</item>
        /// <item>
        ///<term><paramref name="parameters"/></term>
        ///<description>If null, the <see cref="Model"/> property sets this parameter to the <see cref="ISQLModel.SetParameters(List{IParameterObject}?)"/>. defined in your class. Otherways you can provide your own <c>List&lt;IParameterObject&gt;"</c> </description>
        ///</item>
        /// </list>
        /// </summary>
        /// <param name="sql">The select statement, it can be null</param>
        /// <param name="parameters"> A List of object parameter, it can be null</param>
        /// <returns> a IEnumerable&lt;<see cref="ISQLModel"/>&gt; object which can be used to create a <see cref="RecordSource"/></returns>
        /// <exception cref="NoModelException">Thrown if the <see cref="Model"/> is null.</exception>
        public IEnumerable<ISQLModel> Retrieve(string? sql = null, List<QueryParameter>? parameters = null);

        public IAsyncEnumerable<ISQLModel> RetrieveAsync(string? sql = null, List<QueryParameter>? parameters = null);

        /// <summary>
        /// This method performs a CRUD operation against the database.
        /// <para/>
        /// Parameters: <para/>
        /// <list type="bullet">
        /// <item>
        /// <term><paramref name="crud"/></term>
        /// <description>a <see cref="CRUD"/> enum that tells what kind of CRUD operation must be performed.</description>
        /// </item>
        /// <item>
        ///<term><paramref name="sql"/></term>
        ///<description>If null, the <see cref="Model"/> property sets this parameter to the auto-generated SQL Statement. Otherwise, you can provide your own CRUD Statement.</description>
        ///</item>
        /// <item>
        ///<term><paramref name="parameters"/></term>
        ///<description>If null, the <see cref="Model"/> property sets this parameter to the <see cref="ISQLModel.SetParameters(List{IParameterObject}?)"/>. defined in your class. Otherways you can provide your own <c>List&lt;IParameterObject&gt;"</c> </description>
        ///</item>
        /// </list>
        /// </summary>
        /// <param name="sql">The select statement, it can be null</param>
        /// <param name="parameters"> A List of object parameter, it can be null</param>
        /// <exception cref="NoModelException">Thrown if the <see cref="Model"/> is null.</exception>
        public void Crud(CRUD crud, string? sql = null, List<QueryParameter>? parameters = null);
        public Task<bool> CrudAsync(CRUD crud, string? sql = null, List<QueryParameter>? parameters = null);

        /// <summary>
        /// It performs an aggregate query against the database. This method is meant to return one value only.
        /// <para/>
        /// Parameters:
        /// <list type="bullet">
        /// <item>
        ///<term><paramref name="parameters"/></term>
        ///<description>null if your statement does not rely on parameters. This method won't use the <see cref="ISQLModel.SetParameters(List{IParameterObject}?)"/></description>
        ///</item>
        /// </list>
        /// </summary>
        /// <param name="sql">The statement the perform the aggregate function</param>
        /// <param name="parameters">A List of object parameter, it can be null</param>
        /// <returns>An object representing the result of an aggregate function.</returns>
        public object? AggregateQuery(string sql, List<QueryParameter>? parameters = null);

        public Task<object?> AggregateQueryAsync(string sql, List<QueryParameter>? parameters = null);

        /// <summary>
        /// It performs a Count query against the database. This method is meant to return one value only.
        /// <para/>
        /// Parameters:
        /// <list type="bullet">
        /// <item>
        ///<term><paramref name="sql"/></term>
        ///<description>If null but the <see cref="Model"/> property is set, it sets this parameter to the auto-generated SQL Count Statement. Otherwise, you can provide your own CRUD Statement.</description>
        ///</item>
        /// <item>
        ///<term><paramref name="parameters"/></term>
        ///<description>null if your statement does not rely on parameters. This method won't use the <see cref="ISQLModel.SetParameters(List{IParameterObject}?)"/></description>
        ///</item>
        /// </list>
        /// </summary>
        /// <param name="sql">The statement the perform the count function</param>
        /// <param name="parameters">A List of object parameter, it can be null</param>
        /// <returns>How many records the query returned.</returns>
        public long? CountRecords(string? sql = null, List<QueryParameter>? parameters = null);

        public Task<long?> CountRecordsAsync(string? sql = null, List<QueryParameter>? parameters = null);

    }
}