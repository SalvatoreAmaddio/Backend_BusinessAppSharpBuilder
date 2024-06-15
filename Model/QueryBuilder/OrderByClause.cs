namespace Backend.Model
{
    #region OrderBy
    public interface IOrderByClause : IQueryClause
    {
        public GroupByClause GroupBy();
        public OrderByClause Field(string field);
    }
    public class OrderByClause : AbstractClause, IOrderByClause
    {
        public override int Order => 6;
        public OrderByClause() { }
        public OrderByClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            Clauses.AddClause(clause);
            _bits.Add("ORDER BY");
        }
        public GroupByClause GroupBy() => new(this, _model);

        public OrderByClause Field(string field)
        {
            _bits.Add(field);
            return this;
        }

        public override string Statement()
        {
            string? s = null;
            sb.Clear();
            foreach (var clause in Clauses) 
            {
                s = clause?.Statement();
                sb.Append(s);
            }

            bool notFirstIndex = false;
            bool notLastIndex = false;

            for (int i = 0; i <= _bits.Count - 1; i++)
            {
                notFirstIndex = i > 0;
                notLastIndex = i < _bits.Count - 1;
                sb.Append(_bits[i]);
                if (notFirstIndex && notLastIndex) sb.Append(',');
                sb.Append(' ');
            }

            return sb.ToString();
        }
    }
    #endregion
}
