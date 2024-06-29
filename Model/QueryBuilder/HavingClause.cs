namespace Backend.Model
{
    /// <summary>
    /// Represents a HAVING clause in an SQL query.
    /// </summary>
    public class HavingClause : AbstractConditionalClause
    {
        public override int Order => 5;

        /// <summary>
        /// Initializes a new instance of the <see cref="HavingClause"/> class.
        /// </summary>
        public HavingClause() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HavingClause"/> class with the specified clause and model.
        /// </summary>
        /// <param name="clause">The existing clause to transfer data from.</param>
        /// <param name="model">The SQL model associated with the clause.</param>
        public HavingClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            TransferClauses(ref clause);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
            _bits.Add("HAVING");
        }

        /// <summary>
        /// Adds an equality condition to the HAVING clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added condition.</returns>
        public new HavingClause EqualsTo(string field, string value) => Condition(field, value, "=");

        /// <summary>
        /// Adds a not-equal condition to the HAVING clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added condition.</returns>
        public new HavingClause NotEqualsTo(string field, string value) => Condition(field, value, "!=");

        /// <summary>
        /// Adds a greater-than condition to the HAVING clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added condition.</returns>
        public new HavingClause GreaterThan(string field, string value) => Condition(field, value, ">");

        /// <summary>
        /// Adds a greater-than-or-equal-to condition to the HAVING clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added condition.</returns>
        public new HavingClause GreaterEqualTo(string field, string value) => Condition(field, value, ">=");

        /// <summary>
        /// Adds a less-than condition to the HAVING clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added condition.</returns>
        public new HavingClause SmallerThan(string field, string value) => Condition(field, value, "<");

        /// <summary>
        /// Adds a less-than-or-equal-to condition to the HAVING clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added condition.</returns>
        public new HavingClause SmallerEqualTo(string field, string value) => Condition(field, value, "<=");

        /// <summary>
        /// Adds a custom condition to the HAVING clause.
        /// </summary>
        /// <param name="field">The field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <param name="oprt">The operator to use in the comparison (e.g., "=", "!=", ">", "<").</param>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added condition.</returns>
        private new HavingClause Condition(string field, string value, string oprt) => (HavingClause)base.Condition(field, value, oprt);

        /// <summary>
        /// Adds an IS NULL condition to the HAVING clause.
        /// </summary>
        /// <param name="field">The field to check for null.</param>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added condition.</returns>
        public new HavingClause IsNull(string field) => (HavingClause)base.IsNull(field);

        /// <summary>
        /// Adds an IS NOT NULL condition to the HAVING clause.
        /// </summary>
        /// <param name="field">The field to check for non-null.</param>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added condition.</returns>
        public new HavingClause IsNotNull(string field) => (HavingClause)base.IsNotNull(field);

        /// <summary>
        /// Adds an OR logical operator to the HAVING clause.
        /// </summary>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added OR operator.</returns>
        public new HavingClause OR() => (HavingClause)LogicalOperator("OR");

        /// <summary>
        /// Adds an AND logical operator to the HAVING clause.
        /// </summary>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added AND operator.</returns>
        public new HavingClause AND() => (HavingClause)LogicalOperator("AND");

        /// <summary>
        /// Adds a NOT logical operator to the HAVING clause.
        /// </summary>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added NOT operator.</returns>
        public new HavingClause NOT() => (HavingClause)LogicalOperator("NOT");

        /// <summary>
        /// Adds an opening bracket to the HAVING clause.
        /// </summary>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added opening bracket.</returns>
        public new HavingClause OpenBracket() => (HavingClause)base.OpenBracket();

        /// <summary>
        /// Adds a closing bracket to the HAVING clause.
        /// </summary>
        /// <returns>The current instance of <see cref="HavingClause"/> with the added closing bracket.</returns>
        public new HavingClause CloseBracket() => (HavingClause)base.CloseBracket();

        /// <summary>
        /// Returns a string representation of the HAVING clause.
        /// </summary>
        /// <returns>A string indicating that this is a HAVING clause.</returns>
        public override string ToString() => "HAVING CLAUSE";
    }

}
