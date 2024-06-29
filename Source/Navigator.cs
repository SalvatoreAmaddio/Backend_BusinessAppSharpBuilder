using Backend.Model;
using System.Collections;

namespace Backend.Source
{
    /// <summary>
    /// Concrete implementation of the <see cref="INavigator{M}"/> interface.
    /// Provides functionality for navigating through an array of objects implementing <see cref="ISQLModel"/>.
    /// </summary>
    /// <typeparam name="M">A type that implements <see cref="ISQLModel"/> and has a parameterless constructor.</typeparam>
    public class Navigator<M> : INavigator<M> where M : ISQLModel, new()
    {
        protected M[] _records;
        private int _position = -1;

        #region Properties
        public int Index { get; set; } = -1;
        public int RecordCount => _records.Length;
        public bool IsNewRecord => Index > LastIndex;
        public bool IsEmpty => RecordCount == 0;
        public bool BOF => Index == 0;
        public bool EOF => Index == LastIndex;
        public bool NoRecords => !IsNewRecord && IsEmpty;
        public bool AllowNewRecord { get; set; } = true;
        public int RecNum => Index + 1;
        public M Current
        {
            get
            {
                if (Index > _position) _position = Index;
                if (_position >= 0 && _position < _records.Length)
                    return _records[_position];

                throw new InvalidOperationException($"{typeof(M).Name}'s Invalid state: New Record: {IsNewRecord}; IsEmpty: {IsEmpty}; Index: {_position}; Record Count: {RecordCount}");
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

        /// <summary>
        /// Gets the last zero-based position of the array.
        /// </summary>
        /// <value>An integer representing the last position in the array.</value>
        protected int LastIndex => IsEmpty ? -1 : RecordCount - 1;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Navigator{M}"/> class with the specified source.
        /// </summary>
        /// <param name="source">The source collection of records.</param>
        public Navigator(IEnumerable<M> source)
        {
            _records = source.ToArray();
            if (IsEmpty) Index = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Navigator{M}"/> class with the specified source, index, and allowNewRecord flag.
        /// </summary>
        /// <param name="source">The source collection of records.</param>
        /// <param name="index">The initial index position.</param>
        /// <param name="allowNewRecord">A value indicating whether new records are allowed.</param>
        public Navigator(IEnumerable<M> source, int index, bool allowNewRecord) : this(source)
        {
            Index = index;
            AllowNewRecord = allowNewRecord;
        }
        #endregion

        public bool MoveNext()
        {
            _position++;
            return _position < _records.Length;
        }

        #region GoAt
        public bool GoNext()
        {
            Index = IsEmpty ? -1 : ++Index;
            bool result = Index <= LastIndex;

            if (result)
                _position = Index;
            return result;
        }

        public bool GoPrevious()
        {
            Index = IsEmpty ? -1 : --Index;
            bool result = Index > -1;
            if (result)
                _position = Index;
            return result;
        }

        public bool GoFirst()
        {
            Index = IsEmpty ? -1 : 0;
            bool result = RecordCount > 0;
            if (result)
                _position = Index;
            return result;
        }

        public bool GoLast()
        {
            Index = IsEmpty ? -1 : LastIndex;
            bool result = RecordCount > 0;
            if (result)
                _position = Index;
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
            Index = IsEmpty ? -1 : index;
            bool result = Index <= LastIndex;
            if (result)
                _position = Index;
            return result;
        }

        public bool GoAt(object record)
        {
            for (int i = 0; i < RecordCount; i++)
            {
                if (_records[i].Equals(record))
                {
                    Index = i;
                    _position = Index;
                    return true;
                }
            }
            return false;
        }
        #endregion

        public void Reset() => Index = 0;

        /// <summary>
        /// Releases all resources used by the <see cref="Navigator{M}"/>.
        /// </summary>
        public void Dispose()
        {
            _records = Array.Empty<M>();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string? ToString() => $"Count: {RecordCount}; Record Number: {RecNum}; BOF: {BOF}; EOF: {EOF}; NewRecord: {IsNewRecord}; No Records: {NoRecords}";
    }
}