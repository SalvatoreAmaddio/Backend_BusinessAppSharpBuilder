using Backend.Exceptions;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using XL = Microsoft.Office.Interop.Excel;

namespace Backend.Office
{
    /// <summary>
    /// This class allows to easily access and manage the COM object for dealing with Excel's Workbooks.
    /// </summary>
    public class Workbook : IDestroyable
    {
        XL.Workbook wrkbk;

        /// <summary>
        /// Gets the currently active worksheet.
        /// </summary>
        public Worksheet ActiveWorksheet { get; private set; }
        
        /// <summary>
        /// Collections of all Worksheets in the Workbook.
        /// </summary>
        public readonly List<Worksheet> Sheets = [];

        /// <summary>
        /// Gets the total number of Worksheets in this Workbook.
        /// </summary>
        public int Count => Sheets.Count;

        /// <summary>
        /// Istantiates a new Workbook with a new <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="xlApp"></param>
        public Workbook(XL.Application xlApp)
        {
            wrkbk = xlApp.Workbooks.Add();
            wrkbk.Activate();
            Sheets.Add(new Worksheet((_Worksheet)wrkbk.ActiveSheet));
            ActiveWorksheet = Sheets[0];
        }

        /// <summary>
        /// Istantiates the Workbook and selects the first <see cref="Worksheet"/> to be read.
        /// </summary>
        /// <param name="xlApp"></param>
        /// <param name="path"></param>
        public Workbook(XL.Application xlApp, string path = "")
        {
            wrkbk = xlApp.Workbooks.Open(path);
            wrkbk.Activate();
            Sheets.Add(new Worksheet((_Worksheet)wrkbk.ActiveSheet));
            ActiveWorksheet = Sheets[0];
        }

        /// <summary>
        /// Selects the <see cref="ActiveWorksheet"/>
        /// </summary>
        /// <param name="index"></param>
        public void SelectSheet(int index) => ActiveWorksheet = Sheets[index];

        /// <summary>
        /// Add a new <see cref="Worksheet"/> to this Workbook.
        /// </summary>
        /// <param name="name"></param>
        public void AddNewSheet(string name = "") 
        {
            Sheets.Add(new Worksheet((_Worksheet)wrkbk.Worksheets.Add(After: wrkbk.Sheets[Count])));
            ActiveWorksheet = Sheets[Sheets.Count-1];

            if (!string.IsNullOrEmpty(name)) 
                ActiveWorksheet.SetName(name);
        }

        /// <summary>
        /// Saves the workbook.
        /// </summary>
        /// <param name="filePath">the file path where to save the workbook</param>
        /// <exception cref="WorkbookException"></exception>
        public void Save(string filePath) 
        {
            try
            {
                wrkbk.SaveAs(filePath);
                Close();
            }
            catch (COMException)
            {
                wrkbk.Close(false); //discard changes.
                throw new WorkbookException("Cannot save the file because it is open");
            }
        }

        /// <summary>
        /// Closes the Workbook. This method is called by <see cref="Save(string)"/>
        /// </summary>
        public void Close() => wrkbk?.Close();

        public void Destroy() 
        {
            foreach (Worksheet sheet in Sheets) 
                sheet.Destroy();

            Sheets.Clear();
            Marshal.ReleaseComObject(wrkbk);
        } 
    }
}