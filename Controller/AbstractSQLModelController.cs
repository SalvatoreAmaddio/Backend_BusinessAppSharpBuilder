using Backend.Database;
using Backend.Exceptions;
using Backend.Model;
using Backend.Source;

namespace Backend.Controller
{
    public abstract class AbstractSQLModelController : IAbstractSQLModelController, IDisposable
    {
        protected bool _disposed = false;
        protected bool _allowNewRecord = true;
        public IAbstractDatabase Db { get; protected set; } = null!;
        public abstract int DatabaseIndex { get; }
        public IRecordSource Source { get; protected set; }
        protected INavigator Navigator => Source.Navigate();
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
        public ICollection<ISQLModel> SourceAsCollection() => (ICollection<ISQLModel>)Source;
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
            bool moved = Navigator.MoveNext();
            if (!moved) 
            {
                return Navigator.EOF ? GoNew() : false;                
            }
            CurrentModel = Navigator.Current;
            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoPrevious()
        {
            if (!CanMove()) return false;
            bool moved = Navigator.MovePrevious();
            if (!moved) return false;
            CurrentModel = Navigator.Current;
            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoLast()
        {
            if (!CanMove()) return false;
            bool moved = Navigator.MoveLast();
            if (!moved) return false;
            CurrentModel = Navigator.Current;
            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoFirst()
        {
            if (!CanMove()) return false;
            bool moved = Navigator.MoveFirst();
            if (!moved) 
            {
                Records = Source.RecordPositionDisplayer();
                return false;
            }
            CurrentModel = Navigator.Current;
            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public virtual bool GoNew()
        {
            if (!CanMove()) return false;
            bool moved = Navigator.MoveNew();
            if (!moved) return false;
            CurrentModel = Navigator.Current;
            Records = Source.RecordPositionDisplayer();
            return moved;   
        }

        public virtual bool GoAt(int index)
        {
            if (!CanMove()) return false;
            bool moved = Navigator.MoveAt(index);
            if (!moved) return false;
            CurrentModel = Navigator.Current;
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
            bool moved = Navigator.MoveAt(record);
            CurrentModel = record;
            Records = Source.RecordPositionDisplayer();
            return true;
        }

        public void DeleteRecord(string? sql = null, List<QueryParameter>? parameters = null)
        {
            if (CurrentModel == null) throw new NoModelException();
            Db.Model = CurrentModel;
            Db.Crud(CRUD.DELETE, sql, parameters);
            if (Db.Model.IsNewRecord()) //this occurs in ListView objects when you add a new record but then decided to delete it.
            {
                SourceAsCollection().Remove(Db.Model); //remove the record from the Source, thereof from the ListView
                if (Navigator.BOF && !Navigator.NoRecords) GoFirst(); //The record is deleted, handle the direction that the Navigator object should point at.
                else GoPrevious(); //if we still have records move back.
            }
            else
                Db?.MasterSource?.NotifyChildren(CRUD.DELETE, Db.Model); //notify children sources that the master source has changed.
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}