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

        public static SelectBuilder Sum(this ISQLModel model, string field)
        {
            SelectBuilder s = new(model);
            return s.Sum(field);
        }

        public static SelectBuilder CountAll(this ISQLModel model)
        {
            SelectBuilder s = new(model);
            return s.CountAll();
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
            return s.MakeJoin(fk, "INNER JOIN");
        }

        public static SelectBuilder InnerJoin(this ISQLModel model, string tableName1, string tableName2, string key1, string key2)
        {
            SelectBuilder s = new(model);
            return s.MakeJoin("INNER JOIN", tableName1,tableName2,key1,key2);
        }

        /// <summary>
        /// Returns a <see cref="QueryBuilder"/> object with a <c>LEFT JOIN</c> in the <c>FROM</c> clause.
        /// </summary>
        /// <param name="fk">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder LeftJoin(this ISQLModel model, ISQLModel fk)
        {
            SelectBuilder s = new(model);
            return s.MakeJoin(fk, "LEFT JOIN");
        }

        public static SelectBuilder LeftJoin(this ISQLModel model, string tableName, string key)
        {
            SelectBuilder s = new(model);
            return s.MakeJoin(tableName, "LEFT JOIN", key);
        }

        /// <summary>
        /// Returns a <see cref="SelectBuilder"/> object with a <c>RIGHT JOIN</c> in the <c>FROM</c> clause.
        /// </summary>
        /// <param name="fk">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder RightJoin(this ISQLModel model, ISQLModel fk)
        {
            SelectBuilder s = new(model);
            return s.MakeJoin(fk, "RIGHT JOIN");
        }

        public static SelectBuilder Where(this ISQLModel model)
        {
            SelectBuilder s = new(model);
            return s.Where();
        }

    }
}
