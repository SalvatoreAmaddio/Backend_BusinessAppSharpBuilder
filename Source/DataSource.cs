using Backend.Controller;
using Backend.Database;
using Backend.Exceptions;
using Backend.Model;
using System.Collections.ObjectModel;
using Backend.Enums;

namespace Backend.Source
{
    /// <summary>
    /// This class extends the <see cref="Collection{T}"/> and deals with IEnumerable&lt;<see cref="ISQLModel"/>&gt;. As Enumerator it uses a <see cref="INavigator"/>.
    /// see also the <seealso cref="Navigator{T}"/> class.
    /// </summary>
    public class DataSource<M> : Collection<M>, IDataSource<M>, IChildSource where M : ISQLModel, new()
    {

        private Navigator<M>? _navigator;
        public IParentSource? ParentSource { get; set; }
        public IAbstractSQLModelController? Controller { get; set; }

        #region Constructor
        /// <summary>
        /// Parameterless Constructor to instantiate a RecordSource object.
        /// </summary>
        public DataSource() { }

        /// <summary>
        /// It instantiates a RecordSource object filled with the given IEnumerable&lt;<see cref="ISQLModel"/>&gt;.
        /// </summary>
        /// <param name="source">An IEnumerable&lt;<see cref="ISQLModel"/>&gt;</param>
        public DataSource(IList<M> source) : base(source)
        {
        }

        /// <summary>
        /// It instantiates a RecordSource object filled with the given IEnumerable&lt;<see cref="ISQLModel"/>&gt;.
        /// </summary>
        /// <param name="source">An IEnumerable&lt;<see cref="ISQLModel"/>&gt;</param>
        public DataSource(IEnumerable<M> source) : base(source.ToList())
        {
        }

        /// <summary>
        /// It instantiates a RecordSource object filled with the given <see cref="IAbstractDatabase.MasterSource"/> IEnumerable.
        /// This constructor will consider this RecordSource object as a child of the <see cref="IAbstractDatabase.MasterSource"/>
        /// </summary>
        /// <param name="db">An instance of <see cref="IAbstractDatabase"/></param>
        public DataSource(IAbstractDatabase db) : this(db.MasterSource.Cast<M>()) => db.MasterSource.AddChild(this);

        /// <summary>
        /// It instantiates a RecordSource object filled with the given <see cref="IAbstractDatabase.MasterSource"/> IEnumerable.
        /// This constructor will consider this RecordSource object as a child of the <see cref="IAbstractDatabase.MasterSource"/>
        /// </summary>
        /// <param name="db">An instance of <see cref="IAbstractDatabase"/></param>
        /// <param name="controller">An instance of <see cref="IAbstractSQLModelController"/></param>
        public DataSource(IAbstractDatabase db, IAbstractSQLModelController controller) : this(db) => Controller = controller;
        #endregion

        #region Enumerator
        /// <summary>
        /// Override the default <c>GetEnumerator()</c> method to replace it with a <see cref="ISourceNavigator"></see> object./>
        /// </summary>
        /// <returns>An Enumerator object.</returns>
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

        public INavigator<M> Navigate() => (INavigator<M>)GetEnumerator();
        #endregion

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
        /// It takes an IAsyncEnumerable, converts it to a List and returns a RecordSource object.
        /// </summary>
        /// <param name="source"> An IAsyncEnumerable&lt;ISQLModel></param>
        /// <returns>Task&lt;RecordSource></returns>
        public static async Task<DataSource<M>> CreateFromAsyncList(IAsyncEnumerable<M> source) =>
        new DataSource<M>(await source.ToListAsync());

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

        public void Dispose()
        {
            ParentSource?.RemoveChild(this);
            Controller?.Dispose();
            _navigator?.Dispose();
            Clear();
            GC.SuppressFinalize(this);
        }

        ~DataSource()
        {
            Dispose();
        }
    }
}