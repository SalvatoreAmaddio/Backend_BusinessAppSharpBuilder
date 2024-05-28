namespace Backend.Model
{
    /// <summary>
    /// This attribute is used to associate a class extending <see cref="AbstractSQLModel"/> to a Table's Name in your Database.
    /// This is pivotal for <see cref="QueryBuilder"/> to produce default queries.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Table(string name) : Attribute 
    {
        private readonly string _name = name;

        public override string ToString() => _name;

    }


    /// <summary>
    /// This is the base class extended by <see cref="Field"/>, <see cref="PK"/> and <see cref="FK"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class AbstractField : Attribute
    {
        public override string ToString() => GetType().Name;
    }

    /// <summary>
    /// This attribute is used to associate a <see cref="AbstractSQLModel"/>'s property to a Field in your table to a Table in your Database.
    /// This is pivotal for <see cref="QueryBuilder"/> to produce default queries.
    /// </summary>
    public class Field : AbstractField
    {
    }

    /// <summary>
    /// This attribute is used to associate one <see cref="AbstractSQLModel"/>'s property to the Primary Key in your table to a Table in your Database.
    /// This is pivotal for <see cref="QueryBuilder"/> to produce default queries.
    /// <para/>
    /// N.B: Only one property can have this attribute.
    /// </summary>
    public class PK : AbstractField
    {
    }

    /// <summary>
    /// This attribute is used to associate a <see cref="AbstractSQLModel"/>'s property to a Foreign Key in your table to a Table in your Database.
    /// This is pivotal for <see cref="QueryBuilder"/> to produce default queries.
    /// </summary>
    public class FK : AbstractField
    {
    }
}
