namespace Backend.Enums
{
    public enum RecordMovement
    {
        GoFirst = 1,
        GoLast = 2,
        GoNext = 3,
        GoPrevious = 4,
        GoNew = 5,
        GoAt = 6,
    }

    /// <summary>
    /// FieldType Enum
    /// </summary>
    public enum FieldType
    {
        PK = 0,
        Field = 1,
        FK = 2
    }

    public enum CRUD
    {
        None = -1,
        INSERT = 0,
        UPDATE = 1,
        DELETE = 2,
    }

    public enum XlAlign
    {
        Center = -4108,
        CenterAcrossSelection = 7,
        Distributed = -4117,
        Fill = 5,
        General = 1,
        Justify = -4130,
        Left = -4131,
        Right = -4152
    }
}