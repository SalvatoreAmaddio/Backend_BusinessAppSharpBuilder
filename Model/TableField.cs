using System.Reflection;
using Backend.Enums;

namespace Backend.Model
{

    /// <summary>
    /// This interface represents fields and methods that a table field object must have.
    /// </summary>
    /// <remarks>
    /// See also <seealso cref="TableField"/>.
    /// </remarks>
    public interface ITableField
    {
        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        public FieldType FieldType { get; }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <value>A string representing the field name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the current value of the field.
        /// </summary>
        /// <returns>An object representing the current value of the field.</returns>
        public object? GetValue();

        /// <summary>
        /// Sets the value of the field.
        /// </summary>
        /// <param name="value">An object representing the new value of the field.</param>
        public void SetValue(object? value);
    }

    /// <summary>
    /// This interface extends <see cref="ITableField"/> and represents a foreign key field.
    /// </summary>
    /// <remarks>
    /// See also <seealso cref="FKField"/>.
    /// </remarks>
    public interface IFKField : ITableField
    {
        /// <summary>
        /// Gets the primary key associated with the foreign key field.
        /// </summary>
        /// <value>A <see cref="TableField"/> object representing the primary key.</value>
        public TableField? PK { get; }

        /// <summary>
        /// Gets the name of the class of the property marked with the <see cref="FK"/> attribute.
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// Gets the type of the class of the property marked with the <see cref="FK"/> attribute.
        /// </summary>
        public Type ClassType { get; }
    }

    /// <summary>
    /// This class is meant for reflection purposes. It encapsulates an <see cref="AbstractField"/>, a <see cref="PropertyInfo"/>, and an <see cref="ISQLModel"/>.
    /// Thanks to this class, <see cref="AbstractSQLModel"/> can produce IEnumerables that are used for creating auto-generated queries.
    /// </summary>
    /// <remarks>
    /// See also: <seealso cref="AbstractSQLModel.GetTableFields"/>, <seealso cref="AbstractSQLModel.GetTableFieldsAs{F}"/>, <seealso cref="AbstractSQLModel.GetForeignKeys"/>, and <seealso cref="AbstractSQLModel.GetPrimaryKey()"/>.
    /// </remarks>
    public class TableField : ITableField
    {
        private AbstractField Field { get; }
        private PropertyInfo Property { get; }
        private ISQLModel Model { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableField"/> class with the specified field, property, and model.
        /// </summary>
        /// <param name="field">The abstract field associated with the table field.</param>
        /// <param name="property">The property info associated with the table field.</param>
        /// <param name="model">The SQL model associated with the table field.</param>
        public TableField(AbstractField field, PropertyInfo property, ISQLModel model)
        {
            Field = field;
            Property = property;
            Model = model;
            FieldType = ReadFieldType(field.ToString());
            Name = field.HasAlternativeName ? field.ToString() : property.Name;
        }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        public FieldType FieldType { get; }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string Name { get; protected set; }

        private static FieldType ReadFieldType(string value)
        {
            return value switch
            {
                "PK" => FieldType.PK,
                "Field" => FieldType.Field,
                _ => FieldType.FK
            };
        }

        /// <summary>
        /// Retrieves the value of a table field's object, i.e., the value of a property associated with an <see cref="AbstractField"/>.
        /// </summary>
        /// <returns>Returns an object representing the value of the property.</returns>
        public object? GetValue() => Property.GetValue(Model);

        /// <summary>
        /// Sets the value of a table field's object, i.e., the value of a property associated with an <see cref="AbstractField"/>.
        /// </summary>
        /// <param name="value">The value to set for the property.</param>
        public void SetValue(object? value) => Property.SetValue(Model, value);

        public override bool Equals(object? obj)
        {
            return obj is TableField field &&
                   EqualityComparer<AbstractField>.Default.Equals(Field, field.Field) &&
                   EqualityComparer<PropertyInfo>.Default.Equals(Property, field.Property) &&
                   EqualityComparer<ISQLModel>.Default.Equals(Model, field.Model);
        }

        public override int GetHashCode() => HashCode.Combine(Field, Property, Model);

        public override string ToString() => $"{Model} - {Property.Name} - {Field}";
    }

    /// <summary>
    /// This class is meant for reflection purposes. It represents an <see cref="FK"/> object.
    /// </summary>
    /// <remarks>
    /// See also <seealso cref="TableField"/>.
    /// </remarks>
    public class FKField : TableField, IFKField
    {
        /// <summary>
        /// Gets the primary key associated with the foreign key field.
        /// </summary>
        public TableField? PK { get; }

        /// <summary>
        /// Gets the name of the class of the property marked with the <see cref="FK"/> attribute.
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// Gets the type of the class of the property marked with the <see cref="FK"/> attribute.
        /// </summary>
        public Type ClassType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FKField"/> class with the specified field, property, and model.
        /// </summary>
        /// <param name="field">The abstract field associated with the foreign key field.</param>
        /// <param name="property">The property info associated with the foreign key field.</param>
        /// <param name="model">The SQL model associated with the foreign key field.</param>
        /// <exception cref="Exception">Thrown if the primary key associated with the foreign key field is not found.</exception>
        public FKField(AbstractField field, PropertyInfo property, ISQLModel model) : base(field, property, model)
        {
            ClassName = property.PropertyType.Name;
            ClassType = property.PropertyType;
            ISQLModel? fk_model = (ISQLModel?)GetValue();

            try 
            {
                fk_model ??= (ISQLModel)Activator.CreateInstance(property.PropertyType)!;
                PK = fk_model.GetPrimaryKey() ?? throw new Exception("No primary key found in the foreign key model.");
                Name = PK.Name;
            }
            catch 
            { 
            
            }

            if (field.HasAlternativeName)
                Name = field.ToString();
        }
    }
}