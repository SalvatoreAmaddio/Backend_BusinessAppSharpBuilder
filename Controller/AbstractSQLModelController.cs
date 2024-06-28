using Backend.Database;
using Backend.Exceptions;
using Backend.Model;
using Backend.Source;
using Backend.Enums;
using Backend.Events;

namespace Backend.Controller
{
    public abstract class AbstractSQLModelController<M> : IAbstractSQLModelController where M : ISQLModel, new()
    {
        #region Variables
        protected bool _allowNewRecord = true;
        #endregion

        #region Properties
        public IAbstractDatabase Db { get; protected set; } = null!;        
        public IDataSource Source => DataSource;
        public IDataSource<M> DataSource { get; protected set; }
        protected INavigator<M> Navigator => DataSource.Navigate();
        public virtual bool AllowNewRecord
        {
            get => _allowNewRecord;
            set 
            {
                _allowNewRecord = value;
                Navigator.AllowNewRecord = value;
            } 
        }
        public virtual M? CurrentRecord { get; set; }
        public virtual string Records { get; protected set; } = string.Empty;
        public bool EOF => Navigator.EOF;
        #endregion

        public void SetCurrentRecord(ISQLModel? model) => CurrentRecord = (M?)model;
        public ISQLModel? GetCurrentRecord() => CurrentRecord;

        #region Events
        public event AfterRecordNavigationEventHandler? AfterRecordNavigation;
        public event BeforeRecordNavigationEventHandler? BeforeRecordNavigation;
        #endregion

        public AbstractSQLModelController()
        {
            Db = DatabaseManager.Find<M>() ?? throw new NullReferenceException();
            DataSource = InitSource();
            GoFirst();
        }

        /// <summary>
        /// Override this method to set the <see cref="DataSource"/> property on Object's Init
        /// </summary>
        /// <returns></returns>
        protected virtual IDataSource<M> InitSource() => new DataSource<M>(Db, this);
        public ICollection<ISQLModel>? SourceAsCollection()
        {
            try
            {
                return (ICollection<ISQLModel>)Source;
            }
            catch 
            {
                return null;
            }
        }

        #region GoTo
        /// <summary>
        /// It checks if the <see cref="CurrentRecord"/>'s property meets the conditions to be updated. This method is called whenever the <see cref="Navigator"/> moves.
        /// </summary>
        /// <returns>true if the Navigator can move.</returns>
        protected virtual bool CanMove()
        {
            if (CurrentRecord != null)
            {
                if (CurrentRecord.IsNewRecord()) return true;
                if (!CurrentRecord.AllowUpdate()) return false;
            }

            return true;
        }

        public virtual bool GoNext()
        {
            if (!CanMove()) return false;
            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoNext)) return false; //Event was cancelled

            bool moved = Navigator.GoNext();
            if (!moved)
            {
                Records = Source.RecordPositionDisplayer();
                if (Source.Count == 0)
                    CurrentRecord = default;
                return Navigator.EOF ? GoNew() : false;
            }

            CurrentRecord = Navigator.Current;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoNext)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoPrevious()
        {
            if (!CanMove()) return false;

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoPrevious)) return false; //Event was cancelled

            bool moved = Navigator.GoPrevious();
            if (!moved) return false;

            try 
            {
                CurrentRecord = Navigator.Current;
            }
            catch 
            { 
                return false;
            }

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoPrevious)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoLast()
        {
            if (!CanMove()) return false;

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoLast)) return false; //Event was cancelled

            bool moved = Navigator.GoLast();

            if (!moved)
            {
                Records = Source.RecordPositionDisplayer();
                if (Source.Count == 0)
                    CurrentRecord = default;
                return false;
            }

            CurrentRecord = Navigator.Current;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoLast)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoFirst()
        {
            if (!CanMove()) return false;

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoFirst)) return false; //Event was cancelled

            bool moved = Navigator.GoFirst();

            if (!moved)
            {
                Records = Source.RecordPositionDisplayer();
                if (Source.Count == 0)
                    CurrentRecord = default;
                return false;
            }

            CurrentRecord = Navigator.Current;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoFirst)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoNew()
        {
            if (!AllowNewRecord) return false;
            if (!CanMove()) return false;
            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoNew)) return false; //Event was cancelled
            bool moved = Navigator.GoNew();

            if (!moved)
            {
                Records = Source.RecordPositionDisplayer();
                if (Source.Count == 0)
                    CurrentRecord = default;
                return false;
            }

            CurrentRecord = new M();

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoNew)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return moved;   
        }

        public virtual bool GoAt(int index)
        {
            if (!CanMove()) return false;

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoAt)) return false; //Event was cancelled

            bool moved = Navigator.GoAt(index);
            if (!moved) return false;
            CurrentRecord = Navigator.Current;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoAt)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoAt(ISQLModel? record)
        {
            if (!CanMove()) return false;
            if (record == null) 
            {
                CurrentRecord = default;
                Records = Source.RecordPositionDisplayer();
                return false;
            }

            if (record.IsNewRecord()) return GoNew();

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoAt)) return false; //Event was cancelled

            bool moved = Navigator.GoAt(record);
            if (!moved) return false;

            CurrentRecord = (M?)record;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoAt)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }
        #endregion

        #region CRUD Operations
        public void DeleteRecord(string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (CurrentRecord == null) throw new NoModelException();
            CurrentRecord.InvokeBeforeRecordDelete();
            Db.Model = CurrentRecord;
            DeleteOrphan(CurrentRecord);
            Db.Crud(CRUD.DELETE, sql, parameters);
            if (Db.Model.IsNewRecord()) //this occurs in ListView objects when you add a new record but then decided to delete it.
            {
                SourceAsCollection()?.Remove(Db.Model); //remove the record from the Source, thereof from the ListView
                if (Navigator.BOF && !Navigator.NoRecords) GoFirst(); //The record is deleted, handle the direction that the Navigator object should point at.
                else GoPrevious(); //if we still have records move back.
            }
            else
                Db?.MasterSource?.NotifyChildren(CRUD.DELETE, Db.Model); //notify children sources that the master source has changed.
        }

        protected abstract void OnUIApplication(EntityTree tree, ISQLModel record);

        private async void DeleteOrphan(ISQLModel? model)
        {
            if (model == null) return;
            await Task.Run(() =>
            {
                IEnumerable<EntityTree> trees = DatabaseManager.Map.FetchParentsOfNode(model.GetType().Name);
                Parallel.ForEachAsync(trees, async (tree, t) =>
                {
                    IEnumerable<ISQLModel>? toRemove = tree.GetRecordsHaving(model);
                    if (toRemove == null) return;
                    foreach (ISQLModel record in toRemove)
                    {
                        await Task.Run(() => DeleteOrphan(record), CancellationToken.None);
                        record.InvokeBeforeRecordDelete();
                        tree.RemoveFromMasterSource(record);
                        await Task.Delay(1, CancellationToken.None);
                        try
                        {
                            tree.NotifyMasterSourceChildren(record);
                        }
                        catch
                        {
                            OnUIApplication(tree, record);
                        }
                    }
                });
            });
        }
        public virtual bool AlterRecord(string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (CurrentRecord == null) throw new NoModelException();
            if (!CurrentRecord.AllowUpdate()) return false; //cannot update.
            Db.Model = CurrentRecord;
            CRUD crud = (!Db.Model.IsNewRecord()) ? CRUD.UPDATE : CRUD.INSERT;
            Db.Crud(crud, sql, parameters);
            Db.MasterSource?.NotifyChildren(crud, Db.Model);
            GoAt(CurrentRecord);
            return true;
        }
        #endregion

        #region Event Invokers
        protected bool InvokeAfterRecordNavigationEvent(RecordMovement recordMovement)
        {
            AllowRecordMovementArgs args = new(recordMovement);
            AfterRecordNavigation?.Invoke(this, args);
            return args.Cancel; // if returns true, the event is cancelled
        }

        protected bool InvokeBeforeRecordNavigationEvent(RecordMovement recordMovement)
        {
            AllowRecordMovementArgs args = new(recordMovement);
            BeforeRecordNavigation?.Invoke(this, args);
            return args.Cancel; // if returns true, the event is cancelled
        }
        #endregion

        #region Disposer
        protected virtual void DisposeEvents() 
        {
            AfterRecordNavigation = null;
            BeforeRecordNavigation = null;
        }

        public virtual void Dispose()
        {
            DisposeEvents();
            Source.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}