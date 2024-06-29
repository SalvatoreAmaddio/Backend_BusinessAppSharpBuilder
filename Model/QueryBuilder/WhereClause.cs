namespace Backend.Model
{
    /// <summary>
    /// Represents a WHERE clause in an SQL query.
    /// </summary>
    public class WhereClause : AbstractConditionalClause
    {
        public override int Order => 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhereClause"/> class.
        /// </summary>
        public WhereClause() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhereClause"/> class with the specified clause and model.
        /// </summary>
        /// <param name="clause">The existing clause to transfer data from.</param>
        /// <param name="model">The SQL model associated with the clause.</param>
        public WhereClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
            _bits.Add("WHERE");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhereClause"/> class with the specified model.
        /// </summary>
        /// <param name="model">The SQL model associated with the clause.</param>
        public WhereClause(ISQLModel model) : base(model)
        {
            Clauses.Add(new SelectClause(model).All().From());
            _bits.Add("WHERE");
        }

        /// <summary>
        /// Adds a condition to the WHERE clause that the primary key equals a specified parameter.
        /// </summary>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public WhereClause This() => this.EqualsTo(TableKey, $"@{TableKey}");

        /// <summary>
        /// Adds a BETWEEN condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value1">The lower bound of the range.</param>
        /// <param name="value2">The upper bound of the range.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public WhereClause Between(string field, string value1, string value2)
        {
            _bits.Add($"{field} BETWEEN {value1} AND {value2}");
            return this;
        }

        /// <summary>
        /// Adds an IN condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="values">The values to include in the IN condition.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public WhereClause In(string field, params string[] values)
        {
            _bits.Add($"{field} IN (");

            foreach (string value in values)
            {
                _bits.Add(value);
                _bits.Add(", ");
            }

            RemoveLastChange(); // remove last comma
            _bits.Add(")");
            return this;
        }

        /// <summary>
        /// Adds a LIKE condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public WhereClause Like(string field, string value) => Condition(field, value, "LIKE");

        /// <summary>
        /// Adds an equality condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public new WhereClause EqualsTo(string field, string value) => Condition(field, value, "=");

        /// <summary>
        /// Adds a not-equal condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public new WhereClause NotEqualsTo(string field, string value) => Condition(field, value, "!=");

        /// <summary>
        /// Adds a greater-than condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public new WhereClause GreaterThan(string field, string value) => Condition(field, value, ">");

        /// <summary>
        /// Adds a greater-than-or-equal-to condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public new WhereClause GreaterEqualTo(string field, string value) => Condition(field, value, ">=");

        /// <summary>
        /// Adds a less-than condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public new WhereClause SmallerThan(string field, string value) => Condition(field, value, "<");

        /// <summary>
        /// Adds a less-than-or-equal-to condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public new WhereClause SmallerEqualTo(string field, string value) => Condition(field, value, "<=");

        /// <summary>
        /// Adds a custom condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <param name="oprt">The operator to use in the comparison (e.g., "=", "!=", ">", "<").</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        private new WhereClause Condition(string field, string value, string oprt) => (WhereClause)base.Condition(field, value, oprt);

        /// <summary>
        /// Adds an IS NULL condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to check for null.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public new WhereClause IsNull(string field) => (WhereClause)base.IsNull(field);

        /// <summary>
        /// Adds an IS NOT NULL condition to the WHERE clause.
        /// </summary>
        /// <param name="field">The field to check for non-null.</param>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added condition.</returns>
        public new WhereClause IsNotNull(string field) => (WhereClause)base.IsNotNull(field);

        /// <summary>
        /// Adds an OR logical operator to the WHERE clause.
        /// </summary>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added OR operator.</returns>
        public new WhereClause OR() => (WhereClause)LogicalOperator("OR");

        /// <summary>
        /// Adds an AND logical operator to the WHERE clause.
        /// </summary>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added AND operator.</returns>
        public new WhereClause AND() => (WhereClause)LogicalOperator("AND");

        /// <summary>
        /// Adds a NOT logical operator to the WHERE clause.
        /// </summary>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added NOT operator.</returns>
        public new WhereClause NOT() => (WhereClause)LogicalOperator("NOT");

        /// <summary>
        /// Adds an opening bracket to the WHERE clause.
        /// </summary>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added opening bracket.</returns>
        public new WhereClause OpenBracket() => (WhereClause)base.OpenBracket();

        /// <summary>
        /// Adds a closing bracket to the WHERE clause.
        /// </summary>
        /// <returns>The current instance of <see cref="WhereClause"/> with the added closing bracket.</returns>
        public new WhereClause CloseBracket() => (WhereClause)base.CloseBracket();


        /// <summary>
        /// Adds a GROUP BY clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="GroupByClause"/> associated with the current query.</returns>
        public GroupByClause GroupBy() => new GroupByClause(this, _model);

        /// <summary>
        /// Returns a string representation of the WHERE clause.
        /// </summary>
        /// <returns>A string indicating that this is a WHERE clause.</returns>
        public override string ToString() => "WHERE CLAUSE";
    }

}
