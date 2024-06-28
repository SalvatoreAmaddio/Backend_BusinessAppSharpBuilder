using Backend.Controller;
using Backend.Model;
using System.Collections;

namespace Backend.Source
{
    public interface IDataSource : ICollection, IChildSource, IDisposable
    {
        /// <summary>
        /// The Controller to which this RecordSource is associated to.
        /// </summary>
        public IAbstractSQLModelController? Controller { get; set; }


        /// <summary>
        /// Return a the position of the records within the RecordSource.
        /// </summary>
        /// <returns>A string.</returns>
        public string RecordPositionDisplayer();
    }

    public interface IDataSource<M> : IDataSource where M : ISQLModel, new()
    {
        /// <summary>
        /// Return the Enumerator as an <see cref="INavigator"/> object.
        /// </summary>
        /// <returns>A <see cref="INavigator"/> object.</returns>
        public INavigator<M> Navigate();
    }

}
