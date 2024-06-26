using Backend.Model;

namespace Backend.Database
{
    public class EntityMap : IDisposable
    {
        private readonly List<EntityTree> _children = [];

        public void AddChild(EntityTree tree)
        {
            _children.Add(tree);
        }

        public IEnumerable<EntityTree> FetchParentsOfNode<T>()
        {
            foreach (var child in _children)
            {
                EntityTree? node = child.FetchNode<T>();
                if (node != null) yield return child;
            }
        }

        public void PrintStructure()
        {
            foreach (var child in _children)
            {
                child.PrintStructure();
            }
        }

        public void Dispose()
        {
            foreach (var child in _children) 
            { 
                child.Dispose();
            }
            _children.Clear();
            GC.SuppressFinalize(this);
        }

    }

    public class EntityTree : IDisposable
    {
        public Type Type { get; }
        public string Name => Type.Name;
        private readonly ISQLModel? _node;
        private readonly List<EntityTree> _children = [];
        public string? PrimaryKeyName => _node?.GetPrimaryKey()?.Name;

        public EntityTree(Type type)
        {
            Type = type;

            try
            {
                _node = (ISQLModel?)Activator.CreateInstance(Type);
            }
            catch
            {
                throw new Exception("type must be an instance of ISQLModel");
            }

            AddChildren();
        }

        private void AddChildren()
        {
            IEnumerable<ITableField>? foreignKeys = _node?.GetForeignKeys();
            if (foreignKeys == null) return;

            foreach (var field in foreignKeys)
                _children.Add(new(((IFKField)field).ClassType));
        }

        public bool HasNode<T>()
        {
            foreach (var child in _children)
            {
                if (child.Type == typeof(T))
                    return true;
            }
            return false;
        }

        public EntityTree? FetchNode<T>()
        {
            foreach (var child in _children)
            {
                if (child.Type == typeof(T))
                    return child;
            }
            return null;
        }

        public void PrintStructure()
        {
            Console.WriteLine($"{Name}:");
            foreach (var child in _children)
            {
                Console.Write($"\t- ");
                child.PrintStructure();
            }
        }

        public override string ToString() => $"EntityTree<{Name}>";

        public void Dispose()
        {
            foreach(var child in _children) 
            { 
                child.Dispose();
            }

            _children.Clear();
            GC.SuppressFinalize(this);
        }
    }

}
