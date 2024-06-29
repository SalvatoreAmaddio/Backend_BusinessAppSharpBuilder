using System.Runtime.InteropServices;
using XL = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using Backend.Exceptions;

namespace Backend.Office
{
    /// <summary>
    /// Provides methods to easily access and manage Excel files using COM objects.
    /// </summary>
    public class Excel : IDestroyable
    {
        Application? xlApp;

        /// <summary>
        /// Gets the Excel workbook.
        /// </summary>
        public Workbook? WorkBook { get; private set; }

        /// <summary>
        /// Gets the current active worksheet.
        /// </summary>
        public Worksheet? Worksheet { get => WorkBook?.ActiveWorksheet; }

        /// <summary>
        /// Creates a new Excel file.
        /// </summary>
        /// <exception cref="MissingExcelException">Thrown when Excel is not installed on the system.</exception>       
        public void Create() 
        {
            xlApp = new XL.Application();
            if (xlApp == null) throw new MissingExcelException();
            WorkBook = new(xlApp);
        }

        /// <summary>
        /// Reads an existing Excel file.
        /// </summary>
        /// <param name="path">The path to the Excel file.</param>
        /// <exception cref="MissingExcelException">Thrown when Excel is not installed on the system.</exception>
        public void Read(string path) 
        {
            xlApp = new XL.Application();
            if (xlApp == null) throw new MissingExcelException();
            WorkBook = new(xlApp, path);
        }

        /// <summary>
        /// Saves the current workbook to the specified file path.
        /// How to use:
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
        /// <remarks>
        /// This method calls <see cref="Workbook.Close"/>. Since it can throw a <see cref="WorkbookException"/>, wrap this method in a try-catch-finally block.
        /// </remarks>
        /// <param name="filePath">The path where the Excel file will be saved.</param>
        public void Save(string filePath) => WorkBook?.Save(filePath);

        /// <summary>
        /// Closes the Excel application and performs clean-up operations.
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

        /// <summary>
        /// Releases COM objects and performs additional clean-up operations.
        /// </summary>
        public void Destroy() 
        {
            if (xlApp == null) return;
            Marshal.ReleaseComObject(xlApp);
            xlApp = null;
        }

    }
}