namespace Backend.Model
{
    #region From
    public class FromClause : AbstractClause, IFromClause
    {
        public override int Order => 2;
        public FromClause() { }
        public FromClause(AbstractClause clause, ISQLModel model) : base(model)
        {
            Clauses.AddClause(clause);
            _bits.Add("FROM");
            _bits.Add(TableName);
        }
        public FromClause(ISQLModel model) : base(model)
        {
            Clauses.AddClause(new SelectClause(model).AllFields());
            _bits.Add("FROM");
            _bits.Add(TableName);
        }

        /// <summary>
        /// Creates a <c>JOIN</c> between the <paramref name="toTable"/>'s PrimaryKey and this table ForeignKey.
        /// </summary>
        /// <param name="join">The type of Join</param>
        /// <param name="toTable">The table to join to</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private FromClause MakeJoin(string join, ISQLModel toTable)
        {
            string commonKey = toTable?.GetPrimaryKey()?.Name ?? throw new Exception("Null Reference");
            return MakeJoin(join, toTable.GetTableName(), commonKey);
        }

        /// <summary>
        /// Creates a <c>JOIN</c> between the <paramref name="toTable"/> and this table based on a <paramref name="commonKey"/> in both tables.
        /// </summary>
        /// <param name="join">The type of Join</param>
        /// <param name="toTable">The table to join to</param>
        /// <param name="commonKey">The name of a common key</param>
        /// <returns></returns>
        private FromClause MakeJoin(string join, string toTable, string commonKey)
        {
            _bits.Add(join);
            _bits.Add(toTable);
            _bits.Add("ON");
            _bits.Add($"{this.TableName}.{commonKey}");
            _bits.Add("=");
            _bits.Add($"{toTable}.{commonKey}");
            return this;
        }
        /// <summary>
        /// Creates a <c>JOIN</c> between two given tables based on a <paramref name="commonKey"/> in both tables.
        /// </summary>
        /// <param name="join">The type of Join</param>
        /// <param name="fromTable">The table to join from</param>
        /// <param name="toTable">The table to join to</param>
        /// <param name="commonKey">The name of a common key</param>
        /// <returns></returns>
        private FromClause MakeJoin(string join, string fromTable, string toTable, string commonKey)
        {
            _bits.Add(join);
            _bits.Add(toTable);
            _bits.Add("ON");
            _bits.Add($"{fromTable}.{commonKey}");
            _bits.Add("=");
            _bits.Add($"{toTable}.{commonKey}");
            return this;
        }

        #region INNER JOIN
        public FromClause InnerJoin(ISQLModel toTable) => MakeJoin("INNER JOIN", toTable);
        public FromClause InnerJoin(string toTable, string commonKey) => MakeJoin("INNER JOIN", toTable, commonKey);
        public FromClause InnerJoin(string fromTable, string toTable, string commonKey) => MakeJoin("INNER JOIN", fromTable, toTable, commonKey);
        #endregion

        #region RIGHT JOIN
        public FromClause RightJoin(ISQLModel toTable) => MakeJoin("RIGHT JOIN", toTable);
        public FromClause RightJoin(string toTable, string commonKey) => MakeJoin("RIGHT JOIN", toTable, commonKey);
        public FromClause RightJoin(string fromTable, string toTable, string commonKey) => MakeJoin("RIGHT JOIN", fromTable, toTable, commonKey);
        #endregion

        #region LEFT JOIN
        public FromClause LeftJoin(ISQLModel toTable) => MakeJoin("LEFT JOIN", toTable);
        public FromClause LeftJoin(string toTable, string commonKey) => MakeJoin("LEFT JOIN", toTable, commonKey);
        public FromClause LeftJoin(string fromTable, string toTable, string commonKey) => MakeJoin("LEFT JOIN", fromTable, toTable, commonKey);
        #endregion

        public WhereClause Where() => new(this, _model);
        public FromClause CloseBracket()
        {
            _bits.Add(")");
            return this;
        }
        public FromClause OpenBracket()
        {
            _bits.Insert(1, "(");
            return this;
        }
        public FromClause Limit(int index = 1)
        {
            _bits.Add($"LIMIT {index}");
            return this;
        }
        public OrderByClause OrderBy() => new(this, _model);
        public GroupByClause GroupBy() => new(this, _model);
        public override string ToString() => "FROM";
    }
    public interface IFromClause : IQueryClause
    {
        public WhereClause Where();
        public OrderByClause OrderBy();
        public FromClause OpenBracket();
        public FromClause CloseBracket();
        public FromClause InnerJoin(ISQLModel toTable);
        public FromClause InnerJoin(string toTable, string commonKey);
        public FromClause InnerJoin(string fromTable, string toTable, string commonKey);
        public FromClause RightJoin(ISQLModel toTable);
        public FromClause RightJoin(string toTable, string commonKey);
        public FromClause RightJoin(string fromTable, string toTable, string commonKey);
        public FromClause LeftJoin(ISQLModel toTable);
        public FromClause LeftJoin(string toTable, string commonKey);
        public FromClause LeftJoin(string fromTable, string toTable, string commonKey);
        public FromClause Limit(int index = 1);
        public GroupByClause GroupBy();
    }

    #endregion

}
