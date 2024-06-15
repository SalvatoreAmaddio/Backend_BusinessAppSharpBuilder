using Backend.Model.QueryBuilder;

namespace Backend.Model
{
    #region GroupBy
    public interface IGroupBy : IQueryClause
    {
        public OrderByClause OrderBy();
        public GroupByClause Fields(params string[] fields);
        public GroupByClause Limit(int index = 1);
        public HavingClause Having();
    }

    public class GroupByClause : AbstractClause, IGroupBy
    {
        public override int Order => 4;
        public GroupByClause() { }
        public GroupByClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            Clauses.Add(clause);
            _bits.Add("GROUP BY");
        }

        public HavingClause Having() => new(this,_model);
        public OrderByClause OrderBy() => new(this,_model);

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

        public override string ToString() => "GROUP BY";

    }
    #endregion

}
