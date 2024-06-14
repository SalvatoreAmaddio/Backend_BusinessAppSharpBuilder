namespace Backend.Model
{
    #region OrderBy
    public interface IOrderByClause : IQueryClause
    {
        public OrderByClause Field(string field);
    }
    public class OrderByClause : AbstractClause, IOrderByClause
    {
        public OrderByClause() { }
        public OrderByClause(IQueryClause clause, ISQLModel model) : base(model)
        {
            PreviousClause = clause;
            _bits.Add("ORDER BY");
        }

        public OrderByClause Field(string field)
        {
            _bits.Add(field);
            return this;
        }

        public override string Statement()
        {
            string? s = PreviousClause?.Statement();
            sb.Clear();
            sb.Append(s);
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
