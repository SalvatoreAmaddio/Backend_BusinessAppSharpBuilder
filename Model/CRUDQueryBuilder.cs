using Backend.Database;
using System.Reflection;
using System.Text;

namespace Backend.Model
{
    public interface IQueryClause : IDisposable
    {
        public List<QueryParameter> Params();
        public bool HasWhereConditions();
        public bool HasWhereClause();
        public void RemoveLastChange();
        public string Statement();
        public void AddParameter(string placeholder, object? value);
        public T? GetClause<T>() where T : class, IQueryClause, new();
        public T OpenClause<T>() where T : class, IQueryClause, new();
        public void Join(AbstractClause clause);
    }
    public abstract class AbstractClause : IQueryClause
    {
        private readonly List<QueryParameter> _parameters = [];
        protected readonly List<string> _bits = [];
        protected readonly StringBuilder sb = new();
        protected ISQLModel _model;
        protected string TableName { get; }
        protected string TableKey { get; }
        public IQueryClause? PreviousClause { get; protected set; }
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
        public bool HasWhereConditions()
        {
            bool found = _bits.Any(s => s.Equals("WHERE"));
            if (!found)
            {
                if (PreviousClause == null) return false;
                return PreviousClause.HasWhereConditions();
            }
            else return (found) ? _bits.Count > 1 : false;
        }
        public bool HasWhereClause()
        {
            bool found = _bits.Any(s => s.Equals("WHERE"));
            if (!found)
            {
                if (PreviousClause == null) return false;
                return PreviousClause.HasWhereClause();
            }
            else return found;
        }
        public void Dispose()
        {
            _bits.Clear();
            sb.Clear();
            GC.SuppressFinalize(this);
        }
        public virtual string Statement()
        {
            string? s = PreviousClause?.Statement();
            sb.Clear();
            sb.Append(s);
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
            Type thisType = GetType();
            if (t.IsAssignableFrom(thisType))
                return this as T;
            if (PreviousClause == null) return null;
            return PreviousClause.GetClause<T>();
        }

        public T OpenClause<T>() where T : class, IQueryClause, new()
        {
            Type t = typeof(T);
            if (t.IsAssignableFrom(typeof(SelectClause))) throw new NotSupportedException("Cannot be Select");
            if (PreviousClause == null) throw new NullReferenceException();
            ConstructorInfo? constructor = t.GetConstructor([PreviousClause.GetType(), _model.GetType()]);
            if (constructor == null)
            {
                throw new InvalidOperationException($"Type {t.FullName} does not have a constructor that takes a parameter of type {_model.GetType().FullName}");
            }

            return (T)constructor.Invoke([PreviousClause, _model]);
        }

        public void Join(AbstractClause clause)
        {
            clause.PreviousClause = this.PreviousClause;
            this.PreviousClause = clause;
        }

    }
    public abstract class AbstractConditionalClause : AbstractClause, IQueryClause 
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AbstractConditionalClause() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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

    #region OrderBy
    public interface IOrderByClause : IQueryClause
    {
        public OrderByClause Field(string field);
    }
    public class OrderByClause : AbstractClause, IOrderByClause
    {
        public OrderByClause() { }
        public OrderByClause(IQueryClause clause, ISQLModel model) : base(model)
        {
            PreviousClause = clause;
            _bits.Add("ORDER BY");
        }

        public OrderByClause Field(string field)
        {
            _bits.Add(field);
            return this;
        }

        public override string Statement()
        {
            string? s = PreviousClause?.Statement();
            sb.Clear();
            sb.Append(s);
            bool notFirstIndex = false;
            bool notLastIndex = false;

            for (int i = 0; i <= _bits.Count - 1; i++)
            {
                notFirstIndex = i > 0;
                notLastIndex = i < _bits.Count - 1;
                sb.Append(_bits[i]);
                if (notFirstIndex && notLastIndex) sb.Append(',');
                sb.Append(' ');
            }

            return sb.ToString();
        }
    }
    #endregion
    
    #region GroupBy
    public interface IGroupBy : IQueryClause 
    {
        public GroupByClause Fields(params string[] fields);
        public GroupByClause Limit(int index = 1);
    }
    public class GroupByClause : AbstractClause, IGroupBy
    {
        public GroupByClause() { }
        public GroupByClause(IQueryClause clause, ISQLModel model) : base(model)
        {
            PreviousClause = clause;
            _bits.Add("GROUP BY");
        }
        
        public GroupByClause Fields(params string[] fields) 
        {
            foreach(string field in fields) 
            {
                _bits.Add(field);
                _bits.Add(",");
            }
            RemoveLastChange();
            return this;
        }

        public GroupByClause Limit(int index = 1)
        {
            _bits.Add($"LIMIT {index}");
            return this;
        }

    }
    #endregion
    
    #region Having
    public interface IHavingClause : IQueryClause
    {
        public HavingClause OpenBracket();
        public HavingClause CloseBracket();
        public HavingClause EqualsTo(string field, string value);
        public HavingClause GreaterThan(string field, string value);
        public HavingClause GreaterEqualTo(string field, string value);
        public HavingClause SmallerThan(string field, string value);
        public HavingClause SmallerEqualTo(string field, string value);
        public HavingClause IsNull(string field);
        public HavingClause IsNotNull(string field);
        public HavingClause OR();
        public HavingClause AND();
        public HavingClause NOT();
        public HavingClause Limit(int index = 1);
    }
    public class HavingClause : AbstractConditionalClause, IHavingClause
    {
        public HavingClause() { }
        public HavingClause(IQueryClause clause, ISQLModel model) : base(model)
        {
            PreviousClause = clause;
            _bits.Add("HAVING");
        }
        public new HavingClause EqualsTo(string field, string value) => Condition(field, value, "=");
        public new HavingClause NotEqualsTo(string field, string value) => Condition(field, value, "!=");
        public new HavingClause GreaterThan(string field, string value) => Condition(field, value, ">");
        public new HavingClause GreaterEqualTo(string field, string value) => Condition(field, value, ">=");
        public new HavingClause SmallerThan(string field, string value) => Condition(field, value, "<");
        public new HavingClause SmallerEqualTo(string field, string value) => Condition(field, value, "<=");
        private new HavingClause Condition(string field, string value, string oprt) => (HavingClause)base.Condition(field, value, oprt);
        public new HavingClause IsNull(string field) => (HavingClause)base.IsNull(field);
        public new HavingClause IsNotNull(string field) => (HavingClause)base.IsNotNull(field);
        public new HavingClause Limit(int index = 1) => (HavingClause)base.Limit(index);
        public new HavingClause OR() => (HavingClause)LogicalOperator("OR");
        public new HavingClause AND() => (HavingClause)LogicalOperator("AND");
        public new HavingClause NOT() => (HavingClause)LogicalOperator("NOT");
        public new HavingClause OpenBracket() => (HavingClause)base.OpenBracket();
        public new HavingClause CloseBracket() => (HavingClause)base.CloseBracket();

    }
    #endregion
    
    #region Where
    public class WhereClause : AbstractConditionalClause, IWhereClause
    {
        public WhereClause() { }
        public WhereClause(IQueryClause clause, ISQLModel model) : base(model)
        {
            PreviousClause = clause;
            _bits.Add("WHERE");
        }
        public WhereClause(ISQLModel model) : base(model)
        {
            PreviousClause = new SelectClause(model).SelectAll().From();
            _bits.Add("WHERE");
        }
        public WhereClause This() 
        {
            return this.EqualsTo(TableKey, $"@{TableKey}");
        }
        public WhereClause Between(string field, string value1, string value2)
        {
            _bits.Add($"{field} BETWEEN {value1} AND {value2}");
            return this;
        }
        public WhereClause In(string field, params string[] values)
        {
            _bits.Add($"{field}");
            _bits.Add($"IN");
            _bits.Add($"(");

            foreach (string value in values)
            {
                _bits.Add(value);
                _bits.Add(", ");
            }
            RemoveLastChange(); // remove last coma
            _bits.Add($")");
            return this;
        }
        public WhereClause Like(string field, string value) => Condition(field, value, "LIKE");
        public new WhereClause EqualsTo(string field, string value) => Condition(field, value, "=");
        public new WhereClause NotEqualsTo(string field, string value) => Condition(field, value, "!=");
        public new WhereClause GreaterThan(string field, string value) => Condition(field, value, ">");
        public new WhereClause GreaterEqualTo(string field, string value) => Condition(field, value, ">=");
        public new WhereClause SmallerThan(string field, string value) => Condition(field, value, "<");
        public new WhereClause SmallerEqualTo(string field, string value) => Condition(field, value, "<=");
        private new WhereClause Condition(string field, string value, string oprt) => (WhereClause)base.Condition(field, value, oprt);
        public new WhereClause IsNull(string field) => (WhereClause)base.IsNull(field);
        public new WhereClause IsNotNull(string field) => (WhereClause)base.IsNotNull(field);
        public new WhereClause Limit(int index = 1) => (WhereClause)base.Limit(index);
        public new WhereClause OR() => (WhereClause)LogicalOperator("OR");
        public new WhereClause AND() => (WhereClause)LogicalOperator("AND");
        public new WhereClause NOT() => (WhereClause)LogicalOperator("NOT");
        public new WhereClause OpenBracket() => (WhereClause)base.OpenBracket();
        public new WhereClause CloseBracket() => (WhereClause)base.CloseBracket();
        public GroupByClause GroupBy() => new(this, _model);
    }
    public interface IWhereClause : IQueryClause
    {
        public GroupByClause GroupBy();
        public WhereClause This();
        public OrderByClause OrderBy();
        public WhereClause In(string field, params string[] values);
        public WhereClause Between(string field, string value1, string value2);
        public WhereClause OpenBracket();
        public WhereClause CloseBracket();
        public WhereClause EqualsTo(string field, string value);
        public WhereClause Like(string field, string value);
        public WhereClause GreaterThan(string field, string value);
        public WhereClause GreaterEqualTo(string field, string value);
        public WhereClause SmallerThan(string field, string value);
        public WhereClause SmallerEqualTo(string field, string value);
        public WhereClause IsNull(string field);
        public WhereClause IsNotNull(string field);
        public WhereClause OR();
        public WhereClause AND();
        public WhereClause NOT();
        public WhereClause Limit(int index = 1);
    }

    #endregion
    
    #region From
    public class FromClause : AbstractClause, IFromClause
    {
        public FromClause() { }
        public FromClause(IQueryClause clause, ISQLModel model) : base(model)
        {
            PreviousClause = clause;
            _bits.Add("FROM");
            _bits.Add(TableName);
        }
        public FromClause(ISQLModel model) : base(model)
        {
            PreviousClause = new SelectClause(model).SelectAll();
            _bits.Add("FROM");
            _bits.Add(TableName);
        }

        /// <summary>
        /// Creates a <c>JOIN</c> between the <paramref name="toTable"/>'s PrimaryKey and this table ForeignKey.
        /// </summary>
        /// <param name="join">The type of Join</param>
        /// <param name="toTable">The table to join to</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private FromClause MakeJoin(string join, ISQLModel toTable)
        {
            string commonKey = toTable?.GetPrimaryKey()?.Name ?? throw new Exception("Null Reference");
            return MakeJoin(join, toTable.GetTableName(), commonKey);
        }

        /// <summary>
        /// Creates a <c>JOIN</c> between the <paramref name="toTable"/> and this table based on a <paramref name="commonKey"/> in both tables.
        /// </summary>
        /// <param name="join">The type of Join</param>
        /// <param name="toTable">The table to join to</param>
        /// <param name="commonKey">The name of a common key</param>
        /// <returns></returns>
        private FromClause MakeJoin(string join, string toTable, string commonKey)
        {
            _bits.Add(join);
            _bits.Add(toTable);
            _bits.Add("ON");
            _bits.Add($"{this.TableName}.{commonKey}");
            _bits.Add("=");
            _bits.Add($"{toTable}.{commonKey}");
            return this;
        }
        /// <summary>
        /// Creates a <c>JOIN</c> between two given tables based on a <paramref name="commonKey"/> in both tables.
        /// </summary>
        /// <param name="join">The type of Join</param>
        /// <param name="fromTable">The table to join from</param>
        /// <param name="toTable">The table to join to</param>
        /// <param name="commonKey">The name of a common key</param>
        /// <returns></returns>
        private FromClause MakeJoin(string join, string fromTable, string toTable, string commonKey)
        {
            _bits.Add(join);
            _bits.Add(toTable);
            _bits.Add("ON");
            _bits.Add($"{fromTable}.{commonKey}");
            _bits.Add("=");
            _bits.Add($"{toTable}.{commonKey}");
            return this;
        }

        #region INNER JOIN
        public FromClause InnerJoin(ISQLModel toTable) => MakeJoin("INNER JOIN", toTable);
        public FromClause InnerJoin(string toTable, string commonKey) => MakeJoin("INNER JOIN", toTable, commonKey);
        public FromClause InnerJoin(string fromTable, string toTable, string commonKey) => MakeJoin("INNER JOIN", fromTable, toTable, commonKey);
        #endregion

        #region RIGHT JOIN
        public FromClause RightJoin(ISQLModel toTable) => MakeJoin("RIGHT JOIN", toTable);
        public FromClause RightJoin(string toTable, string commonKey) => MakeJoin("RIGHT JOIN", toTable, commonKey);
        public FromClause RightJoin(string fromTable, string toTable, string commonKey) => MakeJoin("RIGHT JOIN", fromTable, toTable, commonKey);
        #endregion

        #region LEFT JOIN
        public FromClause LeftJoin(ISQLModel toTable) => MakeJoin("LEFT JOIN", toTable);
        public FromClause LeftJoin(string toTable, string commonKey) => MakeJoin("LEFT JOIN", toTable, commonKey);
        public FromClause LeftJoin(string fromTable, string toTable, string commonKey) => MakeJoin("LEFT JOIN", fromTable, toTable, commonKey);
        #endregion

        public WhereClause Where() => new(this, _model);
        public FromClause CloseBracket()
        {
            _bits.Add(")");
            return this;
        }
        public FromClause OpenBracket()
        {
            _bits.Insert(1, "(");
            return this;
        }
        public FromClause Limit(int index = 1)
        {
            _bits.Add($"LIMIT {index}");
            return this;
        }
        public OrderByClause OrderBy() => new(this, _model);
        public GroupByClause GroupBy() => new(this, _model);
    }
    public interface IFromClause : IQueryClause
    {
        public WhereClause Where();
        public OrderByClause OrderBy();
        public FromClause OpenBracket();
        public FromClause CloseBracket();
        public FromClause InnerJoin(ISQLModel toTable);
        public FromClause InnerJoin(string toTable, string commonKey);
        public FromClause InnerJoin(string fromTable, string toTable, string commonKey);
        public FromClause RightJoin(ISQLModel toTable);
        public FromClause RightJoin(string toTable, string commonKey);
        public FromClause RightJoin(string fromTable, string toTable, string commonKey);
        public FromClause LeftJoin(ISQLModel toTable);
        public FromClause LeftJoin(string toTable, string commonKey);
        public FromClause LeftJoin(string fromTable, string toTable, string commonKey);
        public FromClause Limit(int index = 1);
        public GroupByClause GroupBy();
    }

    #endregion
    
    #region Select
    public class SelectClause : AbstractClause, ISelectClause
    {
        public SelectClause() { }
        public SelectClause(ISQLModel model) : base(model) => _bits.Add("SELECT");

        public SelectClause(IInsertClause clause, ISQLModel model) : this(model)
        {
            PreviousClause = clause;
        }

        public SelectClause SelectAll(string? tableName = null)
        {
            if (string.IsNullOrEmpty(tableName))
                tableName = this.TableName;

            _bits.Add($"{tableName}.*");
            return this;
        }
        public SelectClause Select(params string[] fields)
        {
            foreach (var field in fields)
                _bits.Add($"{field}");
            return this;
        }
        public SelectClause Distinct()
        {
            _bits.Add("DISTINCT");
            return this;
        }
        public SelectClause CountAll()
        {
            _bits.Add($"Count(*)");
            return this;
        }
        public SelectClause Sum(string field)
        {
            _bits.Add($"sum({field})");
            return this;
        }
        public FromClause From()
        {
            if (_bits.Count == 1) SelectAll();
            return new FromClause(this, _model);
        }
        public WhereClause Where()
        {
            return From().Where();
        }
        public override string Statement()
        {
            string? s = PreviousClause?.Statement();
            sb.Clear();
            sb.Append(s);
            bool notFirstIndex = false;
            bool notLastIndex = false;
            for (int i = 0; i <= _bits.Count - 1; i++)
            {
                notFirstIndex = i > 0;
                notLastIndex = i < _bits.Count - 1;
                sb.Append(_bits[i]);
                if (notFirstIndex && notLastIndex) sb.Append(',');
                sb.Append(' ');
            }

            return sb.ToString();
        }
    }
    public interface ISelectClause : IQueryClause
    {
        public SelectClause Sum(string field);
        public SelectClause CountAll();
        public SelectClause SelectAll(string? tableName = null);
        public SelectClause Select(params string[] fields);
        public FromClause From();
        public WhereClause Where();
        public SelectClause Distinct();
    }

    #endregion

    #region Insert
    public interface IInsertClause : IQueryClause
    {
        public InsertClause AllFields();
        public InsertClause Fields(params string[] fields);
        public InsertClause Values(bool includeFields = true);
        public InsertClause RowValues(params string[] fields);
        public SelectClause Select();
    }
    public class InsertClause : AbstractClause, IInsertClause
    {
        public InsertClause() { }
        public InsertClause(ISQLModel model) : base(model) => _bits.Add($"INSERT INTO {model.GetTableName()}");

        public InsertClause AllFields()
        {
            _bits.Add("(");
            string pkName = _model.GetPrimaryKey()?.Name ?? "";

            foreach (string fieldName in _model.GetEntityFieldNames())
            {
                if (pkName.Equals(fieldName)) continue;
                _bits.Add(fieldName);
                _bits.Add(",");
            }
            RemoveLastChange();
            _bits.Add(")");
            return this;
        }

        public InsertClause Fields(params string[] fields)
        {
            _bits.Add("(");
            foreach (string fieldName in fields)
            {
                _bits.Add(fieldName);
                _bits.Add(",");
            }
            _bits.Add(")");
            return this;
        }

        public InsertClause Values(bool includeFields = true)
        {
            IEnumerable<string>? fields = null;

            if (includeFields)
            {
                int index = _bits.IndexOf("(") + 1;
                fields = _bits.Skip(index).ToList();
                fields = fields.Take(fields.Count() - 1);
            }

            if (fields == null)
            {
                _bits.Add("VALUES");
                return this;
            }
            else _bits.Add("VALUES (");

            foreach (string fieldName in fields)
            {
                if (fieldName.Equals(","))
                {
                    _bits.Add(fieldName);
                }
                else _bits.Add($"@{fieldName}");
            }
            _bits.Add(")");
            return this;
        }

        public InsertClause RowValues(params string[] fields)
        {
            foreach (string fieldName in fields)
            {
                _bits.Add($"@{fieldName}");
                _bits.Add(",");
            }
            _bits.Add("),");
            return this;
        }

        public override string Statement()
        {
            int index = _bits.LastIndexOf("),");
            if (index >= 0)
                _bits[index] = ")";
            return base.Statement();
        }

        public SelectClause Select() => new(this, _model);
    }
    #endregion

    #region Update
    public interface IUpdateClause : IQueryClause
    {
        public UpdateClause AllFields();
        public WhereClause Where();
    }
    public class UpdateClause : AbstractClause, IUpdateClause
    {
        public UpdateClause() { }
        public UpdateClause(ISQLModel model) : base(model) => _bits.Add($"UPDATE {model.GetTableName()}");

        public UpdateClause AllFields() 
        {
            string pkName = _model.GetPrimaryKey()?.Name ?? "";
            _bits.Add("SET");

            foreach (string fieldName in _model.GetEntityFieldNames())
            {
                if (pkName.Equals(fieldName)) continue;
                _bits.Add($"{fieldName} = @{fieldName}");
                _bits.Add(",");
            }
            RemoveLastChange();
            return this;
        }

        public WhereClause Where() => new(this, _model);

    }
    #endregion

    #region Delete
    public interface IDeleteClause 
    {
        public FromClause From();
        public WhereClause Where();
    }
    public class DeleteClause : AbstractClause, IDeleteClause
    {
        public DeleteClause() { }
        public DeleteClause(ISQLModel model) : base(model) => _bits.Add($"DELETE");        
        public FromClause From() => new(this, _model);
        public WhereClause Where() => new(this, _model);
    }
    #endregion
}