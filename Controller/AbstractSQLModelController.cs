using Backend.Database;
using Backend.Exceptions;
using Backend.Model;
using Backend.Source;
using Backend.Enums;
using Backend.Events;

namespace Backend.Controller
{
    public abstract class AbstractSQLModelController : IAbstractSQLModelController
    {
        #region Variables
        protected bool _allowNewRecord = true;
        #endregion

        #region Properties
        public IAbstractDatabase Db { get; protected set; } = null!;        
        public abstract int DatabaseIndex { get; }
        public IRecordSource Source { get; protected set; }
        protected INavigator Navigator => Source.Navigate();
        public virtual bool AllowNewRecord
        {
            get => _allowNewRecord;
            set 
            {
                _allowNewRecord = value;
                Navigator.AllowNewRecord = value;
            } 
        }
        public virtual ISQLModel? CurrentModel { get; set; }
        public virtual string Records { get; protected set; } = string.Empty;
        #endregion

        #region Events
        public event AfterRecordNavigationEventHandler? AfterRecordNavigation;
        public event BeforeRecordNavigationEventHandler? BeforeRecordNavigation;
        #endregion

        public AbstractSQLModelController()
        {
            try
            {
                Db = DatabaseManager.Get(DatabaseIndex);
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
            }

            Source = InitSource();
            GoFirst();
        }

        protected virtual IRecordSource InitSource() => new RecordSource(Db, this);
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
        protected virtual bool CanMove() 
        {
            if (CurrentModel != null)
            {
                if (CurrentModel.IsNewRecord()) return true;
                if (!CurrentModel.AllowUpdate()) return false;
            }
            return true;
        }

        public virtual bool GoNext()
        {
            if (!CanMove()) return false;
            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoNext)) return false; //Event was cancelled

            bool moved = Navigator.MoveNext();
            if (!moved)
            {
                Records = Source.RecordPositionDisplayer();
                if (Source.Count == 0)
                    CurrentModel = null;
                return Navigator.EOF ? GoNew() : false;
            }

            CurrentModel = Navigator.Current;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoNext)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoPrevious()
        {
            if (!CanMove()) return false;

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoPrevious)) return false; //Event was cancelled

            bool moved = Navigator.MovePrevious();
            if (!moved) return false;

            try 
            {
                CurrentModel = Navigator.Current;
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

            bool moved = Navigator.MoveLast();

            if (!moved)
            {
                Records = Source.RecordPositionDisplayer();
                if (Source.Count == 0)
                    CurrentModel = null;
                return false;
            }

            CurrentModel = Navigator.Current;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoLast)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoFirst()
        {
            if (!CanMove()) return false;

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoFirst)) return false; //Event was cancelled

            bool moved = Navigator.MoveFirst();

            if (!moved)
            {
                Records = Source.RecordPositionDisplayer();
                if (Source.Count == 0)
                    CurrentModel = null;
                return false;
            }

            CurrentModel = Navigator.Current;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoFirst)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoNew()
        {
            if (!CanMove()) return false;

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoNew)) return false; //Event was cancelled

            bool moved = Navigator.MoveNew();

            if (!moved)
            {
                Records = Source.RecordPositionDisplayer();
                if (Source.Count == 0)
                    CurrentModel = null;
                return false;
            }
            CurrentModel = Navigator.Current;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoNew)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return moved;   
        }

        public virtual bool GoAt(int index)
        {
            if (!CanMove()) return false;

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoAt)) return false; //Event was cancelled

            bool moved = Navigator.MoveAt(index);
            if (!moved) return false;
            CurrentModel = Navigator.Current;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoAt)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoAt(ISQLModel? record)
        {
            if (!CanMove()) return false;
            if (record == null) 
            {
                CurrentModel = null;
                Records = Source.RecordPositionDisplayer();
                return false;
            }

            if (record.IsNewRecord()) return GoNew();

            if (InvokeBeforeRecordNavigationEvent(RecordMovement.GoAt)) return false; //Event was cancelled

            bool moved = Navigator.MoveAt(record);
            if (!moved) return false;

            CurrentModel = record;

            if (InvokeAfterRecordNavigationEvent(RecordMovement.GoAt)) return false; //Event was cancelled

            Records = Source.RecordPositionDisplayer();
            return true;
        }
        #endregion

        #region CRUD Operations
        public void DeleteRecord(string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (CurrentModel == null) throw new NoModelException();
            Db.Model = CurrentModel;
            DeleteOrphan(CurrentModel);
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

        protected abstract void OnUIApplication(IAbstractDatabase? db, ISQLModel record);

        private async void DeleteOrphan(ISQLModel? model)
        {
            if (model == null) return;
            await Task.Run(() =>
            {
                IEnumerable<EntityTree> trees = DatabaseManager.Map.FetchParentsOfNode(model.GetType().Name);
                Parallel.ForEachAsync(trees, async (tree, t) =>
                {
                    IAbstractDatabase? db = DatabaseManager.Find(tree.Name);
                    IEnumerable<ISQLModel>? toRemove = db?.MasterSource.Where(s => FetchToRemove(s, model)).ToList();
                    if (toRemove == null) return;
                    foreach (ISQLModel record in toRemove)
                    {
                        await Task.Run(()=>DeleteOrphan(record));
                        record.InvokeBeforeRecordDelete();
                        db?.MasterSource.Remove(record);
                        await Task.Delay(1);
                        try 
                        {
                            db?.MasterSource?.NotifyChildren(CRUD.DELETE, record);
                        }
                        catch 
                        {
                            OnUIApplication(db, record);
                        }
                    }
                });

                //foreach (EntityTree tree in trees)
                //{
                //    IAbstractDatabase? db = DatabaseManager.Find(tree.Name);
                //    IEnumerable<ISQLModel>? toRemove = db?.MasterSource.Where(s => AbstractFormController<M>.FetchToRemove(s, model)).ToList();
                //    if (toRemove == null) continue;
                //    foreach (ISQLModel record in toRemove)
                //    {
                //        DeleteOrphan(record);
                //        record.InvokeBeforeRecordDelete();
                //        db?.MasterSource.Remove(record);
                //        Application.Current.Dispatcher.Invoke(() => 
                //        {
                //            db?.MasterSource?.NotifyChildren(CRUD.DELETE, record);
                //        });
                //    }
                //}
            });
        }

        private static bool FetchToRemove(ISQLModel model, ISQLModel? mod)
        {
            string? propName = mod?.GetTableName();
            if (string.IsNullOrEmpty(propName)) return false;
            object? obj = model.GetPropertyValue(propName);
            if (obj == null) return false;
            bool res = obj.Equals(mod);
            return res;
        }
        public virtual bool AlterRecord(string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (CurrentModel == null) throw new NoModelException();
            if (!CurrentModel.AllowUpdate()) return false; //cannot update.
            Db.Model = CurrentModel;
            CRUD crud = (!Db.Model.IsNewRecord()) ? CRUD.UPDATE : CRUD.INSERT;
            Db.Crud(crud, sql, parameters);
            Db.MasterSource?.NotifyChildren(crud, Db.Model);
            GoAt(CurrentModel);
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