namespace Backend.Model
{
    /// <summary>
    /// Represents a FROM clause in an SQL query.
    /// </summary>
    public class FromClause : AbstractClause
    {
        public override int Order => 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="FromClause"/> class.
        /// </summary>
        public FromClause() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FromClause"/> class with the specified clause and model.
        /// </summary>
        /// <param name="clause">The existing clause to transfer data from.</param>
        /// <param name="model">The SQL model associated with the clause.</param>
        public FromClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
            _bits.Add("FROM");
            _bits.Add(TableName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FromClause"/> class with the specified model.
        /// </summary>
        /// <param name="model">The SQL model associated with the clause.</param>
        public FromClause(ISQLModel model) : base(model)
        {
            Clauses.Add(new SelectClause(model).All());
            _bits.Add("FROM");
            _bits.Add(TableName);
        }

        /// <summary>
        /// Creates a JOIN between the specified table's primary key and this table's foreign key.
        /// </summary>
        /// <param name="join">The type of join (e.g., "INNER JOIN").</param>
        /// <param name="toTable">The table to join to.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        /// <exception cref="Exception">Thrown if the primary key of the target table is null.</exception>
        private FromClause MakeJoin(string join, ISQLModel toTable)
        {
            string commonKey = toTable?.GetPrimaryKey()?.Name ?? throw new Exception("Null Reference");
            return MakeJoin(join, toTable.GetTableName(), commonKey);
        }

        /// <summary>
        /// Creates a JOIN between the specified table and this table based on a common key.
        /// </summary>
        /// <param name="join">The type of join (e.g., "INNER JOIN").</param>
        /// <param name="toTable">The table to join to.</param>
        /// <param name="commonKey">The name of the common key in both tables.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        private FromClause MakeJoin(string join, string toTable, string commonKey)
        {
            _bits.Add(join);
            _bits.Add(toTable);
            _bits.Add("ON");
            _bits.Add($"{this.TableName}.{commonKey}");
            _bits.Add("=");
            _bits.Add($"{toTable}.{commonKey}");
            return this;
        }

        /// <summary>
        /// Creates a JOIN between two given tables based on a common key in both tables.
        /// </summary>
        /// <param name="join">The type of join (e.g., "INNER JOIN").</param>
        /// <param name="fromTable">The table to join from.</param>
        /// <param name="toTable">The table to join to.</param>
        /// <param name="commonKey">The name of the common key in both tables.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        private FromClause MakeJoin(string join, string fromTable, string toTable, string commonKey)
        {
            _bits.Add(join);
            _bits.Add(toTable);
            _bits.Add("ON");
            _bits.Add($"{fromTable}.{commonKey}");
            _bits.Add("=");
            _bits.Add($"{toTable}.{commonKey}");
            return this;
        }

        #region INNER JOIN
        /// <summary>
        /// Adds an INNER JOIN to the query.
        /// </summary>
        /// <param name="toTable">The table to join to.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        public FromClause InnerJoin(ISQLModel toTable) => MakeJoin("INNER JOIN", toTable);

        /// <summary>
        /// Adds an INNER JOIN to the query based on a common key.
        /// </summary>
        /// <param name="toTable">The table to join to.</param>
        /// <param name="commonKey">The common key in both tables.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        public FromClause InnerJoin(string toTable, string commonKey) => MakeJoin("INNER JOIN", toTable, commonKey);

        /// <summary>
        /// Adds an INNER JOIN to the query between two specified tables based on a common key.
        /// </summary>
        /// <param name="fromTable">The table to join from.</param>
        /// <param name="toTable">The table to join to.</param>
        /// <param name="commonKey">The common key in both tables.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        public FromClause InnerJoin(string fromTable, string toTable, string commonKey) => MakeJoin("INNER JOIN", fromTable, toTable, commonKey);
        #endregion

        #region RIGHT JOIN
        /// <summary>
        /// Adds a RIGHT JOIN to the query.
        /// </summary>
        /// <param name="toTable">The table to join to.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        public FromClause RightJoin(ISQLModel toTable) => MakeJoin("RIGHT JOIN", toTable);

        /// <summary>
        /// Adds a RIGHT JOIN to the query based on a common key.
        /// </summary>
        /// <param name="toTable">The table to join to.</param>
        /// <param name="commonKey">The common key in both tables.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        public FromClause RightJoin(string toTable, string commonKey) => MakeJoin("RIGHT JOIN", toTable, commonKey);

        /// <summary>
        /// Adds a RIGHT JOIN to the query between two specified tables based on a common key.
        /// </summary>
        /// <param name="fromTable">The table to join from.</param>
        /// <param name="toTable">The table to join to.</param>
        /// <param name="commonKey">The common key in both tables.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        public FromClause RightJoin(string fromTable, string toTable, string commonKey) => MakeJoin("RIGHT JOIN", fromTable, toTable, commonKey);
        #endregion

        #region LEFT JOIN
        /// <summary>
        /// Adds a LEFT JOIN to the query.
        /// </summary>
        /// <param name="toTable">The table to join to.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        public FromClause LeftJoin(ISQLModel toTable) => MakeJoin("LEFT JOIN", toTable);

        /// <summary>
        /// Adds a LEFT JOIN to the query based on a common key.
        /// </summary>
        /// <param name="toTable">The table to join to.</param>
        /// <param name="commonKey">The common key in both tables.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        public FromClause LeftJoin(string toTable, string commonKey) => MakeJoin("LEFT JOIN", toTable, commonKey);

        /// <summary>
        /// Adds a LEFT JOIN to the query between two specified tables based on a common key.
        /// </summary>
        /// <param name="fromTable">The table to join from.</param>
        /// <param name="toTable">The table to join to.</param>
        /// <param name="commonKey">The common key in both tables.</param>
        /// <returns>The current instance of <see cref="FromClause"/> with the added join.</returns>
        public FromClause LeftJoin(string fromTable, string toTable, string commonKey) => MakeJoin("LEFT JOIN", fromTable, toTable, commonKey);
        #endregion

        /// <summary>
        /// Adds a closing bracket to the FROM clause.
        /// </summary>
        /// <returns>The current instance of <see cref="FromClause"/> with the added closing bracket.</returns>
        public FromClause CloseBracket()
        {
            _bits.Add(")");
            return this;
        }

        /// <summary>
        /// Adds an opening bracket to the FROM clause.
        /// </summary>
        /// <returns>The current instance of <see cref="FromClause"/> with the added opening bracket.</returns>
        public FromClause OpenBracket()
        {
            _bits.Insert(1, "(");
            return this;
        }

        /// <summary>
        /// Adds a WHERE clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="WhereClause"/> associated with the current query.</returns>
        public WhereClause Where() => new WhereClause(this, _model);

        /// <summary>
        /// Adds a GROUP BY clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="GroupByClause"/> associated with the current query.</returns>
        public GroupByClause GroupBy() => new GroupByClause(this, _model);

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
        /// Returns a string representation of the FROM clause.
        /// </summary>
        /// <returns>A string indicating that this is a FROM clause.</returns>
        public override string ToString() => "FROM CLAUSE";
    }

}
