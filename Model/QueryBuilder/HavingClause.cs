namespace Backend.Model.QueryBuilder
{
    #region Having
    public interface IHavingClause : IQueryClause
    {
        public HavingClause OpenBracket();
        public HavingClause CloseBracket();
        public HavingClause EqualsTo(string field, string value);
        public HavingClause GreaterThan(string field, string value);
        public HavingClause GreaterEqualTo(string field, string value);
        public HavingClause SmallerThan(string field, string value);
        public HavingClause SmallerEqualTo(string field, string value);
        public HavingClause IsNull(string field);
        public HavingClause IsNotNull(string field);
        public HavingClause OR();
        public HavingClause AND();
        public HavingClause NOT();
        public HavingClause Limit(int index = 1);
    }
    public class HavingClause : AbstractConditionalClause, IHavingClause
    {
        public override int Order => 5;
        public HavingClause() { }
        public HavingClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            Clauses.Add(clause);
            _bits.Add("HAVING");
        }
        public new HavingClause EqualsTo(string field, string value) => Condition(field, value, "=");
        public new HavingClause NotEqualsTo(string field, string value) => Condition(field, value, "!=");
        public new HavingClause GreaterThan(string field, string value) => Condition(field, value, ">");
        public new HavingClause GreaterEqualTo(string field, string value) => Condition(field, value, ">=");
        public new HavingClause SmallerThan(string field, string value) => Condition(field, value, "<");
        public new HavingClause SmallerEqualTo(string field, string value) => Condition(field, value, "<=");
        private new HavingClause Condition(string field, string value, string oprt) => (HavingClause)base.Condition(field, value, oprt);
        public new HavingClause IsNull(string field) => (HavingClause)base.IsNull(field);
        public new HavingClause IsNotNull(string field) => (HavingClause)base.IsNotNull(field);
        public new HavingClause Limit(int index = 1) => (HavingClause)base.Limit(index);
        public new HavingClause OR() => (HavingClause)LogicalOperator("OR");
        public new HavingClause AND() => (HavingClause)LogicalOperator("AND");
        public new HavingClause NOT() => (HavingClause)LogicalOperator("NOT");
        public new HavingClause OpenBracket() => (HavingClause)base.OpenBracket();
        public new HavingClause CloseBracket() => (HavingClause)base.CloseBracket();
        public override string ToString() => "HAVING";
    }
    #endregion
}
