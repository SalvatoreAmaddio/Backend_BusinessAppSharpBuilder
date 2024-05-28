using Backend.Model;
using Backend.Source;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Backend.Controller
{
    /// <summary>
    /// This class follows the Singleton pattern. 
    /// It is a collections of List&lt;IAbstractSQLModelController> that can be used accross the application.
    /// </summary>
    public sealed class ControllerManager
    {
        private static readonly Lazy<ControllerManager> lazyInstance = new(() => new ControllerManager());
        private readonly List<IAbstractSQLModelController> Controllers;
        public static ControllerManager Do => lazyInstance.Value;

        /// <summary>
        /// The number of IAbstractDatabases.
        /// </summary>
        /// <value>An Integer.</value>
        public int Count => Controllers.Count;   
        private ControllerManager()
        {
            Controllers = [];
        }

        /// <summary>
        /// It adds a IAbstractSQLModelController object.
        /// </summary>
        /// <param name="controller">An object implementing <see cref="IAbstractDatabase"/></param>
        public void Add(IAbstractSQLModelController controller) => Controllers.Add(controller);

        /// <summary>
        /// Gets a Controller based on its zero-based position index.
        /// <para/>
        /// For Example:
        /// <code>
        /// IAbstractSQLModelController controller = ControllerManager.Do[0]; //get the first IAbstractSQLModelController;
        /// </code>
        /// </summary>
        /// <param name="index">zero-based position.</param>
        /// <returns>An IAbstractSQLModelController</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public IAbstractSQLModelController this[int index] => (index < 0 || index >= Controllers.Count) ? throw new IndexOutOfRangeException() : Controllers[index];

    }

}
