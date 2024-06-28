using Backend.Database;
using Backend.Model;
using Backend.Source;

namespace Backend.Controller
{
    public interface IAbstractSQLModelController : IDisposable
    {
        /// <summary>
        /// Gets a string representing the Record position to be displayed
        /// </summary>
        public string Records { get; }

        /// <summary>
        /// An instance of a <see cref="IAbstractDatabase"/>.
        /// </summary>
        /// <value>An object representing a Database that extends <see cref="AbstractDatabase"/></value>
        public IAbstractDatabase Db { get; }

        /// <summary>
        /// The current selected record.
        /// </summary>
        /// <value>An object that implements <see cref="ISQLModel"/> or extends <see cref="AbstractSQLModel"/>, which represents the current selected record</value>
        //public ISQLModel? CurrentModel { get; set; }

        public void SetCurrentRecord(ISQLModel? model);
        public ISQLModel? GetCurrentRecord();

        /// <summary>
        /// A recordsource object that old the collection of records.
        /// </summary>
        /// <value>A RecordSource</value>
        public IDataSource Source { get; }

        public bool EOF { get; }
        /// <summary>
        /// Gets and Sets whether or no a new Record can be added. Default value is true.
        /// </summary>
        /// <value>True if the <see cref="Navigator"/> can add new records.</value>
        public bool AllowNewRecord { get; set; }

        /// <summary>
        /// It tells the RecordSource's Enumerator to go to the next available record.
        /// see <see cref="DataSource"/>
        /// </summary>
        public bool GoNext();

        /// <summary>
        /// It tells the RecordSource's Enumerator to go to the previous available record.
        /// see <see cref="DataSource"/>
        /// </summary>
        public bool GoPrevious();

        /// <summary>
        /// It tells the RecordSource's Enumerator to go to the last record.
        /// see <see cref="DataSource"/>
        /// </summary>
        public bool GoLast();

        /// <summary>
        /// It tells the RecordSource's Enumerator to go to the first record.
        /// see <see cref="DataSource"/>
        /// </summary>
        public bool GoFirst();

        /// <summary>
        /// It tells the RecordSource's Enumerator to go to a given record based on its zero-based position.
        /// see <see cref="DataSource"/>
        /// </summary>
        /// <param name="index">the zero-based index of the Record to go to.</param>
        public bool GoAt(int index);

        /// <summary>
        /// It finds the given record and tells the RecordSource's Enumerator to go to its zero-based position.
        /// see <see cref="DataSource"/>
        /// </summary>
        /// <param name="record">the record to move to.</param>
        public bool GoAt(ISQLModel? record);

        /// <summary>
        /// It tells the RecordSource's Enumerator that a new record will be added.
        /// see <see cref="DataSource"/>
        /// </summary>
        public bool GoNew();

        /// <summary>
        /// It performs a Insert or Update Statement based on the <see cref="CurrentModel"/>'s <see cref="INotifier.IsDirty"/> property and the <see cref="ISQLModel.IsNewRecord"/> method.
        /// <list type="bullet">
        /// <item>
        /// <term>update</term>
        /// <description> (IsDirty = True) AND (IsNewRecord() = False)</description>
        /// </item>
        /// <item>
        /// <term>insert</term>
        /// <description> (IsDirty = True) AND (IsNewRecord() = True)</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <returns>true if the record was successfully altered.</returns>
        /// <exception cref="NoModelException">Thrown if the <see cref="Model"/> is null.</exception>
        public bool AlterRecord(string? sql = null, List<QueryParameter>? parameters = null);

        /// <summary>
        /// It deletes the <see cref="CurrentModel"/> from the database.
        /// </summary>
        /// <exception cref="NoModelException">Thrown if the <see cref="Model"/> is null.</exception>
        public void DeleteRecord(string? sql = null, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Returns the <see cref="Source"/> Property as a <see cref="ICollection{ISQLModel}"/>
        /// </summary>
        public ICollection<ISQLModel>? SourceAsCollection();
    }
}