using Microsoft.Office.Interop.Excel;
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


    public class SelectBuilder(ISQLModel model) 
    {
        private string WhereClause = string.Empty;
        private readonly List<string> Fields = [];
        private readonly List<string> Joins = [];
        private readonly List<string> WhereCondition = [];
        private ISQLModel model = model;
        private string tableName => model.GetTableName();

        public string Statement()
        {
            StringBuilder sb = new();
            sb.Append($"SELECT ");

            if (Fields.Count == 0)
                sb.Append('*');
            else
            {
                foreach (string s in Fields)
                {
                    sb.Append(s);
                }
            }

            sb.Append($" FROM {tableName}");
            
            foreach (string s in Joins)
            {
                sb.Append(s);
            }

            if (WhereClause.Length > 0)
            {
                sb.Append(WhereClause);
            }

            foreach (string s in WhereCondition)
            {
                sb.Append(s);
            }

            WhereCondition.Clear();
            Joins.Clear();
            return sb.ToString();
        }
        private SelectBuilder MakeJoin(ISQLModel model, string join)
        {
            string? key = model.GetTablePK()?.Name;
            Joins.Add($" {join}");
            Joins.Add($" {model.GetTableName()} ON {tableName}.{key} = {model.GetTableName()}.{key}");
            return this;
        }

        public SelectBuilder SelectAll() 
        {
            Fields.Add("*");
            return this;
        }

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

        public SelectBuilder InnerJoin(ISQLModel model) => MakeJoin(model, "INNER JOIN");
        public SelectBuilder RightJoin(ISQLModel model) => MakeJoin(model, "RIGHT JOIN");
        public SelectBuilder LeftJoin(ISQLModel model) => MakeJoin(model, "LEFT JOIN");
        public SelectBuilder Where()
        {
            WhereClause = " WHERE ";
            return this;
        }
        public SelectBuilder Like(string field, string placeholder)
        {
            string? key = model.GetTablePK()?.Name;
            WhereCondition.Add($"{field} LIKE {placeholder}");
            return this;
        }
        public SelectBuilder Equals(string field, string placeholder)
        {
            string? key = model.GetTablePK()?.Name;
            WhereCondition.Add($"{field} = {placeholder}");
            return this;
        }
        public SelectBuilder NotEquals(string field, string placeholder)
        {
            string? key = model.GetTablePK()?.Name;
            WhereCondition.Add($"{field} != {placeholder}");
            return this;
        }
        public SelectBuilder Greater(string field, string placeholder)
        {
            string? key = model.GetTablePK()?.Name;
            WhereCondition.Add($"{field} > {placeholder}");
            return this;
        }
        public SelectBuilder GreaterEquals(string field, string placeholder)
        {
            string? key = model.GetTablePK()?.Name;
            WhereCondition.Add($"{field} >= {placeholder}");
            return this;
        }
        public SelectBuilder Smaller(string field, string placeholder)
        {
            string? key = model.GetTablePK()?.Name;
            WhereCondition.Add($"{field} < {placeholder}");
            return this;
        }
        public SelectBuilder SmallerEquals(string field, string placeholder)
        {
            string? key = model.GetTablePK()?.Name;
            WhereCondition.Add($"{field} <= {placeholder}");
            return this;
        }
        public SelectBuilder OR()
        {
            WhereCondition.Add($" OR ");
            return this;
        }
        public SelectBuilder OpenBracket()
        {
            if (WhereClause.Length == 0) 
            Joins.Add($"(");
            else WhereCondition.Add($"(");
            return this;
        }
        public SelectBuilder CloseBracket()
        {
            if (WhereClause.Length == 0)
            Joins.Add($")");
            else WhereCondition.Add($")");
            return this;
        }
        public SelectBuilder AND()
        {
            WhereCondition.Add($" AND ");
            return this;
        }
    }
}