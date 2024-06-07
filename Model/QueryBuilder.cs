using Backend.Database;
using System.Text;

namespace Backend.Model
{
    /// <summary>
    /// This class build default queries for <see cref="AbstractSQLModel"/> objects.
    /// </summary>
    public class QueryBuilder
    {
        private readonly ISQLModel model;
        private readonly string tableName;
        private readonly IEnumerable<ITableField> fields;
        private readonly IEnumerable<ITableField> fks;
        private readonly TableField? pk;

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="model">Any object that implements a ISQLModel interface</param>
        public QueryBuilder(ISQLModel model)
        {
            this.model = model;
            tableName = model.GetTableName();
            fields = model.GetTableFields();
            fks = model.GetTableFKs();
            pk = model.GetTablePK();
            model.RecordCountQry = $"SELECT COUNT(*) FROM {tableName}";
            model.SelectQry = $"SELECT * FROM {tableName}";
            model.UpdateQry = BuildUpdateQuery();
            model.InsertQry = BuildInsertQuery();
            model.DeleteQry = $"DELETE FROM {tableName} WHERE {pk?.Name}=@{pk?.Name};";
        }

        private string BuildUpdateQuery()
        {
            StringBuilder sb = new();
            sb.Append($"UPDATE {tableName} SET ");

            foreach (ITableField field in fields)
                sb.Append($"{field.Name} = @{field.Name}, ");

            foreach (ITableField field in fks)
                sb.Append($"{field.Name} = @{field.Name}, ");

            sb.Remove(sb.Length - 1, 1);
            sb.Remove(sb.Length - 1, 1);

            sb.Append($" WHERE {pk?.Name} = @{pk?.Name};");
            return sb.ToString();
        }

        private string BuildInsertQuery()
        {
            StringBuilder sb = new();
            sb.Append($"INSERT INTO {tableName} (");

            foreach (ITableField field in fields)
                sb.Append($"{field.Name}, ");

            foreach (ITableField field in fks)
                sb.Append($"{field.Name}, ");

            sb.Remove(sb.Length - 1, 1);
            sb.Remove(sb.Length - 1, 1);

            sb.Append($") VALUES (");

            foreach (ITableField field in fields)
                sb.Append($"@{field.Name}, ");

            foreach (ITableField field in fks)
                sb.Append($"@{field.Name}, ");

            sb.Remove(sb.Length - 1, 1);
            sb.Remove(sb.Length - 1, 1);

            sb.Append($");");
            return sb.ToString();
        }
    }

    //////////////////////////////////////////////////////////////

    public interface IQueryClause : IDisposable
    {
        public List<QueryParameter> Params();
        public bool HasWhereClause();
        public void RemoveLastChange();
        public string Statement();
        public IQueryClause OpenBracket();
        public IQueryClause CloseBracket();
        public void AddParameter(string placeholder, object? value);
    }

    public abstract class AbstractClause : IQueryClause
    {
        private readonly List<QueryParameter> _parameters = [];
        protected readonly List<string> _bits = [];
        protected readonly StringBuilder sb = new();
        protected ISQLModel _model;
        protected string TableName { get; }
        protected string TableKey { get; }
        protected IQueryClause? _clause;
        public AbstractClause(ISQLModel model)
        {
            _model = model;
            TableName = model.GetTableName();
            TableKey = model?.GetTablePK()?.Name ?? throw new NullReferenceException("PK is null");
        }
        public void AddParameter(string placeholder, object? value) => _parameters.Add(new(placeholder, value));
        public List<QueryParameter> Params() => _parameters;
        public bool HasWhereClause() => _bits.Any(s => s.Equals("WHERE"));
        public void Dispose()
        {
            _bits.Clear();
            sb.Clear();
            GC.SuppressFinalize(this);
        }

        public virtual string Statement()
        {
            string? s = _clause?.Statement();
            sb.Clear();
            sb.Append(s);
            for (int i = 0; i <= _bits.Count - 1; i++)
            {
                sb.Append(_bits[i]);
                sb.Append(' ');
            }

            return sb.ToString();
        }

        public virtual IQueryClause OpenBracket()
        {
            _bits.Add("(");
            return this;
        }
        public virtual IQueryClause CloseBracket()
        {
            _bits.Add(")");
            return this;
        }

        public virtual AbstractClause Limit(int limit = 1)
        {
            _bits.Add($"LIMIT {limit}");
            return this;
        }

        public void RemoveLastChange() => _bits.RemoveAt(_bits.Count - 1);
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
    public interface IFromClause : IQueryClause
    {
        public FromClause InnerJoin(ISQLModel toTable);
        public FromClause InnerJoin(string toTable, string commonKey);
        public FromClause InnerJoin(string fromTable, string toTable, string commonKey);
        public FromClause RightJoin(ISQLModel toTable);
        public FromClause RightJoin(string toTable, string commonKey);
        public FromClause RightJoin(string fromTable, string toTable, string commonKey);
        public FromClause LeftJoin(ISQLModel toTable);
        public FromClause LeftJoin(string toTable, string commonKey);
        public FromClause LeftJoin(string fromTable, string toTable, string commonKey);
        public WhereClause Where();
        public AbstractClause Limit(int index);
    }
    public interface IWhereClause : IQueryClause
    {
        public WhereClause In(string field, params string[] values);
        public WhereClause Between(string field, string value1, string value2);
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
        public AbstractClause Limit(int index);
    }
    public class WhereClause : AbstractClause, IWhereClause
    {
        public WhereClause(IQueryClause clause, ISQLModel model) : base(model)
        {
            _clause = clause;
            _bits.Add("WHERE");
        }
        public WhereClause(ISQLModel model) : base(model)
        {
            _clause = new SelectClause(model).SelectAll().From();
            _bits.Add("WHERE");
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
        public WhereClause EqualsTo(string field, string value) => Condition(field, value, "=");
        public WhereClause Like(string field, string value) => Condition(field, value, "LIKE");
        public WhereClause NotEqualsTo(string field, string value) => Condition(field, value, "!=");
        public WhereClause GreaterThan(string field, string value) => Condition(field, value, ">");
        public WhereClause GreaterEqualTo(string field, string value) => Condition(field, value, ">=");
        public WhereClause SmallerThan(string field, string value) => Condition(field, value, "<");
        public WhereClause SmallerEqualTo(string field, string value) => Condition(field, value, "<=");
        public WhereClause IsNull(string field)
        {
            _bits.Add($"{field} IS NULL");
            return this;
        }
        public WhereClause IsNotNull(string field)
        {
            _bits.Add($"{field} IS NOT NULL");
            return this;
        }
        private WhereClause Condition(string field, string value, string oprt)
        {
            _bits.Add($"{field} {oprt} {value}");
            return this;
        }
        public new WhereClause Limit(int index) => (WhereClause)base.Limit(index);
        private WhereClause LogicalOperator(string oprt)
        {
            _bits.Add(oprt);
            return this;
        }
        public WhereClause OR() => LogicalOperator("OR");
        public WhereClause AND() => LogicalOperator("AND");
        public WhereClause NOT() => LogicalOperator("NOT");
        public new WhereClause OpenBracket() => (WhereClause)base.OpenBracket();
        public new WhereClause CloseBracket() => (WhereClause)base.CloseBracket();

    }
    public class FromClause : AbstractClause, IFromClause
    {
        public FromClause(ISelectClause clause, ISQLModel model) : base(model)
        {
            _clause = clause;
            _bits.Add("FROM");
            _bits.Add(TableName);
        }
        public FromClause(ISQLModel model) : base(model)
        {
            _clause = new SelectClause(model).SelectAll();
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
            string commonKey = toTable?.GetTablePK()?.Name ?? throw new Exception("Null Reference");
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
            _bits.Add(fromTable);
            _bits.Add(join);
            _bits.Add(toTable);
            _bits.Add("ON");
            _bits.Add($"{this.TableName}.{commonKey}");
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
        public new FromClause OpenBracket()
        {
            _bits.Insert(1, "(");
            return this;
        }
        public new FromClause CloseBracket() => (FromClause)base.CloseBracket();
        public new FromClause Limit(int index) => (FromClause)base.Limit(index);
    }
    public class SelectClause : AbstractClause, ISelectClause
    {
        public SelectClause(ISQLModel model) : base(model) => _bits.Add("SELECT");

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
            sb.Clear();
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

        public new SelectClause OpenBracket() => (SelectClause)base.OpenBracket();
        public new SelectClause CloseBracket() => (SelectClause)base.CloseBracket();

        /// <summary>
        /// Cannot use Limit in Select Clause.
        /// </summary>
        public override AbstractClause Limit(int limit = 1)
        {
            throw new NotImplementedException("Limit should only be used in FROM and WHERE Clauses");
        }
    }
}