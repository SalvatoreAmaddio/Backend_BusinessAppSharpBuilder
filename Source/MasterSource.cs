﻿using Backend.Enums;
using Backend.Model;

namespace Backend.Source
{
    public class MasterSource : List<ISQLModel>, IParentSource, IDisposable
    {
        private List<IChildSource> Children { get; } = [];

        public MasterSource() { }

        public MasterSource(List<ISQLModel> list) : base(list) { }

        public void AddChild(IChildSource child)
        {
            child.ParentSource = this;
            Children.Add(child);
        }

        public void NotifyChildren(CRUD crud, ISQLModel model)
        {
            foreach (IChildSource child in Children)
                child.Update(crud, model);
        }

        public void RemoveChild(IChildSource child) => Children.Remove(child);

        public void Dispose()
        {
            foreach (ISQLModel record in this)
                record.Dispose();
            Clear();
            Children.Clear();
            GC.SuppressFinalize(this);
        }
    }
}