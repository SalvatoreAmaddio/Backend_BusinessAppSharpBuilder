
namespace Backend.Model
{
    /// <summary>
    /// This attribute marks a <see cref="AbstractSQLModel"/>'s property as mandatory. By default, the mandatory check is performed in the <see cref="ISQLModel.AllowUpdate"/> method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Mandatory : Attribute
    {        
        public Mandatory() { }
       
    }
}
