using Backend.Database;
using Backend.Enums;
using Backend.Exceptions;
using Backend.Model;
using System.Runtime.InteropServices;
using XL = Microsoft.Office.Interop.Excel;

namespace Backend.Office
{
    /// <summary>
    /// Provides methods to easily access and manage Excel worksheets using COM objects.
    /// </summary>
    public class Worksheet : IDestroyable
    {
        XL._Worksheet wrksheet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worksheet"/> class.
        /// </summary>
        /// <param name="wrksheet">The COM object representing the worksheet.</param>
        public Worksheet(XL._Worksheet wrksheet) => this.wrksheet = wrksheet;

        /// <summary>
        /// Sets the name of the worksheet.
        /// </summary>
        /// <param name="name">The name to set for the worksheet.</param>
        public void SetName(string name) => this.wrksheet.Name = name;

        /// <summary>
        /// Prints a header with a default style. By default, the header will be at the first row.
        /// </summary>
        /// <param name="headers">The headers to print.</param>
        /// <param name="row">The row index where the header will be printed.</param>
        public void PrintHeader(IEnumerable<string> headers, int row = 1) => PrintHeader(headers.ToArray(), row);

        /// <summary>
        /// Prints a header with a default style. By default, the header will be at the first row.
        /// </summary>
        /// <param name="headers">The headers to print.</param>
        /// <param name="row">The row index where the header will be printed.</param>
        public void PrintHeader(string[] headers, int row = 1)
        {
            int column = 1;
            foreach (string header in headers)
            {
                SetValue(header, row, column);
                column++;
            }

            Range range = GetRange(1, row, headers.Length, row);
            range.HorizontalAlignment(XlAlign.Center);
            range.VerticalAlignment(XlAlign.Center);
            range.Bold(true);
            range.ApplyFilters();
            range.ColumnWidth(16);
            range.Destroy();
        }

        /// <summary>
        /// Prints a collection of records in a tabular format. By default, the print starts at the second row.
        /// </summary>
        /// <param name="records">The records to print.</param>
        /// <param name="use_pk_for_fk">Set to true if ForeignKey's objects' PK should be printed.</param>
        /// <param name="row">The row index to start printing from (default is 2).</param>
        public void PrintData(IEnumerable<ISQLModel> records, bool use_pk_for_fk = false, int row = 2)
        {
            if (!records.Any()) return;
            if (row <= 0) throw new ExcelIndexException();
            int initialRow = row;
            int totalColumns = 0;
            int column = 1;
            foreach (ISQLModel record in records)
            {
                foreach (ITableField tableField in record.GetEntityFields())
                {
                    if (tableField is FKField fk)
                    {
                        object? value;
                        if (!use_pk_for_fk)
                            value = DatabaseManager.Find(fk.ClassName)?.MasterSource.FirstOrDefault(s => s.Equals(fk?.GetValue()));
                        else
                            value = fk?.PK?.GetValue();

                        SetValue(value, row, column);
                    }
                    else
                        SetValue(tableField?.GetValue(), row, column);
                    column++;
                }
                totalColumns = column;
                column = 1;
                row++;
            }

            Range range = GetRange(1, initialRow, totalColumns, row);
            range.WrapText(false);
            range.Destroy();
        }

        /// <summary>
        /// Deletes the worksheet.
        /// </summary>
        public void Delete() => wrksheet.Delete();

        /// <summary>
        /// Sets the value of a given cell. The cell is represented by the row and column index. 
        /// For example:
        /// <code>
        ///   excel.Worksheet?.SetValue("ciao", 1, 1); // Prints the word 'ciao' in cell A1.
        /// </code>
        /// </summary>
        /// <param name="value">The value to print.</param>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="col">The column index of the cell.</param>
        /// <exception cref="ExcelIndexException">Thrown when the row or column index is less than or equal to zero.</exception>
        public void SetValue(object? value, int row = 1, int col = 1)
        {
            if (row <= 0 || col <= 0) throw new ExcelIndexException();
            this.wrksheet.Cells[row, col] = (value == null) ? string.Empty : value.ToString();
        }

        /// <summary>
        /// Sets the value of a given cell. The cell is represented by the row and column label. 
        /// For example:
        /// <code>
        ///   excel.Worksheet?.SetValue("ciao", 1, "A"); // Prints the word 'ciao' in cell A1.
        /// </code>
        /// </summary>
        /// <param name="value">The value to print.</param>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="col">The column label of the cell.</param>
        /// <exception cref="ExcelIndexException">Thrown when the row index is less than or equal to zero.</exception>
        public void SetValue(object? value, int row = 1, string col = "A")
        {
            if (row <= 0) throw new ExcelIndexException();
            this.wrksheet.Cells[row, col] = (value == null) ? string.Empty : value;
        }

        /// <summary>
        /// Gets a range based on indexes. For example:
        /// <code>
        ///     Range range = GetRange(1, 1, 2, 1); // Gets the range from A1 to B1.
        /// </code>
        /// </summary>
        /// <param name="col1">The starting column index.</param>
        /// <param name="row1">The starting row index.</param>
        /// <param name="col2">The ending column index.</param>
        /// <param name="row2">The ending row index.</param>
        /// <returns>A <see cref="Range"/> representing the specified range of cells.</returns>
        /// <exception cref="ExcelIndexException">Thrown when any of the provided indexes are less than or equal to zero.</exception>
        public Range GetRange(int col1 = 1, int row1 = 1, int col2 = 1, int row2 = 1)
        {
            if (row1 <= 0 || col1 <= 0 || col2 <= 0 || row2 <= 0) throw new ExcelIndexException();
            return new Range(wrksheet, col1, row1, col2, row2);
        }

        /// <summary>
        /// Gets a range based on cell names. For example:
        /// <code>
        ///     Range range = GetRange("A1", "H1"); // Gets the range from A1 to H1.
        /// </code>
        /// </summary>
        /// <param name="cell1">The starting cell name.</param>
        /// <param name="cell2">The ending cell name.</param>
        /// <returns>A <see cref="Range"/> representing the specified range of cells.</returns>
        public Range GetRange(string cell1, string cell2) => new Range(wrksheet, cell1, cell2);

        /// <summary>
        /// Gets a range object which selects the entire column based on the column's letter. For example:
        /// <code>
        ///     Range range = GetRange("A"); // Gets the entire column A.
        /// </code>
        /// </summary>
        /// <param name="columnLetter">The letter of the column to select.</param>
        /// <returns>A <see cref="Range"/> representing the specified column.</returns>
        public Range GetRange(string columnLetter) => new Range(wrksheet, columnLetter);

        /// <summary>
        /// Gets a range object which selects the entire column based on the column's index. For example:
        /// <code>
        ///     Range range = GetRange(1); // Gets the entire column A.
        /// </code>
        /// </summary>
        /// <param name="columnIndex">The index of the column to select.</param>
        /// <returns>A <see cref="Range"/> representing the specified column.</returns>
        public Range GetRange(int columnIndex) => new Range(wrksheet, columnIndex);

        /// <summary>
        /// Releases the COM object for the worksheet.
        /// </summary>
        public void Destroy() => Marshal.ReleaseComObject(this.wrksheet);
    }
}
