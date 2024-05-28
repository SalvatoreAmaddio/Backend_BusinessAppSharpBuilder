using Backend.Controller;
using System.Collections;

namespace Backend.Source
{
    public interface IRecordSource : ICollection, IChildSource
    {
        /// <summary>
        /// The Controller to which this RecordSource is associated to.
        /// </summary>
        public IAbstractSQLModelController? Controller { get; set; }

        /// <summary>
        /// Return the Enumerator as an <see cref="INavigator"/> object.
        /// </summary>
        /// <returns>A <see cref="INavigator"/> object.</returns>
        public INavigator Navigate();

        /// <summary>
        /// Return a the position of the records within the RecordSource.
        /// </summary>
        /// <returns>A string.</returns>
        public string RecordPositionDisplayer();
    }

}
