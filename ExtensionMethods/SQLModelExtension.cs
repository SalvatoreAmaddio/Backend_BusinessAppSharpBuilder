using Backend.Model;

namespace Backend.ExtensionMethods
{
    public static class SQLModelExtension
    {
        public static SelectClause Distinct(this ISQLModel model) => new SelectClause(model).Distinct();
        public static SelectClause SelectAll(this ISQLModel model) => new(model);
        public static SelectClause Select(this ISQLModel model, params string[] fields) => new SelectClause(model).Fields(fields);
        public static SelectClause Sum(this ISQLModel model, string field) => new SelectClause(model).Sum(field);
        public static SelectClause CountAll(this ISQLModel model) => new SelectClause(model).CountAll();
        public static FromClause From(this ISQLModel model) => new(model);
        public static WhereClause Where(this ISQLModel model) => new(model);
        public static InsertClause Insert(this ISQLModel model) => new(model);

        public static UpdateClause Update(this ISQLModel model) => new(model);

        public static DeleteClause Delete(this ISQLModel model) => new(model);
    }
}