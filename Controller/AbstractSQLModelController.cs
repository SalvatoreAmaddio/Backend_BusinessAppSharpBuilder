using Backend.Database;
using Backend.Exceptions;
using Backend.Model;
using Backend.Source;
using Backend.Enums;
using Backend.Events;

namespace Backend.Controller
{
    /// <summary>
    /// Provides an abstract implementation of the <see cref="IAbstractSQLModelController"/> interface, 
    /// handling CRUD operations, record navigation, and event management for SQL models. 
    /// This class serves as a base for controllers that manage SQL-based data sources.
    /// </summary>
    /// <typeparam name="M">The type of the SQL model, which must implement <see cref="ISQLModel"/> and have a parameterless constructor.</typeparam>
    public abstract class AbstractSQLModelController<M> : IAbstractSQLModelController where M : ISQLModel, new()
    {
        #region Variables
        protected bool _allowNewRecord = true;
        #endregion

        #region Properties
        public IAbstractDatabase Db { get; protected set; } = null!;
        public IDataSource Source => DataSource;
        public virtual string Records { get; protected set; } = string.Empty;
        public bool EOF => Navigator.EOF;

        /// <summary>
        /// Gets the <see cref="Source"/> property as a <see cref="IDataSource{M}"/> object.
        /// </summary>
        public IDataSource<M> DataSource { get; protected set; }

        /// <summary>
        /// Gets a reference to the <see cref="DataSource"/>'s <see cref="Navigator"/> object.
        /// </summary>
        protected INavigator<M> Navigator => DataSource.Navigate();

        /// <summary>
        /// Gets or sets a value indicating whether a new record can be added. Default value is true.
        /// This property also sets the <see cref="Navigator.AllowNewRecord"/> property.
        /// </summary>
        /// <value>true if the <see cref="Navigator"/> can add new records; otherwise, false.</value>
        public virtual bool AllowNewRecord
        {
            get => _allowNewRecord;
            set
            {
                _allowNewRecord = value;
                Navigator.AllowNewRecord = value;
            }
        }

        /// <summary>
        /// Gets or sets the record on which the <see cref="Navigator"/> is currently pointing.
        /// </summary>
        public virtual M? CurrentRecord { get; set; }
        #endregion

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

        #region DataSource
        /// <summary>
        /// Initializes the <see cref="IDataSource{M}"/> property. This method is called in the constructor.
        /// Override this method to set a custom data source.
        /// </summary>
        /// <returns>A <see cref="IDataSource{M}"/> object.</returns>
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
        #endregion

        #region Current Record
        public void SetCurrentRecord(ISQLModel? model) => CurrentRecord = (M?)model;
        public ISQLModel? GetCurrentRecord() => CurrentRecord;
        #endregion

        #region GoTo
        /// <summary>
        /// Checks if the <see cref="CurrentRecord"/> meets the conditions to be updated.
        /// This method is called whenever the <see cref="Navigator"/> moves.
        /// </summary>
        /// <returns>True if the Navigator can move; otherwise, false.</returns>
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

            CurrentRecord = Navigator.CurrentRecord;

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
                CurrentRecord = Navigator.CurrentRecord;
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

            CurrentRecord = Navigator.CurrentRecord;

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

            CurrentRecord = Navigator.CurrentRecord;

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
            CurrentRecord = Navigator.CurrentRecord;

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

        /// <summary>
        /// Deletes all records associated by foreign key constraints to the deleted record.
        /// This method is called inside <see cref="DeleteRecord(string?, List{QueryParameter}?)"/>.
        /// </summary>
        /// <param name="model">The model representing the record to delete.</param>
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

        /// <summary>
        /// Handles operations performed in the <see cref="DeleteOrphan(ISQLModel?)"/> method on the UI thread.
        /// </summary>
        /// <param name="tree">The entity tree.</param>
        /// <param name="record">The record being processed.</param>
        protected abstract void OnUIApplication(EntityTree tree, ISQLModel record);
        #endregion

        #region Event Invokers
        /// <summary>
        /// Invokes the <see cref="AfterRecordNavigation"/> event.
        /// </summary>
        /// <param name="recordMovement">The type of record movement.</param>
        /// <returns>True if the event was cancelled; otherwise, false.</returns>
        protected bool InvokeAfterRecordNavigationEvent(RecordMovement recordMovement)
        {
            AllowRecordMovementArgs args = new(recordMovement);
            AfterRecordNavigation?.Invoke(this, args);
            return args.Cancel; // if returns true, the event is cancelled
        }

        /// <summary>
        /// Invokes the <see cref="BeforeRecordNavigation"/> event.
        /// </summary>
        /// <param name="recordMovement">The type of record movement.</param>
        /// <returns>True if the event was cancelled; otherwise, false.</returns>
        protected bool InvokeBeforeRecordNavigationEvent(RecordMovement recordMovement)
        {
            AllowRecordMovementArgs args = new(recordMovement);
            BeforeRecordNavigation?.Invoke(this, args);
            return args.Cancel; // if returns true, the event is cancelled
        }
        #endregion

        #region Disposer
        /// <summary>
        /// Unsubscribes the controller from events. Override this method to add additional disposal logic.
        /// This method is called in <see cref="Dispose"/>.
        /// </summary>
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