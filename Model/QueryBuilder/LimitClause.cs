namespace Backend.Model
{
    public class LimitClause : AbstractClause
    {
        public override int Order => 7;
        public LimitClause() { }
        public LimitClause(AbstractClause clause, ISQLModel model, int limit = 1) : base(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
            _bits.Add($"LIMIT {limit}");
        }
    }
}
