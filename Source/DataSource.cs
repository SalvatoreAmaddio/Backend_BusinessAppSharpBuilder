using Backend.Controller;
using Backend.Database;
using Backend.Exceptions;
using Backend.Model;
using System.Collections.ObjectModel;
using Backend.Enums;

namespace Backend.Source
{
    /// <summary>
    /// This class extends the <see cref="Collection{T}"/> and deals with <see cref="IEnumerable{T}"/> of <see cref="ISQLModel"/>. 
    /// As enumerator, it uses a <see cref="Navigator{T}"/>.
    /// See also the <seealso cref="Navigator{T}"/> class.
    /// </summary>
    public class DataSource<M> : Collection<M>, IDataSource<M>, IChildSource where M : ISQLModel, new()
    {
        private Navigator<M>? _navigator;

        /// <summary>
        /// Gets or sets the parent source of this data source.
        /// </summary>
        public IParentSource? ParentSource { get; set; }

        /// <summary>
        /// Gets or sets the controller to which this data source is associated.
        /// </summary>
        public IAbstractSQLModelController? Controller { get; set; }

        #region Constructor

        /// <summary>
        /// Parameterless constructor to instantiate a DataSource object.
        /// </summary>
        public DataSource() { }

        /// <summary>
        /// Instantiates a DataSource object filled with the given <see cref="IList{T}"/> of <see cref="ISQLModel"/>.
        /// </summary>
        /// <param name="source">An <see cref="IList{T}"/> of <see cref="ISQLModel"/>.</param>
        public DataSource(IList<M> source) : base(source) { }

        /// <summary>
        /// Instantiates a DataSource object filled with the given <see cref="IEnumerable{T}"/> of <see cref="ISQLModel"/>.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> of <see cref="ISQLModel"/>.</param>
        public DataSource(IEnumerable<M> source) : base(source.ToList()) { }

        /// <summary>
        /// Instantiates a DataSource object filled with the given <see cref="IAbstractDatabase.MasterSource"/> <see cref="IEnumerable"/>.
        /// This constructor considers this DataSource object as a child of the <see cref="IAbstractDatabase.MasterSource"/>.
        /// </summary>
        /// <param name="db">An instance of <see cref="IAbstractDatabase"/>.</param>
        public DataSource(IAbstractDatabase db) : this(db.MasterSource.Cast<M>()) => db.MasterSource.AddChild(this);

        /// <summary>
        /// Instantiates a DataSource object filled with the given <see cref="IAbstractDatabase.MasterSource"/> <see cref="IEnumerable"/>.
        /// This constructor considers this DataSource object as a child of the <see cref="IAbstractDatabase.MasterSource"/>.
        /// </summary>
        /// <param name="db">An instance of <see cref="IAbstractDatabase"/>.</param>
        /// <param name="controller">An instance of <see cref="IAbstractSQLModelController"/>.</param>
        public DataSource(IAbstractDatabase db, IAbstractSQLModelController controller) : this(db) => Controller = controller;

        #endregion

        #region Enumerator

        /// <summary>
        /// Overrides the default <c>GetEnumerator()</c> method to replace it with a <see cref="Navigator{T}"/> object.
        /// </summary>
        /// <returns>An enumerator object.</returns>
        public new IEnumerator<M> GetEnumerator()
        {
            if (_navigator != null)
            {
                _navigator = new Navigator<M>(this, _navigator.Index, _navigator.AllowNewRecord);
                return _navigator;
            }
            _navigator = new Navigator<M>(this);
            return _navigator;
        }

        /// <summary>
        /// Returns the enumerator as an <see cref="INavigator{T}"/> object.
        /// </summary>
        /// <returns>An <see cref="INavigator{T}"/> object that allows navigation through the data source.</returns>
        public INavigator<M> Navigate() => (INavigator<M>)GetEnumerator();

        #endregion

        /// <summary>
        /// Updates the data source based on the specified CRUD operation and model.
        /// </summary>
        /// <param name="crud">The CRUD operation to perform.</param>
        /// <param name="model">The model to be used for the operation.</param>
        public virtual void Update(CRUD crud, ISQLModel model)
        {
            switch (crud)
            {
                case CRUD.INSERT:
                    Add((M)model);
                    Controller?.GoLast();
                    break;
                case CRUD.UPDATE:
                    break;
                case CRUD.DELETE:
                    bool removed = Remove((M)model);
                    if (!removed) break;
                    if (_navigator != null)
                    {
                        if (_navigator.BOF && !_navigator.NoRecords) Controller?.GoFirst();
                        else Controller?.GoPrevious();
                    }
                    break;
            }
        }

        /// <summary>
        /// Takes an <see cref="IAsyncEnumerable{T}"/>, converts it to a list, and returns a DataSource object.
        /// </summary>
        /// <param name="source">An <see cref="IAsyncEnumerable{T}"/>.</param>
        /// <returns>A task representing the asynchronous operation, with a DataSource object as the result.</returns>
        public static async Task<DataSource<M>> CreateFromAsyncList(IAsyncEnumerable<M> source) =>
            new DataSource<M>(await source.ToListAsync());

        /// <summary>
        /// Returns a string that represents the position of the records within the data source.
        /// </summary>
        /// <returns>A string representing the record position.</returns>
        /// <exception cref="NoNavigatorException">Thrown when the navigator is null.</exception>
        public virtual string RecordPositionDisplayer()
        {
            if (_navigator == null) throw new NoNavigatorException();
            return true switch
            {
                true when _navigator.NoRecords => "NO RECORDS",
                true when _navigator.IsNewRecord => "New Record",
                _ => $"Record {_navigator?.RecNum} of {_navigator?.RecordCount}",
            };
        }

        /// <summary>
        /// Disposes of the resources used by the data source.
        /// </summary>
        public void Dispose()
        {
            ParentSource?.RemoveChild(this);
            Controller?.Dispose();
            _navigator?.Dispose();
            Clear();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer to ensure resources are released.
        /// </summary>
        ~DataSource()
        {
            Dispose();
        }
    }

}