using Backend.Model;

namespace Backend.ExtensionMethods
{
    public static class SQLModelExtension
    {
        public static SelectBuilder SelectAll(this ISQLModel model)
        {
            SelectBuilder s = new(model);
            return s.SelectAll();
        }

        public static SelectBuilder SelectFields(this ISQLModel model, params string[] fields)
        {
            SelectBuilder s = new(model);
            return s.SelectFields(fields);
        }

        /// <summary>
        /// Returns a <see cref="SelectBuilder"/> object with a <c>INNER JOIN</c> in the <c>FROM</c> clause.
        /// </summary>
        /// <param name="fk">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder InnerJoin(this ISQLModel model, ISQLModel fk)
        {
            SelectBuilder s = new(model);
            return s.InnerJoin(fk);
        }

        /// <summary>
        /// Returns a <see cref="QueryBuilder"/> object with a <c>LEFT JOIN</c> in the <c>FROM</c> clause.
        /// </summary>
        /// <param name="fk">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder LeftJoin(this ISQLModel model, ISQLModel fk)
        {
            SelectBuilder s = new(model);
            return s.LeftJoin(fk);
        }


        /// <summary>
        /// Returns a <see cref="SelectBuilder"/> object with a <c>RIGHT JOIN</c> in the <c>FROM</c> clause.
        /// </summary>
        /// <param name="fk">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder RightJoin(this ISQLModel model, ISQLModel fk)
        {
            SelectBuilder s = new(model);
            return s.RightJoin(fk);
        }

        public static SelectBuilder Where(this ISQLModel model)
        {
            SelectBuilder s = new(model);
            return s.Where();
        }

    }
}
