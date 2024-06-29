using Backend.Database;
using Backend.Events;
using System.Data.Common;
using System.Reflection;

namespace Backend.Model
{
    /// <summary>
    /// Defines a set of methods to reflect over an object's properties.
    /// </summary>
    public interface IReflector : IDisposable
    {
        /// <summary>
        /// Determines whether a property with the specified name exists.
        /// </summary>
        /// <param name="propertyName">The name of the property to check.</param>
        /// <returns>true if the property exists; otherwise, false.</returns>
        bool PropertyExists(string propertyName);

        /// <summary>
        /// Gets the value of the property with the specified name.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property, or null if the property does not exist.</returns>
        object? GetPropertyValue(string propertyName);

        /// <summary>
        /// Gets information about all properties of the object.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="PropertyInfo"/> objects.</returns>
        IEnumerable<PropertyInfo> GetPropertiesInfo();
    }

    /// <summary>
    /// Defines a set of methods and properties that allow a <see cref="AbstractDatabase{M}"/> object to interact with a <see cref="AbstractSQLModel"/> object.
    /// </summary>
    public interface ISQLModel : IReflector
    {
        #region Properties
        /// <summary>
        /// Gets or sets the SQL statement to count all rows in a table.
        /// This property has a default auto-generated query.
        /// </summary>
        string RecordCountQry { get; set; }

        /// <summary>
        /// Gets or sets the SELECT statement used by the <see cref="AbstractDatabase{M}"/> class.
        /// This property has a default auto-generated query.
        /// </summary>
        string SelectQry { get; set; }

        /// <summary>
        /// Gets or sets the UPDATE statement used by the <see cref="AbstractDatabase{M}"/> class.
        /// This property has a default auto-generated query.
        /// </summary>
        string UpdateQry { get; set; }

        /// <summary>
        /// Gets or sets the DELETE statement used by the <see cref="AbstractDatabase{M}"/> class.
        /// This property has a default auto-generated query.
        /// </summary>
        string DeleteQry { get; set; }

        /// <summary>
        /// Gets or sets the INSERT statement used by the <see cref="AbstractDatabase{M}"/> class.
        /// This property has a default auto-generated query.
        /// </summary>
        string InsertQry { get; set; }
        #endregion

        #region Events
        event BeforeRecordDeleteEventHandler? BeforeRecordDelete;
        event AfterRecordDeleteEventHandler? AfterRecordDelete;
        #endregion

        /// <summary>
        /// Creates an instance of the model by reading data from a <see cref="DbDataReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="DbDataReader"/> object.</param>
        /// <returns>A new instance of an object that implements <see cref="ISQLModel"/>.</returns>
        ISQLModel Read(DbDataReader reader);

        /// <summary>
        /// Gets the names of the fields in the entity.
        /// </summary>
        /// <returns>An enumerable collection of field names.</returns>
        IEnumerable<string> GetEntityFieldNames();

        /// <summary>
        /// Gets the fields in the entity.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="ITableField"/> objects.</returns>
        IEnumerable<ITableField> GetEntityFields();

        /// <summary>
        /// Identifies the properties that serve as table fields.
        /// This method is used to auto-generate queries through reflection.
        /// </summary>
        /// <returns>An enumerable collection of properties marked with the [<see cref="Field"/>] attribute.</returns>
        IEnumerable<ITableField> GetTableFields();

        /// <summary>
        /// Identifies the properties that serve as foreign keys.
        /// This method is used to auto-generate queries through reflection.
        /// </summary>
        /// <returns>An enumerable collection of properties marked with the [<see cref="FK"/>] attribute.</returns>
        IEnumerable<ITableField> GetForeignKeys();

        /// <summary>
        /// Identifies the property that serves as the primary key.
        /// This method is used to auto-generate queries through reflection and by the <see cref="IsNewRecord"/> method.
        /// </summary>
        /// <returns>The property marked with the [<see cref="PK"/>] attribute.</returns>
        TableField? GetPrimaryKey();

        /// <summary>
        /// Gets the name of the table that this class represents in the database.
        /// This method is used to auto-generate queries through reflection.
        /// </summary>
        /// <returns>A string representing the table name.</returns>
        string GetTableName();

        /// <summary>
        /// Determines whether the record is a new record.
        /// By default, this check is done by assessing if the primary key is equal to zero.
        /// </summary>
        /// <returns>true if the object is a new record; otherwise, false.</returns>
        bool IsNewRecord();

        /// <summary>
        /// Defines the parameters used in a query.
        /// Each parameter is a <see cref="QueryParameter"/> object.
        /// <para>Example usage:</para>
        /// <code>
        /// public override void SetParameters(List&lt;QueryParameter&gt;? parameters)
        /// {
        ///     parameters?.Add(new QueryParameter("", PersonID));
        ///     parameters?.Add(new QueryParameter("", Name));
        /// }
        /// </code>
        /// </summary>
        /// <param name="parameters">A list of <see cref="QueryParameter"/> objects.</param>
        void SetParameters(List<QueryParameter>? parameters);

        /// <summary>
        /// Checks whether the conditions for an update or insert operation are met.
        /// By default, this method checks for any property marked as <see cref="Mandatory"/> and returns false if any are null.
        /// Override this method to implement additional logic for conditions that must be met for updates or inserts.
        /// </summary>
        /// <returns>true if all conditions are met; otherwise, false.</returns>
        bool AllowUpdate();

        /// <summary>
        /// Gets a list of properties marked with the <see cref="Mandatory"/> attribute that are null.
        /// </summary>
        /// <returns>A string listing the empty mandatory fields.</returns>
        string GetEmptyMandatoryFields();

        /// <summary>
        /// Invokes the event before a record is deleted.
        /// </summary>
        void InvokeBeforeRecordDelete();

        /// <summary>
        /// Invokes the event after a record is deleted.
        /// </summary>
        void InvokeAfterRecordDelete();
    }
}