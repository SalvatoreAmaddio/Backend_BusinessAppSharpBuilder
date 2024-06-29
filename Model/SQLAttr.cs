namespace Backend.Model
{
    /// <summary>
    /// This attribute is used to associate a class extending <see cref="AbstractSQLModel"/> with a table name in your database.
    /// It is pivotal for building auto-generated queries.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Table : Attribute
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class with the specified table name.
        /// </summary>
        /// <param name="name">The name of the table in the database.</param>
        public Table(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Returns the name of the table associated with this attribute.
        /// </summary>
        /// <returns>The name of the table.</returns>
        public override string ToString() => _name;
    }

    /// <summary>
    /// This is the base class extended by <see cref="Field"/>, <see cref="PK"/>, and <see cref="FK"/> attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class AbstractField : Attribute
    {
        /// <summary>
        /// Returns the name of the derived attribute class.
        /// </summary>
        /// <returns>The name of the derived attribute class.</returns>
        public override string ToString() => GetType().Name;
    }

    /// <summary>
    /// This attribute is used to associate a property of an <see cref="AbstractSQLModel"/> with a field in the corresponding database table.
    /// It is pivotal for building auto-generated queries.
    /// </summary>
    public class Field : AbstractField
    {
        // No additional functionality required for the Field attribute
    }

    /// <summary>
    /// This attribute is used to associate a property of an <see cref="AbstractSQLModel"/> with the primary key of the corresponding database table.
    /// It is pivotal for building auto-generated queries.
    /// <para/>
    /// Note: Only one property in a class can have this attribute.
    /// </summary>
    public class PK : AbstractField
    {
        // No additional functionality required for the PK attribute
    }

    /// <summary>
    /// This attribute is used to associate a property of an <see cref="AbstractSQLModel"/> with a foreign key in the corresponding database table.
    /// It is pivotal for building auto-generated queries.
    /// </summary>
    public class FK : AbstractField
    {
        // No additional functionality required for the FK attribute
    }

}
