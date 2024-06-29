namespace Backend.Model
{
    /// <summary>
    /// Represents an ORDER BY clause in an SQL query.
    /// </summary>
    public class OrderByClause : AbstractClause
    {
        public override int Order => 6;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByClause"/> class.
        /// </summary>
        public OrderByClause() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByClause"/> class with the specified clause and model.
        /// </summary>
        /// <param name="clause">The existing clause to transfer data from.</param>
        /// <param name="model">The SQL model associated with the clause.</param>
        public OrderByClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
            _bits.Add("ORDER BY");
        }

        /// <summary>
        /// Adds a GROUP BY clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="GroupByClause"/> associated with the current query.</returns>
        public GroupByClause GroupBy() => new GroupByClause(this, _model);

        /// <summary>
        /// Specifies the field to order by in the ORDER BY clause.
        /// </summary>
        /// <param name="field">The field to order by.</param>
        /// <returns>The current instance of <see cref="OrderByClause"/> with the specified field.</returns>
        public OrderByClause Field(string field)
        {
            _bits.Add(field);
            return this;
        }

        /// <summary>
        /// Converts the ORDER BY clause to its string representation.
        /// </summary>
        /// <returns>A string representing the ORDER BY clause.</returns>
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

        /// <summary>
        /// Adds a LIMIT clause to the query.
        /// </summary>
        /// <param name="limit">The maximum number of records to return. Default is 1.</param>
        /// <returns>A new instance of <see cref="LimitClause"/> associated with the current query.</returns>
        public LimitClause Limit(int limit = 1) => new LimitClause(this, _model, limit);

        /// <summary>
        /// Returns a string representation of the ORDER BY clause.
        /// </summary>
        /// <returns>A string indicating that this is an ORDER BY clause.</returns>
        public override string ToString() => "ORDER BY CLAUSE";
    }
}