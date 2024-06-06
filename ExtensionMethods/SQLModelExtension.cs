using Backend.Model;

namespace Backend.ExtensionMethods
{
    public static class SQLModelExtension
    {
        public static SelectBuilder SelectAll(this ISQLModel model) => new SelectBuilder(model).SelectAll();
        public static SelectBuilder SelectFields(this ISQLModel model, params string[] fields) => new SelectBuilder(model).SelectFields(fields);

        public static SelectBuilder Sum(this ISQLModel model, string field) => new SelectBuilder(model).Sum(field);
        
        public static SelectBuilder CountAll(this ISQLModel model) => new SelectBuilder(model).CountAll();        

        #region INNER JOIN

        /// <summary>
        /// Returns a <see cref="SelectBuilder"/> object with a <c>INNER JOIN</c> in the <c>FROM</c> clause.
        /// The <c>INNER JOIN</c> is built by using the Primary Key of the <paramref name="fk"/> argument.
        /// For Example:
        /// <code>
        /// new Patient().InnerJoin(new Gender()) // FROM Patient INNER JOIN Gender ON Patient.GenderID = Gender.GenderID
        /// </code>
        /// </summary>
        /// <param name="fk">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder InnerJoin(this ISQLModel model, ISQLModel fk) => new SelectBuilder(model).MakeJoin(fk,"INNER JOIN");
            

        /// <summary>
        /// Returns a <see cref="SelectBuilder"/> object with a <c>INNER JOIN</c> in the <c>FROM</c> clause.
        /// The <c>INNER JOIN</c> is built as follow:
        /// <code>
        /// new Patient().InnerJoin("Patient","Gender","GenderID") // FROM Patient INNER JOIN Gender ON Patient.GenderID = Gender.GenderID
        /// </code>
        /// </summary>
        /// <param name="fk">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder InnerJoin(this ISQLModel model, string tableName1, string tableName2, string commonKey) => new SelectBuilder(model).MakeJoin("INNER JOIN", tableName1,tableName2,commonKey);

        #endregion

        #region LEFT JOIN

        /// <summary>
        /// Returns a <see cref="SelectBuilder"/> object with a <c>LEFT JOIN</c> in the <c>FROM</c> clause.
        /// The <c>LEFT JOIN</c> is built by using the Primary Key of the <paramref name="fk"/> argument.
        /// For Example:
        /// <code>
        /// new Patient().LeftJoin(new Gender()) // FROM Patient LEFT JOIN Gender ON Patient.GenderID = Gender.GenderID
        /// </code>
        /// </summary>
        /// <param name="fk">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder LeftJoin(this ISQLModel model, ISQLModel fk) => new SelectBuilder(model).MakeJoin(fk, "LEFT JOIN");

        /// <summary>
        /// Returns a <see cref="SelectBuilder"/> object with a <c>LEFT JOIN</c> in the <c>FROM</c> clause.
        /// The <c>LEFT JOIN</c> is as follow.
        /// For Example:
        /// <code>
        /// new Patient().LeftJoin("Gender","GenderID") // FROM Patient LEFT JOIN Gender ON Patient.GenderID = Gender.GenderID
        /// </code>
        /// </summary>
        /// <param name="fk">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder LeftJoin(this ISQLModel model, string tableName, string tableNameKey) => new SelectBuilder(model).MakeJoin(tableName, "LEFT JOIN", tableNameKey);
        #endregion

        #region RIGHT JOIN
        /// <summary>
        /// Returns a <see cref="SelectBuilder"/> object with a <c>RIGHT JOIN</c> in the <c>FROM</c> clause.
        /// The <c>RIGHT JOIN</c> is built by using the Primary Key of the <paramref name="table"/> argument.
        /// For Example:
        /// <code>
        /// new Patient().RightJoin(new Gender()) // FROM Patient RIGHT JOIN Gender ON Patient.GenderID = Gender.GenderID
        /// </code>
        /// </summary>
        /// <param name="table">a <see cref="ISQLModel"/> which represents the Table to join.</param>
        /// <returns>A <see cref="SelectBuilder"/> object.</returns>
        public static SelectBuilder RightJoin(this ISQLModel model, ISQLModel table) => new SelectBuilder(model).MakeJoin(table, "RIGHT JOIN");
        #endregion

        #region WHERE
        public static SelectBuilder Where(this ISQLModel model) => new SelectBuilder(model).Where();
        #endregion

    }
}
