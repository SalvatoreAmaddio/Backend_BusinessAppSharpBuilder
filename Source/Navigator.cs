using Backend.Model;
using System.Collections;

namespace Backend.Source
{
    /// <summary>
    /// Concrete implementation of the <see cref="INavigator"/> Interface.
    /// </summary>
    public class Navigator : INavigator
    {
        protected ISQLModel[] _records;
        public int Index { get; set; } = -1;
        public int RecordCount => _records.Length;
        public bool IsNewRecord => Index > LastIndex;
        public bool IsEmpty => RecordCount == 0;
        public bool BOF => Index == 0;
        public bool EOF => Index == LastIndex;
        public bool NoRecords => !IsNewRecord && IsEmpty;
        public bool AllowNewRecord { get; set; } = true;
        public int RecNum => Index + 1;

        /// <summary>
        /// The last zero-based position of the array.
        /// </summary>
        /// <value>An integer telling which is the last position in the array.</value>
        protected int LastIndex => (IsEmpty) ? -1 : RecordCount - 1;
        object? IEnumerator.Current => Current;

        public Navigator(IEnumerable<ISQLModel> source)
        {
            _records = source.ToArray();
            if (IsEmpty) Index = -1;
        }

        public Navigator(IEnumerable<ISQLModel> source, int index, bool allowNewRecord) : this(source) 
        {
            Index = index;
            AllowNewRecord = allowNewRecord;
        }

        public ISQLModel Current
        {
            get 
            {
               if (Index >= 0 && Index < _records.Length)
                        return _records[Index];
                
                throw new InvalidOperationException($"Invalid state: New Record: {IsNewRecord}; IsEmpty: {IsEmpty}; Index: {Index}");
            }
        }

        public void Dispose()
        {
            _records = [];
            GC.SuppressFinalize(this);
        }
        public bool MoveNext()
        {
            Index = IsEmpty ? -1 : ++Index;
            return Index <= LastIndex;
        }
        public bool MovePrevious()
        {
            Index = IsEmpty ? -1 : --Index;
            return Index > -1;
        }
        public bool MoveFirst()
        {
            Index = (IsEmpty) ? -1 : 0;
            return RecordCount > 0;
        }
        public bool MoveLast()
        {
            Index = (IsEmpty) ? -1 : LastIndex;
            return RecordCount > 0;
        }
        public bool MoveNew()
        {
            if (!AllowNewRecord || IsNewRecord) return false;
            Index = RecordCount;
            return true;
        }
        public bool MoveAt(int index)
        {
            Index = (IsEmpty) ? -1 : index;
            return Index <= LastIndex;
        }

        public bool MoveAt(object record)
        {
            for (int i = 0; i < RecordCount; i++)
            {
                if (_records[i].Equals(record))
                {
                    Index = i;
                    return true;
                }
            }
            return false;
        }

        public void Reset() => Index = 0;
        public override string? ToString() => $"Count: {RecordCount}; Record Number: {RecNum}; BOF: {BOF}; EOF: {EOF}; NewRecord: {IsNewRecord}; No Records: {NoRecords}";

    }

}
