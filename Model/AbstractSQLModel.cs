using Backend.Database;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace Backend.Model
{

    /// <summary>
    /// This class implements <see cref="ISQLModel"/> and represents a SQL's Table.
    /// </summary>
    abstract public class AbstractSQLModel : ISQLModel
    {
        public string SelectQry { get; set; } = string.Empty;
        public string UpdateQry { get; set; } = string.Empty;
        public string InsertQry { get; set; } = string.Empty;
        public string DeleteQry { get; set; } = string.Empty;
        public string RecordCountQry { get; set; } = string.Empty;        
        public AbstractSQLModel()
        {
            _ = new QueryBuilder(this);
        }

        public abstract ISQLModel Read(DbDataReader reader);
        public IEnumerable<ITableField> GetTableFields() => GetTableFieldsAs<Field>();
        public TableField? GetTablePK() 
        {
            PropertyInfo? prop = GetType().GetProperties().Where(s=>s.GetCustomAttribute<PK>()!=null).FirstOrDefault();
            if (prop == null) return null;
            AbstractField? field = prop.GetCustomAttribute<PK>();
            return (field != null) ? new TableField(field, prop, this) : null;
        }

        public IEnumerable<ITableField> GetTableFKs() => GetTableFieldsAs<FK>();

        public string GetTableName()
        {
            Table? tableAttr = GetType().GetCustomAttribute<Table>();
            return $"{tableAttr}";
        }

        private IEnumerable<PropertyInfo> GetMandatoryFields() 
        {
            Type type = GetType();
            PropertyInfo[] props = type.GetProperties();

            foreach (PropertyInfo prop in props)
            {
                var field = prop.GetCustomAttribute<Mandatory>();
                if (field != null)
                    yield return prop;
            }
        }

        public IEnumerable<string> GetEntityFields() 
        {
            Type type = GetType();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
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

        public IEnumerable<ITableField> GetAllTableFields()
        {
            Type type = GetType();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
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

        private IEnumerable<ITableField> GetTableFieldsAs<F>() where F : AbstractField
        {
            Type type = GetType();
            PropertyInfo[] props = type.GetProperties();
            bool isForeignKey = typeof(F) == typeof(FK);
            foreach (PropertyInfo prop in props)
            {
                AbstractField? field = prop.GetCustomAttribute<F>();
                if (field != null) 
                {
                    if (isForeignKey) yield return new FKField(field, prop, this);
                    else yield return new TableField(field, prop, this);
                }
            }
        }

        public bool IsNewRecord() => (long?)GetTablePK()?.GetValue() == 0;
        
        public override bool Equals(object? obj)
        {
            if (obj is not AbstractSQLModel other) return false;
            long? value = (long?)(GetTablePK()?.GetValue());
            long? value2 = (long?)(other?.GetTablePK()?.GetValue());
            if (value == null) return false;
            return value == value2;
        }

        public override int GetHashCode() => HashCode.Combine(GetTablePK()?.GetValue());

        public virtual void SetParameters(List<QueryParameter>? parameters) 
        {
            parameters?.Add(new(GetTablePK()?.Name!, GetTablePK()?.GetValue()));

            foreach(ITableField field in GetTableFields()) 
                parameters?.Add(new(field.Name, field.GetValue()));

            foreach (ITableField field in GetTableFKs()) 
            {
                IFKField fk_field = (IFKField)field;
                parameters?.Add(new(fk_field.Name, fk_field.PK?.GetValue()));
            }
        }

        private readonly List<SimpleTableField> emptyFields = [];

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
