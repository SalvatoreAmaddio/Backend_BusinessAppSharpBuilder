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
    /// see also the <seealso cref="Navigator"/> class.
    /// </summary>
    public class RecordSource : Collection<ISQLModel>, IRecordSource, IChildSource
    {

        private INavigator? navigator;
        public IParentSource? ParentSource { get; set; }
        public IAbstractSQLModelController? Controller { get; set; }

        #region Constructor
        /// <summary>
        /// Parameterless Constructor to instantiate a RecordSource object.
        /// </summary>
        public RecordSource() { }

        /// <summary>
        /// It instantiates a RecordSource object filled with the given IEnumerable&lt;<see cref="ISQLModel"/>&gt;.
        /// </summary>
        /// <param name="source">An IEnumerable&lt;<see cref="ISQLModel"/>&gt;</param>
        public RecordSource(IList<ISQLModel> source) : base(source)
        {
        }

        /// <summary>
        /// It instantiates a RecordSource object filled with the given IEnumerable&lt;<see cref="ISQLModel"/>&gt;.
        /// </summary>
        /// <param name="source">An IEnumerable&lt;<see cref="ISQLModel"/>&gt;</param>
        public RecordSource(IEnumerable<ISQLModel> source) : base(source.ToList())
        {
        }

        /// <summary>
        /// It instantiates a RecordSource object filled with the given <see cref="IAbstractDatabase.MasterSource"/> IEnumerable.
        /// This constructor will consider this RecordSource object as a child of the <see cref="IAbstractDatabase.MasterSource"/>
        /// </summary>
        /// <param name="db">An instance of <see cref="IAbstractDatabase"/></param>
        public RecordSource(IAbstractDatabase db) : this(db.MasterSource) => db.MasterSource.AddChild(this);

        /// <summary>
        /// It instantiates a RecordSource object filled with the given <see cref="IAbstractDatabase.MasterSource"/> IEnumerable.
        /// This constructor will consider this RecordSource object as a child of the <see cref="IAbstractDatabase.MasterSource"/>
        /// </summary>
        /// <param name="db">An instance of <see cref="IAbstractDatabase"/></param>
        /// <param name="controller">An instance of <see cref="IAbstractSQLModelController"/></param>
        public RecordSource(IAbstractDatabase db, IAbstractSQLModelController controller) : this(db) => Controller = controller;
        #endregion

        #region Enumerator
        /// <summary>
        /// Override the default <c>GetEnumerator()</c> method to replace it with a <see cref="ISourceNavigator"></see> object./>
        /// </summary>
        /// <returns>An Enumerator object.</returns>
        public new IEnumerator<ISQLModel> GetEnumerator()
        {
            if (navigator != null)
            {
                navigator = new Navigator(this, navigator.Index, navigator.AllowNewRecord);
                return navigator;
            }
            navigator = new Navigator(this);
            return navigator;
        }

        public INavigator Navigate() => (INavigator)GetEnumerator();
        #endregion

        public virtual void Update(CRUD crud, ISQLModel model)
        {
            switch (crud)
            {
                case CRUD.INSERT:
                    Add(model);
                    Controller?.GoLast();
                    break;
                case CRUD.UPDATE:
                    break;
                case CRUD.DELETE:
                    bool removed = Remove(model);
                    if (!removed) break;
                    if (navigator != null) 
                    {
                        if (navigator.BOF && !navigator.NoRecords) Controller?.GoFirst();
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
        public static async Task<RecordSource> CreateFromAsyncList(IAsyncEnumerable<ISQLModel> source) =>
        new RecordSource(await source.ToListAsync());

        public virtual string RecordPositionDisplayer()
        {
            if (navigator == null) throw new NoNavigatorException();
            return true switch
            {
                true when navigator.NoRecords => "NO RECORDS",
                true when navigator.IsNewRecord => "New Record",
                _ => $"Record {navigator?.RecNum} of {navigator?.RecordCount}",
            };
        }

        public void Dispose()
        {
            ParentSource?.RemoveChild(this);
            Controller?.Dispose();
            navigator = null;
            GC.SuppressFinalize(this);
        }

        public void Combine(RecordSource child, string key)
        {
            RecordSource master = new(this.ToList());
            int index = -1;

            foreach (ISQLModel record in master)
            {
                IEnumerable<ISQLModel> subGroup = child.Where(s => FilterSubGroupBy(s, key, record)).ToList();
                index++;
                foreach (ISQLModel subRecord in subGroup)
                {
                    index++;
                    this.Insert(index, subRecord);
                }
            }
        }

        private static bool FilterSubGroupBy(ISQLModel record, string key, ISQLModel record2)
        {
            object? obj = record.GetPropertyValue(key);
            if (obj == null) return false;
            return obj.Equals(record2);
        }

        ~RecordSource()
        {
            Dispose();
        }
    }
}