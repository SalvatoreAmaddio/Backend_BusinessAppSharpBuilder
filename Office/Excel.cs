using System.Runtime.InteropServices;
using XL = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using Backend.Exceptions;

namespace Backend.Office
{
    /// <summary>
    /// This class allows to easily access and manage the COM object for dealing with Excel Files.
    /// </summary>
    public class Excel : IDestroyable
    {
        Application? xlApp;

        /// <summary>
        /// Gets the Excel's Workbook.
        /// </summary>
        public Workbook? WorkBook { get; private set; }

        /// <summary>
        /// Get's the current active Worksheet.
        /// </summary>
        public Worksheet? Worksheet { get => WorkBook?.ActiveWorksheet; }
       
        /// <summary>
        /// Create a new Excel File.
        /// </summary>
        /// <exception cref="MissingExcelException"></exception>
        public void Create() 
        {
            xlApp = new XL.Application();
            if (xlApp == null) throw new MissingExcelException();
            WorkBook = new(xlApp);
        }

        /// <summary>
        /// Read an existing excel file.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="MissingExcelException"></exception>
        public void Read(string path) 
        {
            xlApp = new XL.Application();
            if (xlApp == null) throw new MissingExcelException();
            WorkBook = new(xlApp, path);
        }

        /// <summary>
        /// This method calls <see cref="Workbook.Close"/>. Since it can throw a <see cref="WorkbookException"/>, wrap this method in a try-catch-finally block.
        /// For Example:
        /// <code>
        /// try 
        /// {
        ///     excel.Save("C:\\Users\\salva\\Desktop\\prova.xlsx");
        /// }
        /// catch (WorkbookException ex)
        /// {
        ///     return Task.FromException(ex); //return the exception.
        /// }
        /// finally 
        /// {
        ///     excel.Close(); //ensure the clean-up will always occur.
        /// }
        /// </code>
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(string filePath) => WorkBook?.Save(filePath);

        /// <summary>
        /// Close the Excel file and performs clean-up operations.
        /// </summary>
        public void Close() 
        {
            xlApp?.Quit();
            Worksheet?.Destroy();
            WorkBook?.Destroy();
            Destroy();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Destroy() 
        {
            if (xlApp == null) return;
            Marshal.ReleaseComObject(xlApp);
        }

    }
}