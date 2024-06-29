namespace Backend.Database
{
    /// <summary>
    /// This class defines the properties that encapsulate information for parameters to be used in queries by the <see cref="AbstractDatabase{M}"/> class.
    /// </summary>
    public class QueryParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParameter"/> class.
        /// </summary>
        /// <param name="placeholder">The placeholder for the parameter, representing the field's name.</param>
        /// <param name="value">The value of the parameter.</param>
        public QueryParameter(string placeholder, object? value)
        {
            Placeholder = placeholder;
            Value = value;
        }

        /// <summary>
        /// Gets the placeholder, i.e., the field's name.
        /// </summary>
        /// <value>A string representing the field's name.</value>
        public string Placeholder { get; }

        /// <summary>
        /// Gets the value of the field.
        /// </summary>
        /// <value>An object representing the field's value.</value>
        public object? Value { get; }

        /// <summary>
        /// Returns a string representation of the query parameter.
        /// </summary>
        /// <returns>A string in the format "Placeholder:Value".</returns>
        public override string ToString() => $"{Placeholder}:{Value}";

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj) =>
            obj is QueryParameter parameter &&
            Placeholder == parameter.Placeholder &&
            EqualityComparer<object?>.Default.Equals(Value, parameter.Value);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => HashCode.Combine(Placeholder, Value);
    }
}
