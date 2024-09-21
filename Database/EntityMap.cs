using Backend.Enums;
using Backend.Model;

namespace Backend.Database
{
    /// <summary>
    /// Represents a Tree Data Structure for each <see cref="ISQLModel"/> object and their <see cref="FK"/>.
    /// </summary>
    public class EntityMap : IDisposable
    {
        private readonly List<EntityTree> _children = [];

        /// <summary>
        /// Adds a child <see cref="EntityTree"/> to the entity map.
        /// </summary>
        /// <param name="tree">The <see cref="EntityTree"/> to add.</param>
        public void AddChild(EntityTree tree) => _children.Add(tree);

        /// <summary>
        /// Fetches the parent nodes of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of node to fetch parents for.</typeparam>
        /// <returns>An enumerable of parent <see cref="EntityTree"/> objects.</returns>
        public IEnumerable<EntityTree> FetchParentsOfNode<T>()
        {
            foreach (var child in _children)
            {
                EntityTree? node = child.FetchNode<T>();
                if (node != null) yield return child;
            }
        }

        /// <summary>
        /// Fetches the parent nodes of a specified name.
        /// </summary>
        /// <param name="name">The name of the node to fetch parents for.</param>
        /// <returns>An enumerable of parent <see cref="EntityTree"/> objects.</returns>
        public IEnumerable<EntityTree> FetchParentsOfNode(string name)
        {
            foreach (var child in _children)
            {
                EntityTree? node = child.FetchNode(name);
                if (node != null) yield return child;
            }
        }

        /// <summary>
        /// Prints the structure of the entity map to the console.
        /// </summary>
        public void PrintStructure()
        {
            foreach (var child in _children)
                child.PrintStructure();
        }

        /// <summary>
        /// Disposes the entity map and its children.
        /// </summary>
        public void Dispose()
        {
            foreach (var child in _children)
                child.Dispose();

            _children.Clear();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// This class represents a tree structure for a <see cref="ISQLModel"/> and its <see cref="FK"/>.
    /// </summary>
    public class EntityTree : IDisposable
    {
        private readonly Type _type;
        private readonly ISQLModel? _node;
        private readonly List<EntityTree> _children = new List<EntityTree>();
        private string Name => _type.Name;
        private IAbstractDatabase? Db => DatabaseManager.Find(Name);

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTree"/> class.
        /// </summary>
        /// <param name="type">The type of the <see cref="ISQLModel"/>.</param>
        /// <exception cref="Exception">Thrown when the type cannot be instantiated as an <see cref="ISQLModel"/>.</exception>
        public EntityTree(Type type)
        {
            _type = type;

            try
            {
                _node = (ISQLModel?)Activator.CreateInstance(_type);
            }
            catch(Exception ex)
            {
                throw new Exception($"{ex.Message} type must be an instance of ISQLModel, {_type}");
            }

            AddChildren();
        }

        /// <summary>
        /// Adds child nodes to the entity tree based on foreign keys.
        /// </summary>
        private void AddChildren()
        {
            IEnumerable<ITableField>? foreignKeys = _node?.GetForeignKeys();
            if (foreignKeys == null) return;

            foreach (var field in foreignKeys)
                _children.Add(new EntityTree(((IFKField)field).ClassType));
        }

        /// <summary>
        /// Determines whether the tree contains a node of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the node to check for.</typeparam>
        /// <returns>true if the node exists; otherwise, false.</returns>
        public bool HasNode<T>()
        {
            foreach (var child in _children)
            {
                if (child._type == typeof(T))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Fetches a node by its name.
        /// </summary>
        /// <param name="name">The name of the node to fetch.</param>
        /// <returns>The <see cref="EntityTree"/> if found; otherwise, null.</returns>
        public EntityTree? FetchNode(string name)
        {
            foreach (var child in _children)
            {
                if (child._type.Name.Equals(name))
                    return child;
            }
            return null;
        }

        /// <summary>
        /// Fetches a node by its type.
        /// </summary>
        /// <typeparam name="T">The type of the node to fetch.</typeparam>
        /// <returns>The <see cref="EntityTree"/> if found; otherwise, null.</returns>
        public EntityTree? FetchNode<T>()
        {
            foreach (var child in _children)
            {
                if (child._type == typeof(T))
                    return child;
            }
            return null;
        }

        /// <summary>
        /// Prints the structure of the entity tree to the console.
        /// </summary>
        public void PrintStructure()
        {
            Console.WriteLine($"{Name}:");
            foreach (var child in _children)
            {
                Console.Write($"\t- ");
                child.PrintStructure();
            }
        }

        /// <summary>
        /// Gets the records from the database that have a specific relation with the given model.
        /// </summary>
        /// <param name="model">The model to check relations for.</param>
        /// <returns>An enumerable of related <see cref="ISQLModel"/> records.</returns>
        public IEnumerable<ISQLModel>? GetRecordsHaving(ISQLModel model) => Db?.MasterSource.Where(s => FetchToRemove(s, model)).ToList();

        /// <summary>
        /// Removes a record from the master source in the database.
        /// </summary>
        /// <param name="record">The record to remove.</param>
        public void RemoveFromMasterSource(ISQLModel record) => Db?.MasterSource.Remove(record);

        /// <summary>
        /// Notifies the master source children about a deletion operation.
        /// </summary>
        /// <param name="record">The record that was deleted.</param>
        public void NotifyMasterSourceChildren(ISQLModel record) => Db?.MasterSource?.NotifyChildren(CRUD.DELETE, record);

        /// <summary>
        /// Determines whether a model should be removed based on its relation to another model.
        /// </summary>
        /// <param name="model">The model to check.</param>
        /// <param name="mod">The related model.</param>
        /// <returns>true if the model should be removed; otherwise, false.</returns>
        private static bool FetchToRemove(ISQLModel model, ISQLModel? mod)
        {
            string? propName = mod?.GetTableName();
            if (string.IsNullOrEmpty(propName)) return false;
            object? obj = model.GetPropertyValue(propName);
            if (obj == null) return false;
            bool res = obj.Equals(mod);
            return res;
        }

        /// <summary>
        /// Disposes the entity tree and its children.
        /// </summary>
        public void Dispose()
        {
            foreach (var child in _children)
            {
                child.Dispose();
            }

            _children.Clear();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns a string representation of the entity tree.
        /// </summary>
        /// <returns>A string representing the entity tree.</returns>
        public override string ToString() => $"EntityTree<{Name}>";
    }
}
