namespace Backend.Model
{
    #region Where
    public class WhereClause : AbstractConditionalClause, IWhereClause
    {
        public override int Order => 3;
        public WhereClause() { }
        public WhereClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            Clauses.AddClause(clause);
            _bits.Add("WHERE");
        }
        public WhereClause(ISQLModel model) : base(model)
        {
            Clauses.AddClause(new SelectClause(model).AllFields().From());
            _bits.Add("WHERE");
        }
        public WhereClause This()
        {
            return this.EqualsTo(TableKey, $"@{TableKey}");
        }
        public WhereClause Between(string field, string value1, string value2)
        {
            _bits.Add($"{field} BETWEEN {value1} AND {value2}");
            return this;
        }
        public WhereClause In(string field, params string[] values)
        {
            _bits.Add($"{field}");
            _bits.Add($"IN");
            _bits.Add($"(");

            foreach (string value in values)
            {
                _bits.Add(value);
                _bits.Add(", ");
            }
            RemoveLastChange(); // remove last coma
            _bits.Add($")");
            return this;
        }
        public WhereClause Like(string field, string value) => Condition(field, value, "LIKE");
        public new WhereClause EqualsTo(string field, string value) => Condition(field, value, "=");
        public new WhereClause NotEqualsTo(string field, string value) => Condition(field, value, "!=");
        public new WhereClause GreaterThan(string field, string value) => Condition(field, value, ">");
        public new WhereClause GreaterEqualTo(string field, string value) => Condition(field, value, ">=");
        public new WhereClause SmallerThan(string field, string value) => Condition(field, value, "<");
        public new WhereClause SmallerEqualTo(string field, string value) => Condition(field, value, "<=");
        private new WhereClause Condition(string field, string value, string oprt) => (WhereClause)base.Condition(field, value, oprt);
        public new WhereClause IsNull(string field) => (WhereClause)base.IsNull(field);
        public new WhereClause IsNotNull(string field) => (WhereClause)base.IsNotNull(field);
        public new WhereClause Limit(int index = 1) => (WhereClause)base.Limit(index);
        public new WhereClause OR() => (WhereClause)LogicalOperator("OR");
        public new WhereClause AND() => (WhereClause)LogicalOperator("AND");
        public new WhereClause NOT() => (WhereClause)LogicalOperator("NOT");
        public new WhereClause OpenBracket() => (WhereClause)base.OpenBracket();
        public new WhereClause CloseBracket() => (WhereClause)base.CloseBracket();
        public GroupByClause GroupBy() => new(this, _model);
        public override string ToString() => "WHERE";
    }
    public interface IWhereClause : IQueryClause
    {
        public GroupByClause GroupBy();
        public WhereClause This();
        public OrderByClause OrderBy();
        public WhereClause In(string field, params string[] values);
        public WhereClause Between(string field, string value1, string value2);
        public WhereClause OpenBracket();
        public WhereClause CloseBracket();
        public WhereClause EqualsTo(string field, string value);
        public WhereClause Like(string field, string value);
        public WhereClause GreaterThan(string field, string value);
        public WhereClause GreaterEqualTo(string field, string value);
        public WhereClause SmallerThan(string field, string value);
        public WhereClause SmallerEqualTo(string field, string value);
        public WhereClause IsNull(string field);
        public WhereClause IsNotNull(string field);
        public WhereClause OR();
        public WhereClause AND();
        public WhereClause NOT();
        public WhereClause Limit(int index = 1);
    }

    #endregion

}
