namespace Backend.Model
{
    /// <summary>
    /// This attribute marks a property of an <see cref="AbstractSQLModel"/> as mandatory.
    /// A mandatory property must have a value before an update operation can be performed.
    /// By default, the mandatory check is enforced in the <see cref="ISQLModel.AllowUpdate"/> method.
    /// </summary>
    /// <remarks>
    /// The <see cref="Mandatory"/> attribute should be applied to properties that are required to have a value
    /// before updating the corresponding record in the database. If a mandatory property is not set,
    /// the <see cref="ISQLModel.AllowUpdate"/> method should prevent the update operation from proceeding.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class Mandatory : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mandatory"/> class.
        /// </summary>
        public Mandatory() { }
    }

}
