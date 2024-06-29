namespace Backend.Model
{
    /// <summary>
    /// Represents the interface for an UPDATE clause in an SQL query.
    /// </summary>
    public interface IUpdateClause : IQueryClause
    {
        /// <summary>
        /// Specifies all fields to be updated in the UPDATE statement.
        /// </summary>
        /// <returns>An instance of <see cref="UpdateClause"/> with all fields specified for the update.</returns>
        public UpdateClause AllFields();

        /// <summary>
        /// Adds a WHERE clause to the UPDATE query.
        /// </summary>
        /// <returns>A new instance of <see cref="WhereClause"/> associated with the current UPDATE query.</returns>
        public WhereClause Where();
    }

    /// <summary>
    /// Represents an UPDATE clause in an SQL query.
    /// </summary>
    public class UpdateClause : AbstractClause, IUpdateClause
    {
        public override int Order => 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateClause"/> class.
        /// </summary>
        public UpdateClause() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateClause"/> class with the specified model.
        /// </summary>
        /// <param name="model">The SQL model associated with the clause.</param>
        public UpdateClause(ISQLModel model) : base(model) => _bits.Add($"UPDATE {model.GetTableName()}");

        /// <summary>
        /// Specifies all fields to be updated in the UPDATE statement, excluding the primary key.
        /// </summary>
        /// <returns>The current instance of <see cref="UpdateClause"/> with all fields specified for the update.</returns>
        public UpdateClause AllFields()
        {
            string pkName = _model.GetPrimaryKey()?.Name ?? "";
            _bits.Add("SET");

            foreach (string fieldName in _model.GetEntityFieldNames())
            {
                if (pkName.Equals(fieldName)) continue;
                _bits.Add($"{fieldName} = @{fieldName}");
                _bits.Add(",");
            }
            RemoveLastChange();
            return this;
        }

        /// <summary>
        /// Adds a WHERE clause to the UPDATE query.
        /// </summary>
        /// <returns>A new instance of <see cref="WhereClause"/> associated with the current UPDATE query.</returns>
        public WhereClause Where() => new WhereClause(this, _model);
    }

}
