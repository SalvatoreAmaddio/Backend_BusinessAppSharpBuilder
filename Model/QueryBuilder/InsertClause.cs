﻿namespace Backend.Model
{
    #region Insert
    public class InsertClause : AbstractClause
    {
        public override int Order => 0;
        public InsertClause() { }
        public InsertClause(ISQLModel model) : base(model) => _bits.Add($"INSERT INTO {model.GetTableName()}");

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

        public override string AsString()
        {
            int index = _bits.LastIndexOf("),");
            if (index >= 0)
                _bits[index] = ")";
            return base.AsString();
        }

        public SelectClause Select() => new(this, _model);
    }
    #endregion

}