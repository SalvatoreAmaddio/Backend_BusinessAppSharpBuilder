using Backend.Database;
using Backend.ExtensionMethods;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace Backend.Model
{

    /// <summary>
    /// This class implements <see cref="ISQLModel"/> and represents a SQL's Table.
    /// </summary>
    public abstract class AbstractSQLModel : ISQLModel
    {
        private readonly List<SimpleTableField> emptyFields = [];

        #region Properties
        public string SelectQry { get; set; } = string.Empty;
        public string UpdateQry { get; set; } = string.Empty;
        public string InsertQry { get; set; } = string.Empty;
        public string DeleteQry { get; set; } = string.Empty;
        public string RecordCountQry { get; set; } = string.Empty;
        #endregion

        public AbstractSQLModel() 
        {
            RecordCountQry = this.CountAll().From().Statement();
            SelectQry = this.Select().AllFields().From().Statement();
            InsertQry = this.Insert().AllFields().Values().Statement();
            UpdateQry = this.Update().AllFields().Where().This().Statement();
            DeleteQry = this.Delete().From().Where().This().Statement();
        }

        public abstract ISQLModel Read(DbDataReader reader);
        public bool PropertyExists(string properyName)
        {
            Type type = GetType();
            return type.GetProperties().Cast<PropertyInfo>().Any(s => s.Name.Equals(properyName));
        }
        public object? GetPropertyValue(string properyName) 
        {
            Type type = GetType();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props) 
                if (prop.Name.Equals(properyName)) return prop.GetValue(this);
            return null;
        }
        public TableField? GetPrimaryKey()
        {
            PropertyInfo? prop = _getProperties().Where(s => s.GetCustomAttribute<PK>() != null).FirstOrDefault();
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
            foreach (PropertyInfo prop in _getProperties())
                yield return prop;
        }
        public IEnumerable<ITableField> GetTableFields() => _getTableFieldsAs<Field>();
        public IEnumerable<ITableField> GetForeignKeys() => _getTableFieldsAs<FK>();
        private IEnumerable<PropertyInfo> GetMandatoryFields()
        {
            foreach (PropertyInfo prop in _getProperties())
            {
                var field = prop.GetCustomAttribute<Mandatory>();
                if (field != null)
                    yield return prop;
            }
        }
        protected PropertyInfo[] _getProperties() => GetType().GetProperties();
        public IEnumerable<string> GetEntityFieldNames()
        {
            foreach (PropertyInfo prop in _getProperties())
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
            foreach (PropertyInfo prop in _getProperties())
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
        private IEnumerable<ITableField> _getTableFieldsAs<F>() where F : AbstractField
        {
            bool isForeignKey = typeof(F) == typeof(FK);
            foreach (PropertyInfo prop in _getProperties())
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
        
        public override bool Equals(object? obj)
        {
            if (obj is not AbstractSQLModel other) return false;
            long? value = (long?)(GetPrimaryKey()?.GetValue());
            long? value2 = (long?)(other?.GetPrimaryKey()?.GetValue());
            if (value == null) return false;
            return value == value2;
        }

        public override int GetHashCode() => HashCode.Combine(GetPrimaryKey()?.GetValue());

        public virtual void SetParameters(List<QueryParameter>? parameters) 
        {
            parameters?.Add(new(GetPrimaryKey()?.Name!, GetPrimaryKey()?.GetValue()));

            foreach(ITableField field in GetTableFields()) 
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

            foreach(SimpleTableField field in emptyFields) 
                sb.Append($"- {field.Name}\n");

            return sb.ToString();
        }

        public virtual bool AllowUpdate()
        {
            emptyFields.Clear();

            foreach(var field in GetMandatoryFields()) 
            {
                string name = field.Name;
                object? value = field.GetValue(this);

                if (value == null) 
                {
                    emptyFields.Add(new(name,value, field));
                    continue;
                }

                if (field.PropertyType == typeof(string)) 
                    if (string.IsNullOrEmpty(value.ToString())) 
                    {
                        emptyFields.Add(new(name, value, field));
                        continue;
                    }

                if (field.PropertyType == typeof(ISQLModel))
                    if (((ISQLModel)field).IsNewRecord()) 
                    {
                        emptyFields.Add(new(name, value, field));
                        continue;
                    }
            }

            return emptyFields.Count == 0;
        }

        public virtual void Dispose()
        {
            emptyFields.Clear();
            GC.SuppressFinalize(this);
        }

    }

    public class SimpleTableField(string name, object? value, PropertyInfo property)
    {
        public PropertyInfo Property = property;
        public string Name { get; } = name;
        private object? Value { get; set; } = value;
        public bool Changed { get; private set; } = false;
        public override string? ToString() => Name;
        public object? GetValue() => Value;
        public void SetValue(object? value) => Value = value;
        public void Change(bool value) => Changed = value;
    }
}