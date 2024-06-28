using Backend.Model;
using System.Collections;

namespace Backend.Source
{
    /// <summary>
    /// Concrete implementation of the <see cref="INavigator"/> Interface.
    /// </summary>
    public class Navigator<M> : INavigator<M> where M : ISQLModel, new()
    {
        protected M[] _records;
        private int _index = -1;
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

        public Navigator(IEnumerable<M> source)
        {
            _records = source.ToArray();
            if (IsEmpty) Index = -1;
        }

        public Navigator(IEnumerable<M> source, int index, bool allowNewRecord) : this(source) 
        {
            Index = index;
            AllowNewRecord = allowNewRecord;
        }

        public bool MoveNext()
        {
            _index++;
            return _index < _records.Length;
        }

        public M Current
        {
            get
            {
                if (Index > _index) _index = Index;
                if (_index >= 0 && _index < _records.Length)
                    return _records[_index];
                
                throw new InvalidOperationException($"{typeof(M).Name}'s Invalid state: New Record: {IsNewRecord}; IsEmpty: {IsEmpty}; Index: {_index}; Record Count: {RecordCount}");
            }
        }

        object? IEnumerator.Current
        { 
            get
            {
                try
                {
                    return Current;
                }
                catch 
                {
                    return null;
                }
            }
        }

        public void Dispose()
        {
            _records = [];
            GC.SuppressFinalize(this);
        }

        public bool GoNext()
        {
            Index = IsEmpty ? -1 : ++Index;
            bool result = Index <= LastIndex;

            if (result)
                _index = Index;
            return result;
        }

        public bool GoPrevious()
        {
            Index = IsEmpty ? -1 : --Index;
            bool result = Index > -1;
            if (result)
                _index = Index;
            return result;
        }
        public bool GoFirst()
        {
            Index = (IsEmpty) ? -1 : 0;
            bool result = RecordCount > 0;
            if (result)
                _index = Index;
            return result;
        }
        public bool GoLast()
        {
            Index = (IsEmpty) ? -1 : LastIndex;
            bool result = RecordCount > 0;
            if (result)
                _index = Index;
            return result;
        }
        public bool GoNew()
        {
            if (!AllowNewRecord || IsNewRecord) return false;
            Index = RecordCount;
            return true;
        }

        public bool GoAt(int index)
        {
            Index = (IsEmpty) ? -1 : index;
            bool result = Index <= LastIndex;
            if (result)
                _index = Index;
            return result;
        }

        public bool GoAt(object record)
        {
            for (int i = 0; i < RecordCount; i++)
            {
                if (_records[i].Equals(record))
                {
                    Index = i;
                    _index = Index;
                    return true;
                }
            }
            return false;
        }

        public void Reset() => Index = 0;
        public override string? ToString() => $"Count: {RecordCount}; Record Number: {RecNum}; BOF: {BOF}; EOF: {EOF}; NewRecord: {IsNewRecord}; No Records: {NoRecords}";

    }

}
