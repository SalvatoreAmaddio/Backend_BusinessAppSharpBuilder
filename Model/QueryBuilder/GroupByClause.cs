namespace Backend.Model
{
    #region GroupBy
    public interface IGroupBy : IQueryClause
    {
        public GroupByClause Fields(params string[] fields);
        public GroupByClause Limit(int index = 1);
    }
    public class GroupByClause : AbstractClause, IGroupBy
    {
        public GroupByClause() { }
        public GroupByClause(IQueryClause clause, ISQLModel model) : base(model)
        {
            PreviousClause = clause;
            _bits.Add("GROUP BY");
        }

        public GroupByClause Fields(params string[] fields)
        {
            foreach (string field in fields)
            {
                _bits.Add(field);
                _bits.Add(",");
            }
            RemoveLastChange();
            return this;
        }

        public GroupByClause Limit(int index = 1)
        {
            _bits.Add($"LIMIT {index}");
            return this;
        }

    }
    #endregion

}
