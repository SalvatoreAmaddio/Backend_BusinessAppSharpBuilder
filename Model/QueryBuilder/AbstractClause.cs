using Backend.Database;
using System.Reflection;
using System.Text;

namespace Backend.Model
{
    /// <summary>
    /// Represents a query clause with methods for handling parameters and generating query strings.
    /// </summary>
    public interface IQueryClause : IDisposable
    {
        /// <summary>
        /// Gets the list of query parameters.
        /// </summary>
        /// <returns>A list of <see cref="QueryParameter"/> objects.</returns>
        public List<QueryParameter> Params();

        /// <summary>
        /// Removes the last change made to the query clause.
        /// </summary>
        public void RemoveLastChange();

        /// <summary>
        /// Converts the query clause to its string representation.
        /// </summary>
        /// <returns>A string representation of the query clause.</returns>
        public string AsString();

        /// <summary>
        /// Adds a parameter to the query clause.
        /// </summary>
        /// <param name="placeholder">The placeholder name for the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public void AddParameter(string placeholder, object? value);

        /// <summary>
        /// Gets an existing clause of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the clause to get.</typeparam>
        /// <returns>An instance of the specified clause type if found; otherwise, null.</returns>
        public T? GetClause<T>() where T : class, IQueryClause, new();

        /// <summary>
        /// Opens a new clause of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the clause to open.</typeparam>
        /// <returns>A new instance of the specified clause type.</returns>
        public T OpenClause<T>() where T : class, IQueryClause, new();

        /// <summary>
        /// Casts the current instance to the specified clause type if possible.
        /// </summary>
        /// <typeparam name="T">The type to cast the current instance to. Must be a class that implements <see cref="IQueryClause"/> and has a parameterless constructor.</typeparam>
        /// <returns>The current instance cast to the specified type, or null if the cast is not possible.</returns>
        public T? As<T>() where T : class, IQueryClause, new();

    }

    /// <summary>
    /// Represents a collection of query clauses.
    /// </summary>
    public class Clauses : List<AbstractClause>
    {
        /// <summary>
        /// Adds a clause to the collection and reorders the collection.
        /// </summary>
        /// <param name="clause">The clause to add.</param>
        public new void Add(AbstractClause clause)
        {
            base.Add(clause);
            Reorder();
        }

        /// <summary>
        /// Reorders the collection based on the order of the clauses.
        /// </summary>
        private void Reorder()
        {
            IEnumerable<AbstractClause> newOrder = this.OrderBy(s => s.Order).ToList();
            this.Clear();
            this.AddRange(newOrder);
        }

        /// <summary>
        /// Gets the last clause in the collection.
        /// </summary>
        /// <returns>The last <see cref="AbstractClause"/> in the collection.</returns>
        public AbstractClause GetLast() => this[Count - 1];
    }

    /// <summary>
    /// Represents an abstract base class for query clauses.
    /// </summary>
    public abstract class AbstractClause : IQueryClause
    {
        #region Properties
        /// <summary>
        /// Gets the order of the clause within the SQL query.
        /// </summary>
        /// <remarks>
        /// This property is used to set the order of clauses in an SQL query. 
        /// For example, the SELECT clause might have an order of 1, the FROM clause might have an order of 2, and so on. 
        /// This ensures that the clauses are processed and constructed in the correct sequence for a valid SQL query.
        /// </remarks>
        /// <value>An integer representing the order of the clause.</value>
        public abstract int Order { get; }
        protected string TableName { get; } = string.Empty;
        protected string TableKey { get; } = string.Empty;
        #endregion

        protected List<QueryParameter> _parameters = [];
        protected List<string> _bits = [];
        protected Clauses Clauses = [];
        protected StringBuilder sb = new();
        protected ISQLModel _model = null!;
        protected AbstractClause() { }

        protected AbstractClause(ISQLModel model)
        {
            Clauses.Add(this);
            _model = model;
            TableName = model.GetTableName();
            TableKey = model?.GetPrimaryKey()?.Name ?? throw new NullReferenceException($"PK Attribute missing from {_model.GetType().Name}");
        }

        protected void TransferClauses(ref AbstractClause clause) => this.Clauses = clause.Clauses;

        protected void TransferParameters(ref List<QueryParameter> parameters) => this._parameters = parameters;

        public void AddParameter(string placeholder, object? value) => _parameters.Add(new QueryParameter(placeholder, value));

        public List<QueryParameter> Params() => _parameters;

        /// <summary>
        /// Releases all resources used by the <see cref="AbstractClause"/> and clears all associated clauses and parameters.
        /// </summary>
        /// <remarks>
        /// This method iterates through all clauses in the <see cref="Clauses"/> collection, calling the <see cref="Clear"/> method on each one. 
        /// It then clears the <see cref="Clauses"/> collection itself and calls <see cref="Clear"/> on the current instance to reset its state. 
        /// Finally, it suppresses the finalization of the current instance to optimize garbage collection.
        /// </remarks>
        public virtual void Dispose()
        {
            foreach (AbstractClause clause in Clauses)
                clause.Clear();

            Clauses.Clear();
            Clear();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clears all the bits and parameters associated with the current clause.
        /// </summary>
        /// <remarks>
        /// This method resets the state of the current clause by clearing the internal collections that store the query bits and parameters.
        /// It is typically used to remove all existing conditions and parameters, allowing the clause to be reused or disposed of cleanly.
        /// </remarks>
        public void Clear()
        {
            _bits.Clear();
            _parameters.Clear();
        }

        /// <summary>
        /// Constructs the full SQL statement by concatenating the string representations of all the clauses.
        /// </summary>
        /// <remarks>
        /// This method iterates through each clause in the <see cref="Clauses"/> collection, 
        /// calling the <see cref="AbstractClause.AsString"/> method on each one to get its string representation.
        /// These string representations are then concatenated into a single <see cref="StringBuilder"/> object,
        /// which is returned as the full SQL statement.
        /// </remarks>
        /// <returns>A string representing the full SQL statement constructed from all the clauses.</returns>
        public string Statement()
        {
            StringBuilder sb = new();
            foreach (AbstractClause clause in Clauses)
            {
                string s = clause.AsString();
                sb.Append(s);
            }
            return sb.ToString();
        }

        public virtual string AsString()
        {
            sb.Clear();
            foreach (string bit in _bits)
                sb.Append(bit).Append(' ');

            return sb.ToString();
        }

        public void RemoveLastChange() => _bits.RemoveAt(_bits.Count - 1);

        public T? GetClause<T>() where T : class, IQueryClause, new()
        {
            Type t = typeof(T);
            return Clauses.FirstOrDefault(s => s.GetType().IsAssignableFrom(t)) as T;
        }

        public T? As<T>() where T : class, IQueryClause, new() => this as T;

        public T OpenClause<T>() where T : class, IQueryClause, new()
        {
            Type t = typeof(T);
            if (t.IsAssignableFrom(typeof(SelectClause))) throw new NotSupportedException("Cannot be Select");
            if (Clauses.Count == 0) throw new NullReferenceException();
            ConstructorInfo? constructor = t.GetConstructor(new[] { this.GetType(), _model.GetType() });
            if (constructor == null)
            {
                throw new InvalidOperationException($"Type {t.FullName} does not have a constructor that takes a parameter of type {_model.GetType().FullName}");
            }
            return (T)constructor.Invoke(new object[] { this, _model });
        }
    }

    /// <summary>
    /// Represents an abstract base class for conditional query clauses.
    /// </summary>
    public abstract class AbstractConditionalClause : AbstractClause
    {
        protected AbstractConditionalClause() { }

        protected AbstractConditionalClause(ISQLModel model) : base(model) { }

        /// <summary>
        /// Determines whether the clause has conditions.
        /// </summary>
        /// <returns>True if the clause has conditions; otherwise, false.</returns>
        public bool HasConditions() => _bits.Count > 1;

        /// <summary>
        /// Adds an equality condition to the clause.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added condition.</returns>
        /// <remarks>
        /// This method adds a condition to the clause that checks if the specified field is equal to the specified value.
        /// </remarks>
        public AbstractConditionalClause EqualsTo(string field, string value) => Condition(field, value, "=");

        /// <summary>
        /// Adds an NOT equality condition to the clause.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added condition.</returns>
        /// <remarks>
        /// This method adds a condition to the clause that checks if the specified field is equal to the specified value.
        /// </remarks>
        public AbstractConditionalClause NotEqualsTo(string field, string value) => Condition(field, value, "!=");

        /// <summary>
        /// Adds a greater-than condition to the clause.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added condition.</returns>
        /// <remarks>
        /// This method adds a condition to the clause that checks if the specified field is greater than the specified value.
        /// </remarks>
        public AbstractConditionalClause GreaterThan(string field, string value) => Condition(field, value, ">");

        /// <summary>
        /// Adds a greater-than-or-equal-to condition to the clause.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added condition.</returns>
        /// <remarks>
        /// This method adds a condition to the clause that checks if the specified field is greater than or equal to the specified value.
        /// </remarks>
        public AbstractConditionalClause GreaterEqualTo(string field, string value) => Condition(field, value, ">=");

        /// <summary>
        /// Adds a less-than condition to the clause.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added condition.</returns>
        /// <remarks>
        /// This method adds a condition to the clause that checks if the specified field is less than the specified value.
        /// </remarks>
        public AbstractConditionalClause SmallerThan(string field, string value) => Condition(field, value, "<");

        /// <summary>
        /// Adds a less-than-or-equal-to condition to the clause.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added condition.</returns>
        /// <remarks>
        /// This method adds a condition to the clause that checks if the specified field is less than or equal to the specified value.
        /// </remarks>
        public AbstractConditionalClause SmallerEqualTo(string field, string value) => Condition(field, value, "<=");

        /// <summary>
        /// Adds an IS NULL condition to the clause.
        /// </summary>
        /// <param name="field">The name of the field to check for null.</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added condition.</returns>
        /// <remarks>
        /// This method adds a condition to the clause that checks if the specified field is null.
        /// </remarks>
        public AbstractConditionalClause IsNull(string field)
        {
            _bits.Add($"{field} IS NULL");
            return this;
        }

        /// <summary>
        /// Adds an IS NOT NULL condition to the clause.
        /// </summary>
        /// <param name="field">The name of the field to check for non-null.</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added condition.</returns>
        /// <remarks>
        /// This method adds a condition to the clause that checks if the specified field is not null.
        /// </remarks>
        public AbstractConditionalClause IsNotNull(string field)
        {
            _bits.Add($"{field} IS NOT NULL");
            return this;
        }

        /// <summary>
        /// Adds a custom condition to the clause.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value to compare the field against.</param>
        /// <param name="oprt">The operator to use in the comparison (e.g., "=", "!=", ">", "<").</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added condition.</returns>
        /// <remarks>
        /// This method adds a condition to the clause with the specified field, value, and operator.
        /// </remarks>
        public AbstractConditionalClause Condition(string field, string value, string oprt)
        {
            _bits.Add($"{field} {oprt} {value}");
            return this;
        }

        /// <summary>
        /// Adds a logical operator to the clause.
        /// </summary>
        /// <param name="oprt">The logical operator to add (e.g., "AND", "OR", "NOT").</param>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added logical operator.</returns>
        /// <remarks>
        /// This method adds a logical operator to the clause, which can be used to combine multiple conditions.
        /// </remarks>
        public AbstractConditionalClause LogicalOperator(string oprt)
        {
            _bits.Add(oprt);
            return this;
        }

        /// <summary>
        /// Adds an OR logical operator to the clause.
        /// </summary>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added OR operator.</returns>
        /// <remarks>
        /// This method adds an OR logical operator to the clause, allowing for the combination of multiple conditions.
        /// </remarks>
        public AbstractConditionalClause OR() => LogicalOperator("OR");

        /// <summary>
        /// Adds an AND logical operator to the clause.
        /// </summary>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added AND operator.</returns>
        /// <remarks>
        /// This method adds an AND logical operator to the clause, allowing for the combination of multiple conditions.
        /// </remarks>
        public AbstractConditionalClause AND() => LogicalOperator("AND");

        /// <summary>
        /// Adds a NOT logical operator to the clause.
        /// </summary>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added NOT operator.</returns>
        /// <remarks>
        /// This method adds a NOT logical operator to the clause, allowing for the negation of a condition.
        /// </remarks>
        public AbstractConditionalClause NOT() => LogicalOperator("NOT");

        /// <summary>
        /// Adds an opening bracket to the clause.
        /// </summary>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added opening bracket.</returns>
        /// <remarks>
        /// This method adds an opening bracket to the clause, allowing for grouping of conditions.
        /// </remarks>
        public AbstractConditionalClause OpenBracket()
        {
            _bits.Add("(");
            return this;
        }

        /// <summary>
        /// Adds a closing bracket to the clause.
        /// </summary>
        /// <returns>The current instance of <see cref="AbstractConditionalClause"/> with the added closing bracket.</returns>
        /// <remarks>
        /// This method adds a closing bracket to the clause, allowing for grouping of conditions.
        /// </remarks>
        public AbstractConditionalClause CloseBracket()
        {
            _bits.Add(")");
            return this;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the query.
        /// </summary>
        /// <returns>A new instance of <see cref="OrderByClause"/> associated with the current model.</returns>
        /// <remarks>
        /// This method creates and returns a new <see cref="OrderByClause"/> object, which can be used to specify the order of the query results.
        /// </remarks>
        public OrderByClause OrderBy() => new OrderByClause(this, _model);

        /// <summary>
        /// Adds a LIMIT clause to the query.
        /// </summary>
        /// <param name="limit">The maximum number of records to return. Default is 1.</param>
        /// <returns>A new instance of <see cref="LimitClause"/> associated with the current model and limit.</returns>
        /// <remarks>
        /// This method creates and returns a new <see cref="LimitClause"/> object, which can be used to limit the number of records returned by the query.
        /// </remarks>
        public LimitClause Limit(int limit = 1) => new LimitClause(this, _model, limit);
    }
}