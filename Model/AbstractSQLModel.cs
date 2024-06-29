using Backend.Database;
using Backend.Events;
using Backend.ExtensionMethods;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace Backend.Model
{
    /// <summary>
    /// This class implements <see cref="ISQLModel"/> and represents a SQL table.
    /// </summary>
    public abstract class AbstractSQLModel : ISQLModel
    {
        private readonly List<SimpleTableField> _emptyFields = new List<SimpleTableField>();

        #region Properties
        public string SelectQry { get; set; } = string.Empty;
        public string UpdateQry { get; set; } = string.Empty;
        public string InsertQry { get; set; } = string.Empty;
        public string DeleteQry { get; set; } = string.Empty;
        public string RecordCountQry { get; set; } = string.Empty;
        #endregion

        #region Events
        public event BeforeRecordDeleteEventHandler? BeforeRecordDelete;
        public event AfterRecordDeleteEventHandler? AfterRecordDelete;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractSQLModel"/> class.
        /// </summary>
        public AbstractSQLModel()
        {
            RecordCountQry = this.CountAll().From().Statement();
            SelectQry = this.Select().All().From().Statement();
            InsertQry = this.Insert().All().Values().Statement();
            UpdateQry = this.Update().AllFields().Where().This().Statement();
            DeleteQry = this.Delete().From().Where().This().Statement();
        }

        public abstract ISQLModel Read(DbDataReader reader);

        public bool PropertyExists(string propertyName)
        {
            Type type = GetType();
            return type.GetProperties().Cast<PropertyInfo>().Any(s => s.Name.Equals(propertyName));
        }

        public object? GetPropertyValue(string propertyName)
        {
            Type type = GetType();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
                if (prop.Name.Equals(propertyName)) return prop.GetValue(this);
            return null;
        }

        public TableField? GetPrimaryKey()
        {
            PropertyInfo? prop = GetProperties().Where(s => s.GetCustomAttribute<PK>() != null).FirstOrDefault();
            if (prop == null) return null;
            AbstractField? field = prop.GetCustomAttribute<PK>();
            return (field != null) ? new TableField(field, prop, this) : null;
        }

        public string GetTableName()
        {
            Table? tableAttr = GetType().GetCustomAttribute<Table>();
            return $"{tableAttr}";
        }

        public IEnumerable<PropertyInfo> GetPropertiesInfo()
        {
            foreach (PropertyInfo prop in GetProperties())
                yield return prop;
        }

        public IEnumerable<ITableField> GetTableFields() => GetTableFieldsAs<Field>();
        public IEnumerable<ITableField> GetForeignKeys() => GetTableFieldsAs<FK>();

        /// <summary>
        /// Identifies the properties that serve as mandatory fields.
        /// </summary>
        /// <returns>An enumerable collection of properties marked with the [<see cref="Mandatory"/>] attribute.</returns>
        private IEnumerable<PropertyInfo> GetMandatoryFields()
        {
            foreach (PropertyInfo prop in GetProperties())
            {
                var field = prop.GetCustomAttribute<Mandatory>();
                if (field != null)
                    yield return prop;
            }
        }

        /// <summary>
        /// Gets all properties of the object.
        /// </summary>
        /// <returns>An array of <see cref="PropertyInfo"/> objects.</returns>
        protected PropertyInfo[] GetProperties() => GetType().GetProperties();

        public IEnumerable<string> GetEntityFieldNames()
        {
            foreach (PropertyInfo prop in GetProperties())
            {
                AbstractField? field = prop.GetCustomAttribute<AbstractField>();
                if (field != null)
                {
                    bool isFK = field.GetType() == typeof(FK);
                    ITableField tableField = (isFK) ? new FKField(field, prop, this) : new TableField(field, prop, this);
                    yield return tableField.Name;
                }
            }
        }

        public IEnumerable<ITableField> GetEntityFields()
        {
            foreach (PropertyInfo prop in GetProperties())
            {
                AbstractField? field = prop.GetCustomAttribute<AbstractField>();
                if (field != null)
                {
                    bool isFK = field.GetType() == typeof(FK);
                    if (isFK)
                        yield return new FKField(field, prop, this);
                    else
                        yield return new TableField(field, prop, this);
                }
            }
        }

        /// <summary>
        /// Identifies the properties that serve as table fields of a specified type.
        /// </summary>
        /// <typeparam name="F">The type of field attribute to look for.</typeparam>
        /// <returns>An enumerable collection of <see cref="ITableField"/> objects.</returns>
        private IEnumerable<ITableField> GetTableFieldsAs<F>() where F : AbstractField
        {
            bool isForeignKey = typeof(F) == typeof(FK);
            foreach (PropertyInfo prop in GetProperties())
            {
                AbstractField? field = prop.GetCustomAttribute<F>();
                if (field != null)
                {
                    if (isForeignKey) yield return new FKField(field, prop, this);
                    else yield return new TableField(field, prop, this);
                }
            }
        }

        public bool IsNewRecord() => (long?)GetPrimaryKey()?.GetValue() == 0;

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not AbstractSQLModel other) return false;
            long? value = (long?)(GetPrimaryKey()?.GetValue());
            long? value2 = (long?)(other?.GetPrimaryKey()?.GetValue());
            if (value == null) return false;
            return value == value2;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => HashCode.Combine(GetPrimaryKey()?.GetValue());

        public virtual void SetParameters(List<QueryParameter>? parameters)
        {
            parameters?.Add(new(GetPrimaryKey()?.Name!, GetPrimaryKey()?.GetValue()));

            foreach (ITableField field in GetTableFields())
                parameters?.Add(new(field.Name, field.GetValue()));

            foreach (ITableField field in GetForeignKeys())
            {
                IFKField fk_field = (IFKField)field;
                parameters?.Add(new(fk_field.Name, fk_field.PK?.GetValue()));
            }
        }

        public string GetEmptyMandatoryFields()
        {
            StringBuilder sb = new();

            foreach (SimpleTableField field in _emptyFields)
                sb.Append($"- {field.Name}\n");

            return sb.ToString();
        }

        public virtual bool AllowUpdate()
        {
            _emptyFields.Clear();

            foreach (var field in GetMandatoryFields())
            {
                string name = field.Name;
                object? value = field.GetValue(this);

                if (value == null)
                {
                    _emptyFields.Add(new(name, value, field));
                    continue;
                }

                if (field.PropertyType == typeof(string))
                    if (string.IsNullOrEmpty(value.ToString()))
                    {
                        _emptyFields.Add(new(name, value, field));
                        continue;
                    }

                if (field.PropertyType == typeof(ISQLModel))
                    if (((ISQLModel)field).IsNewRecord())
                    {
                        _emptyFields.Add(new(name, value, field));
                        continue;
                    }
            }

            return _emptyFields.Count == 0;
        }

        public void InvokeBeforeRecordDelete() => BeforeRecordDelete?.Invoke(this, EventArgs.Empty);
        public void InvokeAfterRecordDelete() => AfterRecordDelete?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Disposes the resources used by the class.
        /// </summary>
        public virtual void Dispose()
        {
            BeforeRecordDelete = null;
            AfterRecordDelete = null;
            _emptyFields.Clear();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents a simple table field with a name, value, and property information.
    /// </summary>
    public class SimpleTableField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTableField"/> class.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="property">The property information associated with the field.</param>
        public SimpleTableField(string name, object? value, PropertyInfo property)
        {
            Name = name;
            Value = value;
            Property = property;
        }

        /// <summary>
        /// Gets the property information associated with the field.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the value of the field.
        /// </summary>
        private object? Value { get; set; }

        /// <summary>
        /// Gets a value indicating whether the field value has changed.
        /// </summary>
        public bool Changed { get; private set; } = false;

        /// <summary>
        /// Returns a string that represents the current field.
        /// </summary>
        /// <returns>A string that represents the current field.</returns>
        public override string? ToString() => Name;

        /// <summary>
        /// Gets the value of the field.
        /// </summary>
        /// <returns>The value of the field.</returns>
        public object? GetValue() => Value;

        /// <summary>
        /// Sets the value of the field.
        /// </summary>
        /// <param name="value">The new value of the field.</param>
        public void SetValue(object? value) => Value = value;

        /// <summary>
        /// Sets the changed status of the field.
        /// </summary>
        /// <param name="value">true if the field value has changed; otherwise, false.</param>
        public void Change(bool value) => Changed = value;
    }

}