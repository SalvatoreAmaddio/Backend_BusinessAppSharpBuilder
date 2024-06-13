using Backend.Database;
using System.Data.Common;
using System.Reflection;

namespace Backend.Model
{
    /// <summary>
    /// This interface defines a set of methods and properties that allow a
    /// <see cref="AbstractDatabase"/> object to talk to a <see cref="AbstractSQLModel"/> object
    /// </summary>
    public interface ISQLModel : IDisposable
    {
        public bool PropertyExists(string properyName);
        public object? GetPropertyValue(string properyName); 
        public IEnumerable<PropertyInfo> GetPropertiesInfo();
        public IEnumerable<string> GetEntityFieldNames();
        public IEnumerable<ITableField> GetEntityFields();

        /// <summary>
        /// This method allows the creation of an object by reading the DataReader.
        /// <para></para>
        /// <example>
        /// How to use:<para></para>
        /// Create a constructor as follow:
        /// <code>
        /// public Person(DbDataReader reader) { ... }
        /// </code>
        /// The override the method as follow:
        /// <code>
        /// public override ISQLModel Read(DbDataReader reader) => new Person(reader);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="reader">A DbDataReader object</param>
        /// <returns>a new instance that implements <see cref="ISQLModel"/> or extends <see cref="AbstractSQLModel"/></returns>
        public ISQLModel Read(DbDataReader reader);

        /// <summary>
        /// This method is used to identify which Properties serve as Table Fields such as TEXT, DATE, INT, and so on.<para/>
        /// This method is used to auto-generate queries through Reflection.
        /// </summary>
        /// <returns>Returns an IEnumerable containing all properties marked with the [<see cref="Field"/>] attribute.</returns>
        public IEnumerable<ITableField> GetTableFields();

        /// <summary>
        /// This method is used to identify which Properties serve as Foreign Keys.<para/>
        /// This method is used to auto-generate queries through Reflection.
        /// </summary>
        /// <returns>Returns an IEnumerable containing all properties marked with the [<see cref="FK"/>] attribute.</returns>
        public IEnumerable<ITableField> GetForeignKeys();

        /// <summary>
        /// This method is used to identify which Property serves as Primary Key.<para/>
        /// This method is used to auto-generate queries through Reflection.<para/>
        /// It is also used by the <see cref="IsNewRecord"/> method.
        /// </summary>
        /// <returns>Returns the property marked with the [<see cref="PK"/>] attribute.</returns>
        public TableField? GetPrimaryKey();

        /// <summary>
        /// This method is used to auto-generate queries through Reflection.
        /// </summary>
        /// <returns>A string telling which Table this class represent in a Database</returns>
        public string GetTableName();

        /// <summary>
        /// This method is used to tell if a record is a new record. By Default, this check is done by assessing if the Primary Key is equal to zero.
        /// </summary>
        /// <returns>True if the object is a new record.</returns>
        public bool IsNewRecord();

        /// <summary>
        /// This method allows to define the parameters used in a query.
        /// Each parameter is a <see cref="QueryParameter"/> object.
        /// <para>For Example:</para>
        /// <code>
        ///public override void SetParameters(List&lt;<see cref="QueryParameter"/>&gt;? parameters)
        ///{
        ///parameters?.Add(new ("", PersonID));
        ///parameters?.Add(new ("", Name));
        ///}
        /// </code>
        /// </summary>
        /// <param name="parameters">A list of IParameterObject</param>
        public void SetParameters(List<QueryParameter>? parameters);

        /// <summary>
        ///By default, this method check for any Property marked as <see cref="Mandatory"/>. If any is found and is null, it will return false.
        ///You can override this method to implement some additional logic by which Update and Insert can fire if certain conditions are met.
        ///<para/>
        ///For instance, if some fields are supposed to greater than a given number, or must have a specific value/format, you can implement your logic here.
        ///</summary>
        ///<returns>true if all conditions are met; False if one ore more condition are not met.</returns>
        public bool AllowUpdate();

        /// <summary>
        /// Gets and Sets the select statement to count all rows in a table.
        /// <para/>
        /// This property comes with a default auto-generate query
        /// </summary>
        /// <value>A string.</value>
        public string RecordCountQry { get; set; }

        /// <summary>
        /// Gets and Sets the Select Statement used by the <see cref="AbstractDatabase"/> class.
        /// <para/>
        /// This property comes with a default auto-generated query
        /// </summary>
        /// <value>A string representing the Select Statement.</value>
        public string SelectQry { get; set; }

        /// <summary>
        /// Gets and Sets the Update Statement used by the <see cref="AbstractDatabase"/> class.
        /// <para/>
        /// This property comes with a default auto-generated query.
        /// </summary>
        /// <value>A string representing the Update Statement.</value>
        public string UpdateQry { get; set; }

        /// <summary>
        /// Gets and Sets the Delete Statement used by the <see cref="AbstractDatabase"/> class.
        /// <para/>
        /// This property comes with a default auto-generated query.
        /// </summary>
        /// <value>A string representing the Delete Statement.</value>
        public string DeleteQry { get; set; }

        /// <summary>
        /// Gets and Sets the Insert Statement used by the <see cref="AbstractDatabase"/> class.
        /// <para/>
        /// This property comes with a auto-generated query.
        /// </summary>
        /// <value>A string representing the Insert Statement.</value>
        public string InsertQry { get; set; }

        /// <summary>
        /// Gets a list of Properties marked with the <see cref="Mandatory"/> attribute but they are null.
        /// </summary>
        /// <returns>A string</returns>
        public string GetEmptyMandatoryFields();
    }

    /// <summary>
    /// This interface defines a set of methods that help to construct a more complex query.
    /// </summary>
}