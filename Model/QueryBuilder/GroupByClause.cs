namespace Backend.Model
{
    public class GroupByClause : AbstractClause
    {
        public override int Order => 4;
        public GroupByClause() { }
        public GroupByClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
            _bits.Add("GROUP BY");
        }

        public GroupByClause Fields(params string[] fields)
        {
            foreach (string field in fields)
            {
                _bits.Add(field);
                _bits.Add(",");
            }
            RemoveLastChange();
            return this;
        }

        public HavingClause Having() => new(this, _model);
        public OrderByClause OrderBy() => new(this, _model);
        public LimitClause Limit(int limit = 1) => new(this, _model, limit);
        public override string ToString() => "GROUP BY CLAUSE";

    }

}
