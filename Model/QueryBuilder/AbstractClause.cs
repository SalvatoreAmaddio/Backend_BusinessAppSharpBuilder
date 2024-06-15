using Backend.Database;
using System.Reflection;
using System.Text;

namespace Backend.Model
{
    public interface IQueryClause : IDisposable
    {
        public List<QueryParameter> Params();
        public void RemoveLastChange();
        public string Statement();
        public void AddParameter(string placeholder, object? value);
        public T? GetClause<T>() where T : class, IQueryClause, new();
        public T OpenClause<T>() where T : class, IQueryClause, new();
    }
    
    public class Clauses : List<AbstractClause>
    { 
        public void AddClause(AbstractClause clause) 
        {
            Add(clause);
            Reorder();
        }
        private void Reorder() 
        {
            IEnumerable<AbstractClause> new_order = this.OrderBy(s => s.Order).ToList();
            this.Clear();
            this.AddRange(new_order);
        }
    }

    public abstract class AbstractClause : IQueryClause
    {
        public abstract int Order { get; }
        private List<QueryParameter> _parameters = [];
        protected List<string> _bits = [];
        protected StringBuilder sb = new();
        protected ISQLModel _model;
        protected string TableName { get; }
        protected string TableKey { get; }
        protected Clauses Clauses = [];
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AbstractClause() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AbstractClause(ISQLModel model)
        {
            _model = model;
            TableName = model.GetTableName();
            TableKey = model?.GetPrimaryKey()?.Name ?? throw new NullReferenceException("PK is null");
        }
        public void AddParameter(string placeholder, object? value) => _parameters.Add(new(placeholder, value));
        public List<QueryParameter> Params() => _parameters;

        public void Dispose()
        {
            _bits.Clear();
            sb.Clear();
            GC.SuppressFinalize(this);
        }
        public virtual string Statement()
        {
            string? s = null;
            sb.Clear();
            foreach (AbstractClause clause in Clauses) 
            {
                s = clause.Statement();
                sb.Append(s);
            }

            for (int i = 0; i <= _bits.Count - 1; i++)
            {
                sb.Append(_bits[i]);
                sb.Append(' ');
            }

            return sb.ToString();
        }
        public void RemoveLastChange() => _bits.RemoveAt(_bits.Count - 1);
        public T? GetClause<T>() where T : class, IQueryClause, new()
        {
            Type t = typeof(T);
            return Clauses.FirstOrDefault(s => s.GetType().IsAssignableFrom(t)) as T;
        }

        public bool HasWhereClause() => GetClause<WhereClause>() != null;

        public bool HasWhereConditions() 
        { 
            WhereClause? whereClause = GetClause<WhereClause>();
            if (whereClause == null) return false;
            return whereClause._bits.Count() > 1;
        } 

        public T OpenClause<T>() where T : class, IQueryClause, new()
        {
            Type t = typeof(T);
            if (t.IsAssignableFrom(typeof(SelectClause))) throw new NotSupportedException("Cannot be Select");
            if (Clauses == null) throw new NullReferenceException();
            ConstructorInfo? constructor = t.GetConstructor([Clauses.GetType(), _model.GetType()]);
            if (constructor == null)
            {
                throw new InvalidOperationException($"Type {t.FullName} does not have a constructor that takes a parameter of type {_model.GetType().FullName}");
            }

            return (T)constructor.Invoke([Clauses, _model]);
        }
    }

    public abstract class AbstractConditionalClause : AbstractClause, IQueryClause
    {
        public AbstractConditionalClause() { }
        public AbstractConditionalClause(ISQLModel model) : base(model) { }
        protected AbstractConditionalClause EqualsTo(string field, string value) => Condition(field, value, "=");
        protected AbstractConditionalClause NotEqualsTo(string field, string value) => Condition(field, value, "!=");
        protected AbstractConditionalClause GreaterThan(string field, string value) => Condition(field, value, ">");
        protected AbstractConditionalClause GreaterEqualTo(string field, string value) => Condition(field, value, ">=");
        protected AbstractConditionalClause SmallerThan(string field, string value) => Condition(field, value, "<");
        protected AbstractConditionalClause SmallerEqualTo(string field, string value) => Condition(field, value, "<=");
        protected AbstractConditionalClause IsNull(string field)
        {
            _bits.Add($"{field} IS NULL");
            return this;
        }
        protected AbstractConditionalClause IsNotNull(string field)
        {
            _bits.Add($"{field} IS NOT NULL");
            return this;
        }
        protected AbstractConditionalClause Condition(string field, string value, string oprt)
        {
            _bits.Add($"{field} {oprt} {value}");
            return this;
        }
        protected AbstractConditionalClause Limit(int index = 1)
        {
            _bits.Add($"LIMIT {index}");
            return this;
        }
        protected AbstractConditionalClause LogicalOperator(string oprt)
        {
            _bits.Add(oprt);
            return this;
        }
        protected AbstractConditionalClause OR() => LogicalOperator("OR");
        protected AbstractConditionalClause AND() => LogicalOperator("AND");
        protected AbstractConditionalClause NOT() => LogicalOperator("NOT");
        protected AbstractConditionalClause OpenBracket()
        {
            _bits.Add("(");
            return this;
        }
        protected AbstractConditionalClause CloseBracket()
        {
            _bits.Add(")");
            return this;
        }
        public OrderByClause OrderBy() => new(this, _model);

    }
}