namespace Backend.Model
{
    public class OrderByClause : AbstractClause
    {
        public override int Order => 6;
        public OrderByClause() { }
        public OrderByClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
            _bits.Add("ORDER BY");
        }
        public GroupByClause GroupBy() => new(this, _model);

        public OrderByClause Field(string field)
        {
            _bits.Add(field);
            return this;
        }

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
        public LimitClause Limit(int limit = 1) => new(this, _model, limit);
        public override string ToString() => "ORDER BY CLAUSE";

    }

}
