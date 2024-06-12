using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Enums
{
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

}
