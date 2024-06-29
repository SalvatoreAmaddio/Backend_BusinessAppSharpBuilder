using Backend.Database;
using Backend.Events;
using Backend.Model;
using Backend.Source;

namespace Backend.Controller
{
    /// <summary>
    /// Defines the contract for a SQL model controller that handles CRUD operations,
    /// navigation, and record management within a data source. This interface provides
    /// methods for interacting with records, navigating through them, and performing
    /// database operations such as insert, update, and delete. It also includes properties
    /// for accessing the database and data source, as well as events for record navigation.
    /// </summary>
    public interface IAbstractSQLModelController : IDisposable
    {
        /// <summary>
        /// Gets a string representing the record position to be displayed.
        /// </summary>
        string Records { get; }

        /// <summary>
        /// Gets a reference to an <see cref="IAbstractDatabase"/>.
        /// </summary>
        IAbstractDatabase Db { get; }

        /// <summary>
        /// Gets a reference to an <see cref="IDataSource"/> object that holds a collection of records.
        /// </summary>
        IDataSource Source { get; }

        /// <summary>
        /// Indicates whether the <see cref="IAbstractSQLModelController"/> has reached the end of the <see cref="Source"/>.
        /// </summary>
        bool EOF { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a new record can be added. Default value is true.
        /// </summary>
        /// <value>True if the controller allows new records to be inserted in the <see cref="Source"/>; otherwise, false.</value>
        bool AllowNewRecord { get; set; }

        /// <summary>
        /// Sets the currently selected record in the <see cref="IAbstractSQLModelController"/>.
        /// </summary>
        /// <param name="model">The record to set as the current record.</param>
        void SetCurrentRecord(ISQLModel? model);

        /// <summary>
        /// Gets the currently selected record in the <see cref="IAbstractSQLModelController"/>.
        /// </summary>
        /// <returns>The current record as an <see cref="ISQLModel"/>.</returns>
        ISQLModel? GetCurrentRecord();

        /// <summary>
        /// Moves to the next available record in the <see cref="Source"/>.
        /// </summary>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        bool GoNext();

        /// <summary>
        /// Moves to the previous available record in the <see cref="Source"/>.
        /// </summary>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        bool GoPrevious();

        /// <summary>
        /// Moves to the last record in the <see cref="Source"/>.
        /// </summary>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        bool GoLast();

        /// <summary>
        /// Moves to the first record in the <see cref="Source"/>.
        /// </summary>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        bool GoFirst();

        /// <summary>
        /// Moves to a given record based on its zero-based position.
        /// </summary>
        /// <param name="index">The zero-based index of the record to move to.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        bool GoAt(int index);

        /// <summary>
        /// Finds the given record and moves to its zero-based position in the <see cref="Source"/>.
        /// </summary>
        /// <param name="record">The record to move to.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        bool GoAt(ISQLModel? record);

        /// <summary>
        /// Prepares the <see cref="Source"/> for adding a new record.
        /// </summary>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        bool GoNew();

        /// <summary>
        /// Performs an insert or update statement based on the <see cref="ISQLModel.IsNewRecord"/> method.
        /// <list type="bullet">
        /// <item>
        /// <term>Update</term>
        /// <description> <c>IsNewRecord() = False</c></description>
        /// </item>
        /// <item>
        /// <term>Insert</term>
        /// <description> <c>IsNewRecord() = True</c></description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="sql">An optional SQL statement to execute.</param>
        /// <param name="parameters">Optional query parameters to include in the operation.</param>
        /// <returns>True if the record was successfully altered; otherwise, false.</returns>
        /// <exception cref="NoModelException">Thrown if the <see cref="Model"/> is null.</exception>
        bool AlterRecord(string? sql = null, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Deletes the current record from the database.
        /// </summary>
        /// <param name="sql">An optional SQL statement to execute.</param>
        /// <param name="parameters">Optional query parameters to include in the operation.</param>
        /// <exception cref="NoModelException">Thrown if the <see cref="Model"/> is null.</exception>
        void DeleteRecord(string? sql = null, List<QueryParameter>? parameters = null);

        /// <summary>
        /// Returns the <see cref="Source"/> property as an <see cref="ICollection{ISQLModel}"/>.
        /// </summary>
        /// <returns>The source as a collection of <see cref="ISQLModel"/>.</returns>
        ICollection<ISQLModel>? SourceAsCollection();

        /// <summary>
        /// Occurs after record navigation.
        /// </summary>
        event AfterRecordNavigationEventHandler? AfterRecordNavigation;

        /// <summary>
        /// Occurs before record navigation.
        /// </summary>
        event BeforeRecordNavigationEventHandler? BeforeRecordNavigation;
    }

}