namespace Backend.Model
{
    #region Select
    public class SelectClause : AbstractClause, ISelectClause
    {
        public override int Order => 1;
        public SelectClause() => Clauses.Add(this);
        public SelectClause(ISQLModel model) : base(model) => _bits.Add("SELECT");
        public SelectClause(InsertClause clause, ISQLModel model) : this(model)
        {
            Clauses = clause.Clauses;
            _parameters = clause._parameters;
            Clauses.Add(this);
        }
        public SelectClause All(string? tableName = null)
        {
            if (string.IsNullOrEmpty(tableName))
                tableName = this.TableName;

            _bits.Add($"{tableName}.*");
            return this;
        }
        public SelectClause Fields(params string[] fields)
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
        public FromClause From(ISQLModel? model = null)
        {
            if (_bits.Count == 1) All();
            if (model!=null) _model = model;
            return new FromClause(this, _model);
        }
        public WhereClause Where() => From().Where();
        public override string AsString()
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

        public override string ToString() => "SELECT CLAUSE";

    }
    public interface ISelectClause : IQueryClause
    {
        public SelectClause Sum(string field);
        public SelectClause CountAll();
        public SelectClause All(string? tableName = null);
        public SelectClause Fields(params string[] fields);
        public FromClause From(ISQLModel? model = null);
        public WhereClause Where();
        public SelectClause Distinct();
    }

    #endregion

}
