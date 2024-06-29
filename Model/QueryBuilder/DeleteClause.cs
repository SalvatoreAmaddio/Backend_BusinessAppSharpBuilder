namespace Backend.Model
{
    /// <summary>
    /// Represents the interface for a DELETE clause in an SQL query.
    /// </summary>
    public interface IDeleteClause
    {
        /// <summary>
        /// Adds a FROM clause to the DELETE query.
        /// </summary>
        /// <returns>A new instance of <see cref="FromClause"/> associated with the current DELETE query.</returns>
        public FromClause From();

        /// <summary>
        /// Adds a WHERE clause to the DELETE query.
        /// </summary>
        /// <returns>A new instance of <see cref="WhereClause"/> associated with the current DELETE query.</returns>
        public WhereClause Where();
    }

    /// <summary>
    /// Represents a DELETE clause in an SQL query.
    /// </summary>
    public class DeleteClause : AbstractClause, IDeleteClause
    {
        public override int Order => 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteClause"/> class.
        /// </summary>
        public DeleteClause() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteClause"/> class with the specified model.
        /// </summary>
        /// <param name="model">The SQL model associated with the clause.</param>
        public DeleteClause(ISQLModel model) : base(model) => _bits.Add("DELETE");

        /// <summary>
        /// Adds a FROM clause to the DELETE query.
        /// </summary>
        /// <returns>A new instance of <see cref="FromClause"/> associated with the current DELETE query.</returns>
        public FromClause From() => new FromClause(this, _model);

        /// <summary>
        /// Adds a WHERE clause to the DELETE query.
        /// </summary>
        /// <returns>A new instance of <see cref="WhereClause"/> associated with the current DELETE query.</returns>
        public WhereClause Where() => new WhereClause(this, _model);
    }
}
