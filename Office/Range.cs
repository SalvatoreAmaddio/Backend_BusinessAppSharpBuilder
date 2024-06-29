using Backend.Exceptions;
using Microsoft.Office.Interop.Excel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using XL = Microsoft.Office.Interop.Excel;
using Backend.Enums;

namespace Backend.Office
{
    /// <summary>
    /// Represents a range of cells in an Excel worksheet and provides methods to manipulate the range.
    /// </summary>
    public class Range : IDestroyable
    {
        XL.Range rng;

        /// <summary>
        /// Instantiates a Range object which selects the entire column based on the column's letter.
        /// For example:
        /// <code>
        ///     Range range = new Range(worksheet, "A"); // Gets the entire column A.
        /// </code>
        /// </summary>
        /// <param name="wrksheet">The worksheet containing the column.</param>
        /// <param name="columnLetter">The letter of the column to select.</param>
        public Range(_Worksheet wrksheet, string columnLetter) => rng = wrksheet.Columns["A"];

        /// <summary>
        /// Instantiates a Range object which selects the entire column based on the column's index.
        /// For example:
        /// <code>
        ///     Range range = new Range(worksheet, 1); // Gets the entire column A.
        /// </code>
        /// </summary>
        /// <param name="wrksheet">The worksheet containing the column.</param>
        /// <param name="columnIndex">The index of the column to select (default is 1).</param>
        public Range(_Worksheet wrksheet, int columnIndex = 1) => rng = wrksheet.Columns[ConvertIndexToColumnLetter(columnIndex)];

        /// <summary>
        /// Instantiates a Range object based on cells' names.
        /// For example:
        /// <code>
        ///     Range range = new Range(worksheet, "A1", "H1"); // Gets the range from A1 to H1.
        /// </code>
        /// </summary>
        /// <param name="wrksheet">The worksheet containing the range.</param>
        /// <param name="cell1">The starting cell of the range.</param>
        /// <param name="cell2">The ending cell of the range.</param>
        public Range(_Worksheet wrksheet, string cell1, string cell2) => rng = wrksheet.get_Range(cell1, cell2);

        /// <summary>
        /// Instantiates a Range object based on indexes.
        /// For example:
        /// <code>
        ///     Range range = new Range(worksheet, 1, 1, 2, 1); // Gets the range from A1 to B1.
        /// </code>
        /// This constructor uses <see cref="ConvertIndexToColumnLetter(int)"/> to convert column indexes to column letters.
        /// </summary>
        /// <param name="wrksheet">The worksheet containing the range.</param>
        /// <param name="col1">The column index of the starting cell (default is 1).</param>
        /// <param name="row1">The row index of the starting cell (default is 1).</param>
        /// <param name="col2">The column index of the ending cell (default is 1).</param>
        /// <param name="row2">The row index of the ending cell (default is 1).</param>
        /// <exception cref="ExcelIndexException">Thrown when any of the provided indexes are less than or equal to zero.</exception>
        public Range(_Worksheet wrksheet, int col1 = 1, int row1 = 1, int col2 = 1, int row2 = 1)
        {
            if (row1 <= 0 || col1 <= 0 || col2 <= 0 || row2 <= 0) throw new ExcelIndexException();
            string cell1 = ConvertIndexToColumnLetter(col1) + row1.ToString();
            string cell2 = ConvertIndexToColumnLetter(col2) + row2.ToString();
            rng = wrksheet.get_Range(cell1, cell2);
        }

        /// <summary>
        /// Converts the number provided to the corresponding column letter.
        /// For instance, 1 will return "A" and so on.
        /// This method is called in <see cref="Range(_Worksheet, int, int, int, int)"/> constructor.
        /// </summary>
        /// <param name="columnNumber">The column number to convert.</param>
        /// <returns>A string representing the column letter.</returns>
        /// <exception cref="ExcelIndexException">Thrown when the column number is less than or equal to zero.</exception>
        public static string ConvertIndexToColumnLetter(int columnNumber)
        {
            if (columnNumber <= 0) throw new ExcelIndexException();
            int dividend = columnNumber;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        /// <summary>
        /// Sets the formula for the range.
        /// </summary>
        /// <param name="formula">The formula to set.</param>
        public void Formula(string formula) => rng.Formula = formula;

        /// <summary>
        /// Sets the bold attribute for the range.
        /// </summary>
        /// <param name="value">True to set the text to bold, false otherwise.</param>
        public void Bold(bool value) => rng.Font.Bold = value;

        /// <summary>
        /// Sets the italic attribute for the range.
        /// </summary>
        /// <param name="value">True to set the text to italic, false otherwise.</param>
        public void Italic(bool value) => rng.Font.Italic = value;

        /// <summary>
        /// Underlines the text in the range.
        /// </summary>
        public void Underline() => rng.Font.Underline = XlUnderlineStyle.xlUnderlineStyleSingle;

        /// <summary>
        /// Sets the font color for the range.
        /// </summary>
        /// <param name="color">The color to set.</param>
        public void SetColor(Color color) => rng.Font.Color = ColorTranslator.ToOle(color);

        /// <summary>
        /// Sets the background color for the range.
        /// </summary>
        /// <param name="color">The background color to set.</param>
        public void SetBackground(Color color) => rng.Font.Background = ColorTranslator.ToOle(color);

        /// <summary>
        /// Sets the horizontal alignment for the range.
        /// </summary>
        /// <param name="align">The horizontal alignment to set.</param>
        public void HorizontalAlignment(XlAlign align) => rng.HorizontalAlignment = align;

        /// <summary>
        /// Sets the vertical alignment for the range.
        /// </summary>
        /// <param name="align">The vertical alignment to set.</param>
        public void VerticalAlignment(XlAlign align) => rng.VerticalAlignment = align;

        /// <summary>
        /// Sets the column width for the range.
        /// </summary>
        /// <param name="width">The width to set.</param>
        public void ColumnWidth(double width) => rng.ColumnWidth = width;

        /// <summary>
        /// Applies filters to the range.
        /// </summary>
        public void ApplyFilters() => rng.AutoFilter(1, Missing.Value, XlAutoFilterOperator.xlAnd, Missing.Value, true);

        /// <summary>
        /// Sets the wrap text attribute for the range.
        /// </summary>
        /// <param name="value">True to wrap text, false otherwise.</param>
        public void WrapText(bool value) => rng.WrapText = value;

        /// <summary>
        /// Returns a <see cref="XL.Range"/> to iterate through each cell in Range.
        /// </summary>
        /// <returns>A <see cref="XL.Range"/> representing the cells in the range.</returns>
        public XL.Range Cells() => rng.Cells;

        /// <summary>
        /// Releases the COM object for the range.
        /// </summary>
        public void Destroy() => Marshal.ReleaseComObject(rng);
    }
}
