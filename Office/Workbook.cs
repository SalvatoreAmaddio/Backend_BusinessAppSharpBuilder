using Backend.Exceptions;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using XL = Microsoft.Office.Interop.Excel;

namespace Backend.Office
{
    /// <summary>
    /// Provides methods to easily access and manage Excel workbooks using COM objects.
    /// </summary>
    public class Workbook : IDestroyable
    {
        XL.Workbook wrkbk;

        /// <summary>
        /// Gets the currently active worksheet.
        /// </summary>
        public Worksheet ActiveWorksheet { get; private set; }

        /// <summary>
        /// Collection of all worksheets in the workbook.
        /// </summary>
        public readonly List<Worksheet> Sheets = new List<Worksheet>();

        /// <summary>
        /// Gets the total number of worksheets in this workbook.
        /// </summary>
        public int Count => Sheets.Count;

        /// <summary>
        /// Instantiates a new workbook with a new <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="xlApp">The Excel application instance.</param>
        public Workbook(XL.Application xlApp)
        {
            wrkbk = xlApp.Workbooks.Add();
            wrkbk.Activate();
            Sheets.Add(new Worksheet((_Worksheet)wrkbk.ActiveSheet));
            ActiveWorksheet = Sheets[0];
        }

        /// <summary>
        /// Instantiates the workbook and selects the first <see cref="Worksheet"/> to be read.
        /// </summary>
        /// <param name="xlApp">The Excel application instance.</param>
        /// <param name="path">The path to the Excel file to open.</param>
        public Workbook(XL.Application xlApp, string path = "")
        {
            wrkbk = xlApp.Workbooks.Open(path);
            wrkbk.Activate();
            Sheets.Add(new Worksheet((_Worksheet)wrkbk.ActiveSheet));
            ActiveWorksheet = Sheets[0];
        }

        /// <summary>
        /// Selects the active worksheet by its index.
        /// </summary>
        /// <param name="index">The index of the worksheet to select.</param>
        public void SelectSheet(int index) => ActiveWorksheet = Sheets[index];

        /// <summary>
        /// Adds a new <see cref="Worksheet"/> to this workbook.
        /// </summary>
        /// <param name="name">The name of the new worksheet (optional).</param>
        public void AddNewSheet(string name = "")
        {
            Sheets.Add(new Worksheet((_Worksheet)wrkbk.Worksheets.Add(After: wrkbk.Sheets[Count])));
            ActiveWorksheet = Sheets[Sheets.Count - 1];

            if (!string.IsNullOrEmpty(name))
                ActiveWorksheet.SetName(name);
        }

        /// <summary>
        /// Saves the workbook.
        /// </summary>
        /// <param name="filePath">The file path where the workbook will be saved.</param>
        /// <exception cref="WorkbookException">Thrown when the file cannot be saved because it is open.</exception>
        public void Save(string filePath)
        {
            try
            {
                wrkbk.SaveAs(filePath);
                Close();
            }
            catch (COMException)
            {
                wrkbk.Close(false); // Discard changes.
                throw new WorkbookException("Cannot save the file because it is open");
            }
        }

        /// <summary>
        /// Closes the workbook. This method is called by <see cref="Save(string)"/>.
        /// </summary>
        public void Close() => wrkbk?.Close();

        /// <summary>
        /// Releases all resources used by the workbook and its worksheets.
        /// </summary>
        public void Destroy()
        {
            foreach (Worksheet sheet in Sheets)
                sheet.Destroy();

            Sheets.Clear();
            Marshal.ReleaseComObject(wrkbk);
        }
    }
}