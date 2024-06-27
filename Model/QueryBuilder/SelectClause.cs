namespace Backend.Model
{
    public class SelectClause : AbstractClause
    {
        public override int Order => 1;
        public SelectClause() => Clauses.Add(this);
        public SelectClause(ISQLModel model) : base(model) => _bits.Add("SELECT");
        public SelectClause(AbstractClause clause, ISQLModel model) : this(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
        }
        public SelectClause All(string? tableName = null)
        {
            if (string.IsNullOrEmpty(tableName))
                tableName = this.TableName;

            _bits.Add($"{tableName}.*");
            return this;
        }

        public SelectClause AllExcept(params string[] fieldNames)
        {
            IEnumerable<string> fields = _model.GetEntityFieldNames();

            foreach (string field in fields)
            {
                if (fieldNames.Any(s => s.Equals(field))) continue;
                _bits.Add($"{TableName}.{field}");
            }
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
        public SelectClause Count(string field, string? alias = null) => FormulaAlias(field, alias);
        public SelectClause Sum(string field, string? alias = null) => FormulaAlias(field, alias);
        private SelectClause FormulaAlias(string field, string? alias = null)
        {
            if (string.IsNullOrEmpty(alias))
                _bits.Add($"sum({field})");
            else
                _bits.Add($"sum({field}) AS {alias}");
            return this;

        }
        public FromClause From(ISQLModel? model = null)
        {
            if (_bits.Count == 1) All();
            if (model!=null) _model = model;
            return new FromClause(this, _model);
        }
        public WhereClause Where() => From().Where();
        public GroupByClause GroupBy() => From().GroupBy();
        public OrderByClause OrderBy() => From().OrderBy();
        public LimitClause Limit(int limit = 1) => From().Limit(limit);
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
}