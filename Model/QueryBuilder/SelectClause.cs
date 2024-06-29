namespace Backend.Model
{
    /// <summary>
    /// Represents a SELECT clause in an SQL query.
    /// </summary>
    public class SelectClause : AbstractClause
    {
        public override int Order => 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectClause"/> class.
        /// </summary>
        public SelectClause() => Clauses.Add(this);

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectClause"/> class with the specified model.
        /// </summary>
        /// <param name="model">The SQL model associated with the clause.</param>
        public SelectClause(ISQLModel model) : base(model) => _bits.Add("SELECT");

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectClause"/> class with the specified clause and model.
        /// </summary>
        /// <param name="clause">The existing clause to transfer data from.</param>
        /// <param name="model">The SQL model associated with the clause.</param>
        public SelectClause(AbstractClause clause, ISQLModel model) : this(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
        }

        /// <summary>
        /// Selects all fields from the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table. If null, the table associated with the model is used.</param>
        /// <returns>The current instance of <see cref="SelectClause"/> with the added selection.</returns>
        public SelectClause All(string? tableName = null)
        {
            if (string.IsNullOrEmpty(tableName))
                tableName = this.TableName;

            _bits.Add($"{tableName}.*");
            return this;
        }

        /// <summary>
        /// Selects all fields except the specified field names.
        /// </summary>
        /// <param name="fieldNames">The field names to exclude from the selection.</param>
        /// <returns>The current instance of <see cref="SelectClause"/> with the added selection.</returns>
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

        /// <summary>
        /// Selects the specified fields.
        /// </summary>
        /// <param name="fields">The fields to select.</param>
        /// <returns>The current instance of <see cref="SelectClause"/> with the added selection.</returns>
        public SelectClause Fields(params string[] fields)
        {
            foreach (var field in fields)
                _bits.Add($"{field}");
            return this;
        }

        /// <summary>
        /// Selects distinct values.
        /// </summary>
        /// <returns>The current instance of <see cref="SelectClause"/> with the DISTINCT keyword added.</returns>
        public SelectClause Distinct()
        {
            _bits.Add("DISTINCT");
            return this;
        }

        /// <summary>
        /// Selects a count of all records.
        /// </summary>
        /// <returns>The current instance of <see cref="SelectClause"/> with the count of all records added.</returns>
        public SelectClause CountAll()
        {
            _bits.Add("Count(*)");
            return this;
        }

        /// <summary>
        /// Selects a count of the specified field with an optional alias.
        /// </summary>
        /// <param name="field">The field to count.</param>
        /// <param name="alias">The optional alias for the count result.</param>
        /// <returns>The current instance of <see cref="SelectClause"/> with the count of the specified field added.</returns>
        public SelectClause Count(string field, string? alias = null) => FormulaAlias(field, alias);

        /// <summary>
        /// Selects the sum of the specified field with an optional alias.
        /// </summary>
        /// <param name="field">The field to sum.</param>
        /// <param name="alias">The optional alias for the sum result.</param>
        /// <returns>The current instance of <see cref="SelectClause"/> with the sum of the specified field added.</returns>
        public SelectClause Sum(string field, string? alias = null) => FormulaAlias(field, alias);

        /// <summary>
        /// Adds a formula with an optional alias to the selection.
        /// </summary>
        /// <param name="field">The field to apply the formula to.</param>
        /// <param name="alias">The optional alias for the formula result.</param>
        /// <returns>The current instance of <see cref="SelectClause"/> with the formula added.</returns>
        private SelectClause FormulaAlias(string field, string? alias = null)
        {
            if (string.IsNullOrEmpty(alias))
                _bits.Add($"sum({field})");
            else
                _bits.Add($"sum({field}) AS {alias}");
            return this;
        }

        /// <summary>
        /// Adds a FROM clause to the query.
        /// </summary>
        /// <param name="model">The SQL model associated with the FROM clause. If null, the current model is used.</param>
        /// <returns>A new instance of <see cref="FromClause"/> associated with the current query.</returns>
        public FromClause From(ISQLModel? model = null)
        {
            if (_bits.Count == 1) All();
            if (model != null) _model = model;
            return new FromClause(this, _model);
        }

        /// <summary>
        /// Adds a WHERE clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="WhereClause"/> associated with the current query.</returns>
        public WhereClause Where() => From().Where();

        /// <summary>
        /// Adds a GROUP BY clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="GroupByClause"/> associated with the current query.</returns>
        public GroupByClause GroupBy() => From().GroupBy();

        /// <summary>
        /// Adds an ORDER BY clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="OrderByClause"/> associated with the current query.</returns>
        public OrderByClause OrderBy() => From().OrderBy();

        /// <summary>
        /// Adds a LIMIT clause to the query.
        /// </summary>
        /// <param name="limit">The maximum number of records to return. Default is 1.</param>
        /// <returns>A new instance of <see cref="LimitClause"/> associated with the current query.</returns>
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

        /// <summary>
        /// Returns a string representation of the SELECT clause.
        /// </summary>
        /// <returns>A string indicating that this is a SELECT clause.</returns>
        public override string ToString() => "SELECT CLAUSE";
    }

}