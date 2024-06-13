using System.Reflection;
using Backend.Enums;

namespace Backend.Model
{

    /// <summary>
    /// This interface represents fields and methods that a Table Field object must have.
    /// <para/>
    /// See also <seealso cref="TableField"/>.
    /// </summary>
    public interface ITableField
    {
        public FieldType FieldType { get; }
        /// <summary>
        /// The name of the field.
        /// </summary>
        /// <value>A string</value>
        public string Name { get; }

        /// <summary>
        /// Get the current value of the field.
        /// </summary>
        /// <returns>An object</returns>
        public object? GetValue();

        /// <summary>
        /// Sets the value of the field.
        /// </summary>
        /// <param name="value">An object</param>
        public void SetValue(object? value);
    }

    /// <summary>
    /// This interface extends <see cref="ITableField"/> and it represents a Foreign Key Field.
    /// <para/>
    /// See also <seealso cref="FKField"/>
    /// </summary>
    public interface IFKField : ITableField
    {
        /// <summary>
        /// The Primary Key of the Foreign Key Field.<para/>
        /// For Example:
        /// <code>
        /// [FK] //The Foreign Key field
        /// public Gender Gender { get => _gender; set => UpdateProperty(ref value, ref _gender);}
        /// 
        /// ....
        /// 
        /// public class Gender : AbstractModel 
        /// {
        ///     [PK]
        ///     public long GenderID { ... } //Return this property.
        /// }
        /// </code>
        /// </summary>
        /// <value>A TableField object</value>
        public TableField? PK { get; }

        /// <summary>
        /// Gets the name of the Class of the Property marked with <see cref="FK"/> attribute.
        /// </summary>
        public string ClassName { get; }
    }

    /// <summary>
    /// This class is meant for Reflection Purpose. It encapsulates an <see cref="AbstractField"/>, a <see cref="PropertyInfo"/> and an <see cref="ISQLModel"/>.
    /// Thanks to this class, <see cref="AbstractSQLModel"/> can produce IEnumerables that are used for creating auto-generated queries.
    /// <para/>
    /// see also: <seealso cref="AbstractSQLModel.GetTableFields"/>, <seealso cref="AbstractSQLModel._getTableFieldsAs{F}"/>, <seealso cref="AbstractSQLModel.GetForeignKeys"/> and <seealso cref="AbstractSQLModel.GetPrimaryKey()"/>.
    /// </summary>
    public class TableField(AbstractField field, PropertyInfo property, ISQLModel model) : ITableField
    {
        private AbstractField Field { get; } = field;
        private PropertyInfo Property { get; } = property;
        private ISQLModel Model { get; } = model;
        public FieldType FieldType { get; } = ReadFieldType(field.ToString());
        public string Name { get; protected set; } = property.Name;

        private static FieldType ReadFieldType(string value)
        {
            if (value.Equals("PK")) return FieldType.PK;
            if (value.Equals("Field")) return FieldType.Field;
            return FieldType.FK;
        }

        /// <summary>
        /// It retrieves the value of a TableField's object, i.e. the value of a Property associated to a <see cref="AbstractField"/>.
        /// </summary>
        /// <returns>Returns an object representing the value of the Property</returns>
        public object? GetValue() => Property.GetValue(Model);

        /// <summary>
        /// It sets the value of a TableField's object, i.e. the value of a Property associated to a <see cref="AbstractField"/>.
        /// </summary>
        /// <param name="value">The value to give to the Property</param>
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
    /// This class is meant for Reflection Purpose. It represents a <see cref="FK"/> object.
    /// See also <seealso cref="TableField"/>.
    /// </summary>
    public class FKField : TableField, IFKField
    {
        public TableField? PK { get; }
        public string ClassName { get; }
        public FKField(AbstractField field, PropertyInfo property, ISQLModel model) : base(field, property, model)
        {
            ISQLModel? fk_model = (ISQLModel?)GetValue();
            fk_model ??= (ISQLModel)Activator.CreateInstance(property.PropertyType)!;
            ClassName = property.PropertyType.Name;
            PK = fk_model.GetPrimaryKey() ?? throw new Exception("NO PK IN FK");
            Name = PK.Name;
        }
    }
}
