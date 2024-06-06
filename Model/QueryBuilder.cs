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


    public class SelectBuilder(ISQLModel model)  : IDisposable
    {
        private string _OpenBracket = string.Empty;
        private string Limit = string.Empty;
        private string WhereClause = string.Empty;
        private readonly List<string> Fields = [];
        private readonly List<string> Joins = [];
        private readonly List<string> WhereCondition = [];
        private readonly List<QueryParameter> _parameters = [];
        private ISQLModel model = model;
        private string tableName => model.GetTableName();

        /// <summary>
        /// Add a parameter to the <see cref="Params"/> property.
        /// </summary>
        /// <param name="placeholder">A string representing the placeholder. e.g. @name</param>
        /// <param name="value">The value of the parameter</param>
        public void AddParameter(string placeholder, object? value) => _parameters.Add(new(placeholder, value));



        /// <summary>
        /// Returns the built SELECT Statement.
        /// </summary>
        /// <returns>a string</returns>
        public string Statement()
        {
            StringBuilder sb = new();
            sb.Append($"SELECT ");

            if (Fields.Count == 0)
                sb.Append('*');
            else
            {
                foreach (string s in Fields)
                    sb.Append(s);
            }

            if (_OpenBracket.Length == 0) 
            {
                sb.Append($" FROM {tableName}");
            } else 
            {
                sb.Append($" FROM ({tableName}");
            }

            foreach (string s in Joins)
                sb.Append(s);

            if (WhereClause.Length > 0)
                sb.Append(WhereClause);

            foreach (string s in WhereCondition)
                sb.Append(s);

            if (Limit.Length > 0)
                sb.Append(Limit);

            return sb.ToString();
        }

        public List<QueryParameter> Params() => _parameters;
        public SelectBuilder LIMIT(int limit =1) 
        {
            Limit = $" LIMIT {limit}";
            return this;
        }

        public SelectBuilder MakeJoin(string join, string tableName1, string tableName2, string key1, string key2)
        {
            Joins.Add($" {join}");
            Joins.Add($" {tableName1} ON {tableName1}.{key1} = {tableName2}.{key2}");
            return this;
        }

        public SelectBuilder MakeJoin(string join, string tableName1, string tableName2, string commonKey)
        {
            Joins.Add($" {join}");
            Joins.Add($" {tableName1} ON {tableName1}.{commonKey} = {tableName2}.{commonKey}");
            return this;
        }

        public SelectBuilder MakeJoin(string tableName, string join, string? tableNameKey)
        {
            Joins.Add($" {join}");
            Joins.Add($" {tableName} ON {this.tableName}.{tableNameKey} = {tableName}.{tableNameKey}");
            return this;
        }

        public SelectBuilder MakeJoin(ISQLModel model, string join)
        {
            string? key = model.GetTablePK()?.Name;
            Joins.Add($" {join}");
            Joins.Add($" {model.GetTableName()} ON {tableName}.{key} = {model.GetTableName()}.{key}");
            return this;
        }

        public SelectBuilder RightJoin(string tableName, string? key) => MakeJoin(tableName, "RIGHT JOIN", key);

        public SelectBuilder LeftJoin(string tableName, string? key) => MakeJoin(tableName, "LEFT JOIN", key);

        public SelectBuilder InnerJoin(string tableName, string? key) => MakeJoin(tableName, "INNER JOIN", key);

        public SelectBuilder RightJoin(ISQLModel model) => MakeJoin(model, "RIGHT JOIN");

        public SelectBuilder LeftJoin(ISQLModel model) => MakeJoin(model, "LEFT JOIN");

        public SelectBuilder InnerJoin(ISQLModel model) => MakeJoin(model,"INNER JOIN");

        public SelectBuilder InnerJoin(string tableName1, string tableName2, string key1, string key2) => MakeJoin("INNER JOIN",tableName1,tableName2,key1,key2);


        /// <summary>
        /// Adds '*' to the <c>SELECT</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder SelectAll() 
        {
            Fields.Add($"{tableName}.*");
            return this;
        }

        public SelectBuilder Sum(string field) 
        {
            Fields.Add($"sum({field})");
            return this;
        }
        public SelectBuilder CountAll()
        {
            Fields.Add($"Count(*)");
            return this;
        }

        /// <summary>
        /// Adds a set of fields to the <c>SELECT</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder SelectFields(params string[] fields)
        {
            if (Fields.Count > 0) 
                Fields.Add(", ");

            foreach (string s in fields) 
            {
                Fields.Add(s);
                Fields.Add(", ");
            }

            Fields.RemoveAt(Fields.Count-1);
            return this;
        }

        /// <summary>
        /// Adds a <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder Where()
        {
            WhereClause = " WHERE ";
            return this;
        }

        /// <summary>
        /// Adds a <c>LIKE</c> operator to the <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder Like(string field, string placeholder)
        {
            WhereCondition.Add($"{field} LIKE {placeholder}");
            return this;
        }

        public SelectBuilder IsNull(string field)
        {
            WhereCondition.Add($"{field} IS NULL");
            return this;
        }

        /// <summary>
        /// Adds a <c>=</c> condition to the <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder EqualsTo(string field, string placeholder)
        {
            WhereCondition.Add($"{field} = {placeholder}");
            return this;
        }

        /// <summary>
        /// Adds a <c>!=</c> condition to the <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder NotEquals(string field, string placeholder)
        {
            WhereCondition.Add($"{field} != {placeholder}");
            return this;
        }

        /// <summary>
        /// Adds a <c>></c> condition to the <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder Greater(string field, string placeholder)
        {
            WhereCondition.Add($"{field} > {placeholder}");
            return this;
        }

        /// <summary>
        /// Adds a <c>>=</c> condition to the <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder GreaterEquals(string field, string placeholder)
        {
            WhereCondition.Add($"{field} >= {placeholder}");
            return this;
        }

        /// <summary>
        /// Adds a <c>&lt;</c> condition to the <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder Smaller(string field, string placeholder)
        {
            WhereCondition.Add($"{field} < {placeholder}");
            return this;
        }

        /// <summary>
        /// Adds a <c>&lt;=</c> condition to the <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder SmallerEquals(string field, string placeholder)
        {
            WhereCondition.Add($"{field} <= {placeholder}");
            return this;
        }

        /// <summary>
        /// Adds a <c>OR</c> operator to the <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder OR()
        {
            WhereCondition.Add($" OR ");
            return this;
        }

        /// <summary>
        /// Adds a opened round bracket to the Statement. If called before <see cref="Where"/>, the bracket is added to the <c>FROM</c> clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder OpenBracket()
        {
            if (WhereClause.Length == 0 && Joins.Count == 0)
                _OpenBracket = "(";
            else if (WhereClause.Length == 0 && Joins.Count != 0)
                Joins.Add($"(");
            else WhereCondition.Add($"(");
            return this;
        }

        /// <summary>
        /// Adds a closed round bracket to the Statement. If called before <see cref="Where"/>, the bracket is added to the <c>FROM</c> clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder CloseBracket()
        {
            if (WhereClause.Length == 0)
            Joins.Add($")");
            else WhereCondition.Add($")");
            return this;
        }

        /// <summary>
        /// Adds a <c>AND</c> operator to the <c>WHERE</c> Clause. 
        /// </summary>
        /// <returns>A <see cref="SelectBuilder"/> object </returns>
        public SelectBuilder AND()
        {
            WhereCondition.Add($" AND ");
            return this;
        }

        public bool HasWhereClause() => this.WhereClause.Length > 0;

        public bool HasWhereConditons() => WhereCondition.Count > 0;

        public SelectBuilder RemoveLastWhereCondition()
        {
            WhereCondition.RemoveAt(WhereCondition.Count-1);
            return this;
        }

        public SelectBuilder ClearWhere()
        {
            this.WhereClause = string.Empty;
            return ClearWhereConditions();
        }
        public SelectBuilder ClearWhereConditions() 
        {
            WhereCondition.Clear();
            return this;
        }

        public override string? ToString()
        {
            return Statement();
        }

        public void Dispose()
        {
            Fields.Clear();
            Joins.Clear();
            WhereCondition.Clear();
            _parameters.Clear();
            GC.SuppressFinalize(this);
        }
    }
}