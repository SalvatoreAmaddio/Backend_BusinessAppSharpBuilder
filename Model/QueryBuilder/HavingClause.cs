namespace Backend.Model.QueryBuilder
{
    public class HavingClause : AbstractConditionalClause
    {
        public override int Order => 5;
        public HavingClause() { }
        public HavingClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            TransferClauses(ref clause.Clauses);
            TransferParameters(ref _parameters);
            Clauses.Add(this);
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
        public new HavingClause OR() => (HavingClause)LogicalOperator("OR");
        public new HavingClause AND() => (HavingClause)LogicalOperator("AND");
        public new HavingClause NOT() => (HavingClause)LogicalOperator("NOT");
        public new HavingClause OpenBracket() => (HavingClause)base.OpenBracket();
        public new HavingClause CloseBracket() => (HavingClause)base.CloseBracket();
        public override string ToString() => "HAVING CLAUSE";

    }

}
