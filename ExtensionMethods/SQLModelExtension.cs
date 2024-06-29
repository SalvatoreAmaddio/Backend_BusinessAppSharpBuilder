using Backend.Model;

namespace Backend.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="ISQLModel"/> to build SQL clauses.
    /// </summary>
    public static class SQLModelExtension
    {
        /// <summary>
        /// Creates a SELECT clause with the DISTINCT keyword for the specified model.
        /// </summary>
        /// <param name="model">The SQL model.</param>
        /// <returns>A <see cref="SelectClause"/> object with the DISTINCT keyword.</returns>
        public static SelectClause Distinct(this ISQLModel model) => new SelectClause(model).Distinct();

        /// <summary>
        /// Creates a SELECT clause for the specified fields in the model.
        /// </summary>
        /// <param name="model">The SQL model.</param>
        /// <param name="fields">The fields to select.</param>
        /// <returns>A <see cref="SelectClause"/> object with the specified fields.</returns>
        public static SelectClause Select(this ISQLModel model, params string[] fields) => new SelectClause(model).Fields(fields);

        /// <summary>
        /// Creates a SELECT clause with the SUM aggregate function for the specified field in the model.
        /// </summary>
        /// <param name="model">The SQL model.</param>
        /// <param name="field">The field to sum.</param>
        /// <returns>A <see cref="SelectClause"/> object with the SUM aggregate function.</returns>
        public static SelectClause Sum(this ISQLModel model, string field) => new SelectClause(model).Sum(field);

        /// <summary>
        /// Creates a SELECT clause with the COUNT(*) function for the specified model.
        /// </summary>
        /// <param name="model">The SQL model.</param>
        /// <returns>A <see cref="SelectClause"/> object with the COUNT(*) function.</returns>
        public static SelectClause CountAll(this ISQLModel model) => new SelectClause(model).CountAll();

        /// <summary>
        /// Creates an INSERT clause for the specified model.
        /// </summary>
        /// <param name="model">The SQL model.</param>
        /// <returns>An <see cref="InsertClause"/> object for the model.</returns>
        public static InsertClause Insert(this ISQLModel model) => new InsertClause(model);

        /// <summary>
        /// Creates an UPDATE clause for the specified model.
        /// </summary>
        /// <param name="model">The SQL model.</param>
        /// <returns>An <see cref="UpdateClause"/> object for the model.</returns>
        public static UpdateClause Update(this ISQLModel model) => new UpdateClause(model);

        /// <summary>
        /// Creates a DELETE clause for the specified model.
        /// </summary>
        /// <param name="model">The SQL model.</param>
        /// <returns>A <see cref="DeleteClause"/> object for the model.</returns>
        public static DeleteClause Delete(this ISQLModel model) => new DeleteClause(model);
    }

}