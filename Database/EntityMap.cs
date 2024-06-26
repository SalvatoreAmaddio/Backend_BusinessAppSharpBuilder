﻿using Backend.Enums;
using Backend.Model;

namespace Backend.Database
{
    /// <summary>
    /// Represents a Tree Data Structure for each <see cref="ISQLModel"/> object and their <see cref="FK"/>.
    /// </summary>
    public class EntityMap : IDisposable
    {
        private readonly List<EntityTree> _children = [];

        public void AddChild(EntityTree tree) => _children.Add(tree);

        public IEnumerable<EntityTree> FetchParentsOfNode<T>()
        {
            foreach (var child in _children)
            {
                EntityTree? node = child.FetchNode<T>();
                if (node != null) yield return child;
            }
        }

        public IEnumerable<EntityTree> FetchParentsOfNode(string name)
        {
            foreach (var child in _children)
            {
                EntityTree? node = child.FetchNode(name);
                if (node != null) yield return child;
            }
        }

        public void PrintStructure()
        {
            foreach (var child in _children)
                child.PrintStructure();
        }

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
        private readonly List<EntityTree> _children = [];
        private string Name => _type.Name;
        private IAbstractDatabase? Db => DatabaseManager.Find(Name);

        public EntityTree(Type type)
        {
            _type = type;

            try
            {
                _node = (ISQLModel?)Activator.CreateInstance(_type);
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
                if (child._type == typeof(T))
                    return true;
            }
            return false;
        }

        public EntityTree? FetchNode(string name)
        {
            foreach (var child in _children)
            {
                if (child._type.Name.Equals(name))
                    return child;
            }
            return null;
        }

        public EntityTree? FetchNode<T>()
        {
            foreach (var child in _children)
            {
                if (child._type == typeof(T))
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

        public IEnumerable<ISQLModel>? GetRecordsHaving(ISQLModel model) => Db?.MasterSource.Where(s => FetchToRemove(s, model)).ToList();

        public void RemoveFromMasterSource(ISQLModel record) => Db?.MasterSource.Remove(record);
        
        public void NotifyMasterSourceChildren(ISQLModel record) => Db?.MasterSource?.NotifyChildren(CRUD.DELETE, record);

        private static bool FetchToRemove(ISQLModel model, ISQLModel? mod)
        {
            string? propName = mod?.GetTableName();
            if (string.IsNullOrEmpty(propName)) return false;
            object? obj = model.GetPropertyValue(propName);
            if (obj == null) return false;
            bool res = obj.Equals(mod);
            return res;
        }

        public void Dispose()
        {
            foreach(var child in _children) 
            { 
                child.Dispose();
            }

            _children.Clear();
            GC.SuppressFinalize(this);
        }

        public override string ToString() => $"EntityTree<{Name}>";

    }

}
