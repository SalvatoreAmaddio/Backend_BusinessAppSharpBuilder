﻿using Backend.Events;
using Backend.Exceptions;
using Backend.Model;
using Backend.Source;
using System.Data.Common;

namespace Backend.Database
{
    /// <summary>
    /// AbstractClass that defines the structure that any Database Class should use. This class implements <see cref="IAbstractDatabase"/>.
    /// </summary>
    public abstract class AbstractDatabase(ISQLModel Model) : IAbstractDatabase
    {
        protected event OnDatabaseConnectionOpen? OnConnectionOpenEvent;
        public virtual string DatabaseName { get; set; } = string.Empty;
        public ISQLModel Model { get; set; } = Model;

        public Type ModelType => Model.GetType();
        public MasterSource MasterSource { get; protected set; } = [];

        public void ReplaceRecords(IEnumerable<ISQLModel> newRecords) 
        {
            MasterSource.Clear();
            MasterSource.AddRange(newRecords);
        }

        public abstract string ConnectionString();
        public abstract DbConnection CreateConnectionObject();
        public Task<DbConnection> CreateConnectionObjectAsync() => Task.FromResult(CreateConnectionObject());
        private void SetCommandWithParameters(DbCommand cmd, string sql, List<QueryParameter>? parameters)
        {
            if (Model == null) throw new NoModelException();
            cmd.CommandText = sql;
            if (parameters == null)
            {
                parameters = [];
                Model.SetParameters(parameters);
            }

            SetParameters(cmd, parameters);
        }

        /// <summary>
        /// Use this method to check if the connection can be made.
        /// <para><c>IMPORTANT:</c></para> the connection closes as soon as the method terminates.
        /// </summary>
        /// <returns>true if the connection was made.</returns>
        public bool AttemptConnection()
        {
            using (DbConnection connection = CreateConnectionObject())
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Use this method to check if the connection can be made.
        /// <para><c>IMPORTANT:</c></para> the connection closes as soon as the method terminates.
        /// </summary>
        /// <returns>true if the connection was made.</returns>
        public async Task<bool> AttemptConnectionAsync()
        {
            using (DbConnection connection = await CreateConnectionObjectAsync())
            {
                try 
                {
                    await connection.OpenAsync();
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
        }

        public async IAsyncEnumerable<ISQLModel> RetrieveAsync(string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (Model == null) throw new NoModelException();

            if (string.IsNullOrEmpty(sql))
                sql = Model.SelectQry;

            sql += ";";
            using (DbConnection connection = await CreateConnectionObjectAsync())
            {
                await connection.OpenAsync();
                using (DbCommand cmd = connection.CreateCommand())
                {
                    SetCommandWithParameters(cmd, sql, parameters);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            yield return Model.Read(reader);
                    }
                }
            }
        }

        public IEnumerable<ISQLModel> Retrieve(string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (Model == null) throw new NoModelException();

            if (string.IsNullOrEmpty(sql))
                sql = Model.SelectQry;
            sql += ";";
            using (DbConnection connection = CreateConnectionObject())
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        SetCommandWithParameters(cmd, sql, parameters);
                        using (DbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                yield return Model.Read(reader);
                        }
                    }
                    transaction.Commit();
                }
            }
        }

        public async Task ExecuteQueryAsync(string sql, List<QueryParameter>? parameters = null)
        {
            using (DbConnection connection = await CreateConnectionObjectAsync())
            {
                await connection.OpenAsync();
                using (DbTransaction transaction = await connection.BeginTransactionAsync())
                {
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandText = sql;
                        SetParameters(cmd, parameters);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    try
                    {
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                        await transaction.RollbackAsync();
                    }
                }
            }
        }

        public void ExecuteQuery(string sql, List<QueryParameter>? parameters = null)
        {
            using (DbConnection connection = CreateConnectionObject())
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandText = sql;
                        SetParameters(cmd, parameters);
                        cmd.ExecuteNonQuery();
                    }
                    try
                    {
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                        transaction?.Rollback();
                    }
                }
            }
        }

        public void Crud(CRUD crud, string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (Model == null) throw new NoModelException();

            if (crud == CRUD.INSERT || crud == CRUD.UPDATE)
                if (!Model.AllowUpdate()) throw new Exception("Conditions for update not met.");

            long? lastID = null;
            if (string.IsNullOrEmpty(sql))
            {
                switch (crud)
                {
                    case CRUD.INSERT:
                        sql = Model.InsertQry;
                        break;
                    case CRUD.UPDATE:
                        sql = Model.UpdateQry;
                        break;
                    case CRUD.DELETE:
                        sql = Model.DeleteQry;
                        break;
                }
            }

            using (DbConnection connection = CreateConnectionObject())
            {
                connection.Open();
                OnConnectionOpenEvent?.Invoke(this, new(connection, crud));
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        SetCommandWithParameters(cmd, sql!, parameters);
                        cmd.ExecuteNonQuery();
                        lastID = (crud == CRUD.INSERT) ? RetrieveLastInsertedID(connection, transaction, LastIDQry()) : null;
                    }
                    try
                    {
                        transaction.Commit();
                        if (lastID != null)
                            Model?.GetPrimaryKey()?.SetValue(lastID);
                        UpdateMasterSource(crud);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                        transaction?.Rollback();
                    }
                }
            }
        }

        public long? CountRecords(string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (string.IsNullOrEmpty(sql) && Model != null)
                sql = Model.RecordCountQry + ";";
            if (string.IsNullOrEmpty(sql)) throw new Exception("No SQL Statement provided");
            return (long?)AggregateQuery(sql, parameters);
        }

        public async Task<long?> CountRecordsAsync(string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (string.IsNullOrEmpty(sql) && Model != null)
                sql = Model.RecordCountQry + ";";
            if (string.IsNullOrEmpty(sql)) throw new Exception("No SQL Statement provided");
            return (long?) await AggregateQueryAsync(sql, parameters);
        }
        public async Task<object?> AggregateQueryAsync(string sql, List<QueryParameter>? parameters = null)
        {
            object? value = null;
            using (DbConnection connection = await CreateConnectionObjectAsync())
            {
                await connection.OpenAsync();
                using (DbTransaction transaction = await connection.BeginTransactionAsync())
                {
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.Transaction = transaction;
                        SetParameters(cmd, parameters);
                        value = await cmd.ExecuteScalarAsync();
                    }
                    try
                    {
                        await transaction.CommitAsync();
                        return value;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                        return value;
                    }
                }
            }
        }

        public object? AggregateQuery(string sql, List<QueryParameter>? parameters = null)
        {
            object? value = null;
            using (DbConnection connection = CreateConnectionObject())
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.Transaction = transaction;
                        SetParameters(cmd, parameters);
                        value = cmd.ExecuteScalar();
                    }
                    try
                    {
                        transaction.Commit();
                        return value;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                        return value;
                    }
                }
            }
        }

        private static long? RetrieveLastInsertedID(DbConnection connection, DbTransaction transaction, string sql)
        {
            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Transaction = transaction;
                return (long?)cmd.ExecuteScalar();
            }
        }

        private static void SetParameters(DbCommand cmd, List<QueryParameter>? parameters)
        {
            if (parameters == null) return;

            foreach (QueryParameter parameter in parameters)
            {
                DbParameter param = cmd.CreateParameter();
                param.ParameterName = $"@{parameter.Placeholder}";
                param.Value = parameter.Value;
                cmd.Parameters.Add(param);
            }
        }

        /// <summary>
        /// Override this method to return the correct statement to retrieve the last inserted id.
        /// </summary>
        /// <returns>A string representing the SQL Statement to retrieve the last inserted id.</returns>
        protected abstract string LastIDQry();

        /// <summary>
        /// Update the <see cref="MasterSource"/> property.
        /// </summary>
        private void UpdateMasterSource(CRUD crud)
        {
            if (MasterSource == null) return;
            switch(crud)
            {
                case CRUD.INSERT:
                    MasterSource.Add(Model);
                break;
                case CRUD.UPDATE:
                    int index = MasterSource.IndexOf(Model);
                    if (index >= 0) MasterSource[index] = Model;
                break;
                case CRUD.DELETE:
                    MasterSource.Remove(Model);
                break;
            }
        }

        public void Dispose()
        {
            OnConnectionOpenEvent = null;
            MasterSource.Dispose();
        }
    }
}
