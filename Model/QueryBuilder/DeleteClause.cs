namespace Backend.Model
{
    #region Delete
    public interface IDeleteClause
    {
        public FromClause From();
        public WhereClause Where();
    }
    public class DeleteClause : AbstractClause, IDeleteClause
    {
        public DeleteClause() { }
        public DeleteClause(ISQLModel model) : base(model) => _bits.Add($"DELETE");
        public FromClause From() => new(this, _model);
        public WhereClause Where() => new(this, _model);
    }
    #endregion
}
