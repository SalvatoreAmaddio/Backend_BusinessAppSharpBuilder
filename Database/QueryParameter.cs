namespace Backend.Database
{
    /// <summary>
    /// This class defines the properties that incapsulate information for parameters to be used in queries by the <see cref="AbstractDatabase"/> class.
    /// </summary>
    public class QueryParameter(string placeholder, object? value)
    {
        /// <summary>
        /// This property gets the Placeholder i.e. the field's name.
        /// </summary>
        /// <value>A string representing the field's name</value>
        public string Placeholder { get; } = placeholder;

        /// <summary>
        /// This property gets the Value of the field's name.
        /// </summary>
        /// <value>An object representing the field's value</value>
        public object? Value { get; } = value;
        public override string ToString() => $"{Placeholder}:{Value}";

        public override bool Equals(object? obj) =>
        obj is QueryParameter @object &&
        Placeholder == @object.Placeholder &&
        Value == @object.Value;
        public override int GetHashCode() => HashCode.Combine(Placeholder, Value);
    }

}
