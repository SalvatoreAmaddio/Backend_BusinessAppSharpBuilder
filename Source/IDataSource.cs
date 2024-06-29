using Backend.Controller;
using Backend.Model;
using System.Collections;

namespace Backend.Source
{
    /// <summary>
    /// Represents a data source that supports collection, child source, and disposable functionalities.
    /// </summary>
    public interface IDataSource : ICollection, IChildSource, IDisposable
    {
        /// <summary>
        /// Gets or sets the controller to which this data source is associated.
        /// </summary>
        IAbstractSQLModelController? Controller { get; set; }

        /// <summary>
        /// Returns the position of the records within the data source.
        /// </summary>
        /// <returns>A string representing the position of the records.</returns>
        string RecordPositionDisplayer();
    }

    /// <summary>
    /// Represents a generic data source that supports collection, child source, and disposable functionalities for a specific model type.
    /// </summary>
    /// <typeparam name="M">The type of the model, which must implement <see cref="ISQLModel"/> and have a parameterless constructor.</typeparam>
    public interface IDataSource<M> : IDataSource where M : ISQLModel, new()
    {
        /// <summary>
        /// Returns the enumerator as an <see cref="INavigator{M}"/> object.
        /// </summary>
        /// <returns>An <see cref="INavigator{M}"/> object that allows navigation through the data source.</returns>
        INavigator<M> Navigate();
    }

}
