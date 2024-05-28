using Backend.Database;
using Backend.Model;

namespace Backend.Source
{
    /// <summary>
    /// <c>Observer Pattern:</c> This interface defines the methods that a <see cref="RecordSource"/> must 
    /// implement to act as Parent Source to notify Children sources.
    /// see also <seealso cref="IChildSource"/>
    /// </summary>
    public interface IParentSource
    {
        /// <summary>
        /// Add a child source to the Parent Source
        /// </summary>
        /// <param name="child">A object implementing IChildSource</param>
        public void AddChild(IChildSource child);

        /// <summary>
        /// Remove a child source from the Parent Source
        /// </summary>
        /// <param name="child">A object implementing IChildSource</param>
        public void RemoveChild(IChildSource child);

        /// <summary>
        /// Notifies the children of changes they must observe. 
        /// </summary>
        /// <param name="crud">A <see cref="CRUD"/> Enum</param>
        /// <param name="model">The Record that has changed.</param>
        public void NotifyChildren(CRUD crud, ISQLModel model);
    }

    /// <summary>
    /// <c>Observer Pattern:</c> This interface defines the methods that a <see cref="RecordSource"/> must 
    /// implement to act as Child Source to receive messages from their Parent Source.
    /// see also <seealso cref="IParentSource"/>
    /// </summary>
    public interface IChildSource 
    {
        /// <summary>
        /// Gets and sets a reference to the ParentSource object.
        /// </summary>
        public IParentSource? ParentSource { get; set; }

        /// <summary>
        /// Defines the logic for implementing record updates comunicated by the Parent Source. 
        /// </summary>
        /// <param name="crud">A <see cref="CRUD"/> Enum</param>
        /// <param name="model">The Record that has changed.</param>
        public void Update(CRUD crud, ISQLModel model);
    }
}
