namespace Backend.Model
{
    /// <summary>
    /// Represents a GROUP BY clause in an SQL query.
    /// </summary>
    public class GroupByClause : AbstractClause
    {
        public override int Order => 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupByClause"/> class.
        /// </summary>
        public GroupByClause() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupByClause"/> class with the specified clause and model.
        /// </summary>
        /// <param name="clause">The existing clause to transfer data from.</param>
        /// <param name="model">The SQL model associated with the clause.</param>
        public GroupByClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
            _bits.Add("GROUP BY");
        }

        /// <summary>
        /// Specifies the fields to group by in the GROUP BY clause.
        /// </summary>
        /// <param name="fields">The fields to group by.</param>
        /// <returns>The current instance of <see cref="GroupByClause"/> with the specified fields.</returns>
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

        /// <summary>
        /// Adds a HAVING clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="HavingClause"/> associated with the current query.</returns>
        public HavingClause Having() => new HavingClause(this, _model);

        /// <summary>
        /// Adds an ORDER BY clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="OrderByClause"/> associated with the current query.</returns>
        public OrderByClause OrderBy() => new OrderByClause(this, _model);

        /// <summary>
        /// Adds a LIMIT clause to the query.
        /// </summary>
        /// <param name="limit">The maximum number of records to return. Default is 1.</param>
        /// <returns>A new instance of <see cref="LimitClause"/> associated with the current query.</returns>
        public LimitClause Limit(int limit = 1) => new LimitClause(this, _model, limit);

        /// <summary>
        /// Returns a string representation of the GROUP BY clause.
        /// </summary>
        /// <returns>A string indicating that this is a GROUP BY clause.</returns>
        public override string ToString() => "GROUP BY CLAUSE";
    }

}
