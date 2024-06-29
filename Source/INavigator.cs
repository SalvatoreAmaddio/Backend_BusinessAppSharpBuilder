using Backend.Model;

namespace Backend.Source
{
    /// <summary>
    /// Extends the <see cref="IEnumerator{T}"/> interface to add extra functionalities 
    /// for dealing with collections of objects implementing <see cref="ISQLModel"/>. 
    /// This enumerator can move up and down the <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="M">A type that implements <see cref="ISQLModel"/> and has a parameterless constructor.</typeparam>
    public interface INavigator<M> : IEnumerator<M>, INav where M : ISQLModel, new()
    {
        /// <summary>
        /// Gets the current record in the collection. Do not use <see cref="IEnumerator{T}.Current"/> for fetch the currently selected record.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public M CurrentRecord { get; }

    }

    /// <summary>
    /// Defines a set of methods and properties that each <see cref="IDataSource{M}"/> must implement as their <see cref="INavigator{M}"/>.
    /// </summary>
    public interface INav : IDisposable
    {
        /// <summary>
        /// Gets the number of records in the collection.
        /// </summary>
        /// <value>The number of records in the collection.</value>
        int RecordCount { get; }

        /// <summary>
        /// Indicates whether the enumerator can add a new record. See also <see cref="GoNew"/>.
        /// </summary>
        bool AllowNewRecord { get; set; }

        /// <summary>
        /// Indicates whether there are no records at all. 
        /// This property returns false if <see cref="Index"/> is pointing to a new record.
        /// </summary>
        bool NoRecords { get; }

        /// <summary>
        /// Gets or sets the zero-based position within the array. Default value is -1.
        /// </summary>
        /// <value>The current zero-based position within the array.</value>
        int Index { get; set; }

        /// <summary>
        /// Indicates whether the collection is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Indicates whether <see cref="Index"/> is at the beginning of the collection.
        /// </summary>
        bool BOF { get; }

        /// <summary>
        /// Indicates whether <see cref="Index"/> is at the end of the collection.
        /// </summary>
        bool EOF { get; }

        /// <summary>
        /// Indicates whether the enumerator is pointing at a new record.
        /// </summary>
        bool IsNewRecord { get; }

        /// <summary>
        /// Gets the one-based position within the collection.
        /// <para>For example: array[0] is record number 1, array[1] is record number 2, and so on.</para>
        /// </summary>
        /// <value>The one-based position within the collection.</value>
        int RecNum { get; }

        /// <summary>
        /// Moves the enumerator to the next element in the collection.
        /// </summary>
        /// <returns>True if the enumerator moved successfully; otherwise, false.</returns>
        bool GoNext();

        /// <summary>
        /// Moves the enumerator to the previous element in the collection.
        /// </summary>
        /// <returns>True if the enumerator moved successfully; otherwise, false.</returns>
        bool GoPrevious();

        /// <summary>
        /// Moves the enumerator to the first element in the collection.
        /// </summary>
        /// <returns>True if the enumerator moved successfully; otherwise, false.</returns>
        bool GoFirst();

        /// <summary>
        /// Moves the enumerator to the last element in the collection.
        /// </summary>
        /// <returns>True if the enumerator moved successfully; otherwise, false.</returns>
        bool GoLast();

        /// <summary>
        /// Moves the enumerator beyond the last element in the collection, indicating a new record can be added.
        /// Returns false if <see cref="AllowNewRecord"/> is set to false.
        /// </summary>
        /// <returns>True if the enumerator moved successfully; otherwise, false.</returns>
        bool GoNew();

        /// <summary>
        /// Moves the enumerator to the element at the specified zero-based position in the collection.
        /// </summary>
        /// <param name="index">The zero-based position to move the enumerator to.</param>
        /// <returns>True if the enumerator moved successfully; otherwise, false.</returns>
        bool GoAt(int index);

        /// <summary>
        /// Moves the enumerator to the specified element in the collection.
        /// </summary>
        /// <param name="record">An <see cref="ISQLModel"/> object representing the record to move to.</param>
        /// <returns>True if the enumerator moved successfully; otherwise, false.</returns>
        bool GoAt(object record);
    }
}