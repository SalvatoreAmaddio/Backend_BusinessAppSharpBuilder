namespace Backend.Model
{
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

}
