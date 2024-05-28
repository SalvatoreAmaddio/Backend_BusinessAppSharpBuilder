using System.Text;

namespace Backend.Model
{
    /// <summary>
    /// This class build default queries for <see cref="AbstractSQLModel"/> objects.
    /// </summary>
    public class QueryBuilder
    {
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
}