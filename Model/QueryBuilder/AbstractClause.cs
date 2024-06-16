﻿using Backend.Database;
using System.Reflection;
using System.Text;

namespace Backend.Model
{
    public interface IQueryClause : IDisposable
    {
        public List<QueryParameter> Params();
        public void RemoveLastChange();
        public string AsString();
        public void AddParameter(string placeholder, object? value);
        public T? GetClause<T>() where T : class, IQueryClause, new();
        public T OpenClause<T>() where T : class, IQueryClause, new();
    }
    
    public class Clauses : List<AbstractClause>
    { 
        public new void Add(AbstractClause clause) 
        {
            base.Add(clause);
            Reorder();
        }
        private void Reorder() 
        {
            IEnumerable<AbstractClause> new_order = this.OrderBy(s => s.Order).ToList();
            this.Clear();
            this.AddRange(new_order);
        }

        public AbstractClause GetLast() => this[Count-1];
    }

    public abstract class AbstractClause : IQueryClause
    {
        public abstract int Order { get; }
        public List<QueryParameter> _parameters = [];
        protected List<string> _bits = [];
        protected StringBuilder sb = new();
        protected ISQLModel _model = null!;
        protected string TableName { get; } = string.Empty;
        protected string TableKey { get; } = string.Empty;
        public Clauses Clauses = [];
        public AbstractClause() { }
        public AbstractClause(ISQLModel model)
        {
            Clauses.Add(this);
            _model = model;
            TableName = model.GetTableName();
            TableKey = model?.GetPrimaryKey()?.Name ?? throw new NullReferenceException("PK is null");
        }

        protected void TransferClauses(ref Clauses clauses) => this.Clauses = clauses;
        protected void TransferParameters(ref List<QueryParameter> parameters) => this._parameters = parameters;
        public void AddParameter(string placeholder, object? value) => _parameters.Add(new(placeholder, value));
        public List<QueryParameter> Params() => _parameters;

        public virtual void Dispose()
        {
            foreach (AbstractClause clause in Clauses) 
                clause.Clear();
            
            Clauses.Clear();
            Clear();
            GC.SuppressFinalize(this);
        }

        public void Clear() 
        {
            _bits.Clear();
            _parameters.Clear();
        }

        public string Statement() 
        {
            StringBuilder sb = new();
            foreach (AbstractClause clause in Clauses) 
            {
                string s = clause.AsString();
                sb.Append(s);
            }

            return sb.ToString();
        }

        public virtual string AsString()
        {
            sb.Clear();

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
       
        public T? As<T>() where T : class, IQueryClause, new()
        {
            return this as T;
        }

        public void Print() 
        {
            foreach (var c in Clauses)
            {
                Console.WriteLine(c);
            }
        }

        public T OpenClause<T>() where T : class, IQueryClause, new()
        {
            Type t = typeof(T);
            if (t.IsAssignableFrom(typeof(SelectClause))) throw new NotSupportedException("Cannot be Select");
            if (Clauses.Count == 0) throw new NullReferenceException();
            ConstructorInfo? constructor = t.GetConstructor([this.GetType(), _model.GetType()]);
            if (constructor == null)
                throw new InvalidOperationException($"Type {t.FullName} does not have a constructor that takes a parameter of type {_model.GetType().FullName}");
            return (T)constructor.Invoke([this, _model]);
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
        public LimitClause Limit(int limit = 1) => new(this, _model, limit);

    }
}