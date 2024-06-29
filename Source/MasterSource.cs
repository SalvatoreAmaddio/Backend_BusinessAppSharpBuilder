using Backend.Enums;
using Backend.Model;

namespace Backend.Source
{
    /// <summary>
    /// Represents the master source that holds a collection of <see cref="ISQLModel"/> instances and acts as a parent source for notifying child sources.
    /// </summary>
    public class MasterSource : List<ISQLModel>, IParentSource, IDisposable
    {
        /// <summary>
        /// Gets the list of child sources associated with this master source.
        /// </summary>
        private List<IChildSource> Children { get; } = new List<IChildSource>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterSource"/> class.
        /// </summary>
        public MasterSource() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterSource"/> class with the specified list of <see cref="ISQLModel"/> instances.
        /// </summary>
        /// <param name="list">The list of <see cref="ISQLModel"/> instances to initialize the master source with.</param>
        public MasterSource(List<ISQLModel> list) : base(list) { }

        public void AddChild(IChildSource child)
        {
            child.ParentSource = this;
            Children.Add(child);
        }

        public void NotifyChildren(CRUD crud, ISQLModel model)
        {
            foreach (IChildSource child in Children)
                child.Update(crud, model);
        }

        public void RemoveChild(IChildSource child) => Children.Remove(child);

        /// <summary>
        /// Disposes of the resources used by the master source and its records.
        /// </summary>
        public void Dispose()
        {
            foreach (ISQLModel record in this)
                record.Dispose();
            Clear();
            Children.Clear();
            GC.SuppressFinalize(this);
        }
    }

}