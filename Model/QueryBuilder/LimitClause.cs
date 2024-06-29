namespace Backend.Model
{
    /// <summary>
    /// Represents a LIMIT clause in an SQL query.
    /// </summary>
    public class LimitClause : AbstractClause
    {
        public override int Order => 7;

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitClause"/> class.
        /// </summary>
        public LimitClause() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitClause"/> class with the specified clause, model, and limit.
        /// </summary>
        /// <param name="clause">The existing clause to transfer data from.</param>
        /// <param name="model">The SQL model associated with the clause.</param>
        /// <param name="limit">The maximum number of records to return. Default is 1.</param>
        public LimitClause(AbstractClause clause, ISQLModel model, int limit = 1) : base(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
            _bits.Add($"LIMIT {limit}");
        }
    }
}