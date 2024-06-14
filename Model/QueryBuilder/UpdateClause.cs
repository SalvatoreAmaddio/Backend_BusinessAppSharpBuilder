namespace Backend.Model
{
    #region Update
    public interface IUpdateClause : IQueryClause
    {
        public UpdateClause AllFields();
        public WhereClause Where();
    }
    public class UpdateClause : AbstractClause, IUpdateClause
    {
        public UpdateClause() { }
        public UpdateClause(ISQLModel model) : base(model) => _bits.Add($"UPDATE {model.GetTableName()}");

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

        public WhereClause Where() => new(this, _model);

    }
    #endregion

}
