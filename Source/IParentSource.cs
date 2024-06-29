using Backend.Enums;
using Backend.Model;

namespace Backend.Source
{
    /// <summary>
    /// <c>Observer Pattern:</c> This interface defines the methods that a <see cref="DataSource"/> must 
    /// implement to act as a parent source to notify child sources.
    /// See also <seealso cref="IChildSource"/>.
    /// </summary>
    public interface IParentSource
    {
        /// <summary>
        /// Adds a child source to the parent source.
        /// </summary>
        /// <param name="child">An object implementing <see cref="IChildSource"/>.</param>
        void AddChild(IChildSource child);

        /// <summary>
        /// Removes a child source from the parent source.
        /// </summary>
        /// <param name="child">An object implementing <see cref="IChildSource"/>.</param>
        void RemoveChild(IChildSource child);

        /// <summary>
        /// Notifies the child sources of changes they must observe.
        /// </summary>
        /// <param name="crud">A <see cref="CRUD"/> enumeration value indicating the type of operation.</param>
        /// <param name="model">The record that has changed.</param>
        void NotifyChildren(CRUD crud, ISQLModel model);
    }

    /// <summary>
    /// <c>Observer Pattern:</c> This interface defines the methods that a <see cref="DataSource"/> must 
    /// implement to act as a child source to receive messages from their parent source.
    /// See also <seealso cref="IParentSource"/>.
    /// </summary>
    public interface IChildSource
    {
        /// <summary>
        /// Gets or sets a reference to the parent source object.
        /// </summary>
        IParentSource? ParentSource { get; set; }

        /// <summary>
        /// Defines the logic for implementing record updates communicated by the parent source.
        /// </summary>
        /// <param name="crud">A <see cref="CRUD"/> enumeration value indicating the type of operation.</param>
        /// <param name="model">The record that has changed.</param>
        void Update(CRUD crud, ISQLModel model);
    }

}
