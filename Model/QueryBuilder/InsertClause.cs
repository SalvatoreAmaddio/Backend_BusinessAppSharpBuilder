namespace Backend.Model
{
    /// <summary>
    /// Represents an INSERT clause in an SQL query.
    /// </summary>
    public class InsertClause : AbstractClause
    {
        public override int Order => 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertClause"/> class.
        /// </summary>
        public InsertClause() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertClause"/> class with the specified model.
        /// </summary>
        /// <param name="model">The SQL model associated with the clause.</param>
        public InsertClause(ISQLModel model) : base(model) => _bits.Add($"INSERT INTO {model.GetTableName()}");

        /// <summary>
        /// Specifies all fields for the INSERT statement, excluding the primary key.
        /// </summary>
        /// <returns>The current instance of <see cref="InsertClause"/> with the specified fields.</returns>
        public InsertClause All()
        {
            _bits.Add("(");
            string pkName = _model.GetPrimaryKey()?.Name ?? "";

            foreach (string fieldName in _model.GetEntityFieldNames())
            {
                if (pkName.Equals(fieldName)) continue;
                _bits.Add(fieldName);
                _bits.Add(",");
            }
            RemoveLastChange();
            _bits.Add(")");
            return this;
        }

        /// <summary>
        /// Specifies the given fields for the INSERT statement.
        /// </summary>
        /// <param name="fields">The fields to include in the INSERT statement.</param>
        /// <returns>The current instance of <see cref="InsertClause"/> with the specified fields.</returns>
        public InsertClause Fields(params string[] fields)
        {
            _bits.Add("(");
            foreach (string fieldName in fields)
            {
                _bits.Add(fieldName);
                _bits.Add(",");
            }
            RemoveLastChange();
            _bits.Add(")");
            return this;
        }

        /// <summary>
        /// Specifies the values for the INSERT statement.
        /// </summary>
        /// <param name="includeFields">If true, includes the field names in the VALUES clause.</param>
        /// <returns>The current instance of <see cref="InsertClause"/> with the specified values.</returns>
        public InsertClause Values(bool includeFields = true)
        {
            IEnumerable<string>? fields = null;

            if (includeFields)
            {
                int index = _bits.IndexOf("(") + 1;
                fields = _bits.Skip(index).ToList();
                fields = fields.Take(fields.Count() - 1);
            }

            if (fields == null)
            {
                _bits.Add("VALUES");
                return this;
            }
            else _bits.Add("VALUES (");

            foreach (string fieldName in fields)
            {
                if (fieldName.Equals(","))
                {
                    _bits.Add(fieldName);
                }
                else _bits.Add($"@{fieldName}");
            }
            _bits.Add(")");
            return this;
        }

        /// <summary>
        /// Specifies the row values for the INSERT statement.
        /// </summary>
        /// <param name="fields">The values to include in the row.</param>
        /// <returns>The current instance of <see cref="InsertClause"/> with the specified row values.</returns>
        public InsertClause RowValues(params string[] fields)
        {
            foreach (string fieldName in fields)
            {
                _bits.Add($"@{fieldName}");
                _bits.Add(",");
            }
            _bits.Add("),");
            return this;
        }

        /// <summary>
        /// Converts the INSERT clause to its string representation.
        /// </summary>
        /// <returns>A string representing the INSERT clause.</returns>
        public override string AsString()
        {
            int index = _bits.LastIndexOf("),");
            if (index >= 0)
                _bits[index] = ")";
            return base.AsString();
        }

        /// <summary>
        /// Adds a SELECT clause to the INSERT query.
        /// </summary>
        /// <returns>A new instance of <see cref="SelectClause"/> associated with the current INSERT query.</returns>
        public SelectClause Select() => new SelectClause(this, _model);
    }

}
