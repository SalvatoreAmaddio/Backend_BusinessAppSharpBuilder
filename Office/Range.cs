using Backend.Exceptions;
using Microsoft.Office.Interop.Excel;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using XL = Microsoft.Office.Interop.Excel;

namespace Backend.Office
{
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

    public class Range : IDestroyable
    {
        XL.Range rng;

        /// <summary>
        /// Instantiates a Range object which selects the entire column based on column's letter. For Example:
        /// <code>
        ///     Range range = new("A"); //gets the entire column A.
        /// </code>
        /// </summary>
        public Range(_Worksheet wrksheet, string columnLetter) => rng = wrksheet.Columns["A"];

        /// <summary>
        /// Instantiates a Range object which selects the entire column based on column's index. For Example:
        /// <code>
        ///     Range range = new(1); //gets the entire column A.
        /// </code>
        /// </summary>
        public Range(_Worksheet wrksheet, int columnIndex = 1) => rng = wrksheet.Columns[ConvertIndexToColumnLetter(columnIndex)];

        /// <summary>
        /// Instantiates a Range object based on cells' names. For Example:
        /// <code>
        ///     Range range = new("A1", "H1"); //gets the Range from A1 to H1.
        /// </code>
        /// </summary>
        public Range(_Worksheet wrksheet, string cell1, string cell2) => rng = wrksheet.get_Range(cell1, cell2);

        /// <summary>
        /// Instantiates a Range object based on indexes. For Example:
        /// <code>
        ///     Range range = new(1, 1, 2, 1); //gets the Range from A1 to B1.
        /// </code>
        /// This constructor uses <see cref="ConvertIndexToColumnLetter(int)"/> to convert column's indexes to column's letters.
        /// </summary>
        /// <exception cref="ExcelIndexException"></exception>
        public Range(_Worksheet wrksheet, int col1 = 1, int row1=1, int col2 = 1, int row2=1)
        {
            if (row1 <= 0 || col1 <= 0 || col2 <= 0 || row2 <= 0) throw new ExcelIndexException();
            string cell1 = ConvertIndexToColumnLetter(col1) + row1.ToString();
            string cell2 = ConvertIndexToColumnLetter(col2) + row2.ToString();
            rng = wrksheet.get_Range(cell1, cell2);
        }

        /// <summary>
        /// Converts the number provided to the corrisponding Column's Letter. For instance, 1 will return "A" and so on.
        /// This method is called in <see cref="Range(_Worksheet, int, int, int, int)"/> constructor.
        /// </summary>
        /// <param name="columnNumber"></param>
        /// <returns>A string representing the Column Letter.</returns>
        /// <exception cref="ExcelIndexException"></exception>
        public static string ConvertIndexToColumnLetter(int columnNumber)
        {
            if (columnNumber <=0) throw new ExcelIndexException();
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

        public void Formula(string formula) => rng.Formula = formula;
        public void Bold(bool value) => rng.Font.Bold = value;
        public void Italic(bool value) => rng.Font.Italic = value;
        public void Underline() => rng.Font.Underline = XlUnderlineStyle.xlUnderlineStyleSingle;
        public void SetColor(Color color) => rng.Font.Color = ColorTranslator.ToOle(color);
        public void SetBackground(Color color) => rng.Font.Background = ColorTranslator.ToOle(color);
        public void HorizontalAlignment(XlAlign align) => rng.HorizontalAlignment = align;
        public void VerticalAlignment(XlAlign align) => rng.VerticalAlignment = align;

        public void ColumnWidth(double width) => rng.ColumnWidth = width;
        public void ApplyFilters() => rng.AutoFilter(1, Missing.Value, XlAutoFilterOperator.xlAnd, Missing.Value, true);
        
        public void WrapText(bool value) => rng.WrapText = value;        

        /// <summary>
        /// Returns a <see cref="XL.Range"/> to iterate through each cell in Range.
        /// </summary>
        /// <returns></returns>
        public XL.Range Cells() => rng.Cells;

        public void Destroy() => Marshal.ReleaseComObject(rng);
    }
}
