using System.Management;
using System.Runtime.InteropServices;

namespace Backend.Exceptions
{
    public class RunAsAdminException() : ManagementException("You must have Administration permissions to perform this action.") { }
    public class PrinterNotFoundException(string printerName) : Exception($"No {printerName} was found in this computer.");
    public class PDFPrinterNotFoundException() : PrinterNotFoundException("PDF Printer");
    public class ExcelIndexException() : Exception("Indexes in Excel starts from 1") { };
    public class WorkbookException(string message) : COMException(message) { }
    public class MissingExcelException() : NullReferenceException("Excel is not installed.") { }
    public class NoModelException() : NullReferenceException("No Model is set") { }
    public class NoNavigatorException() : NullReferenceException("No Navigator is set") { }
    public class AssemblyCreateInstanceFailure(string text) : Exception(text) { }
    public class CurrentUserNotSetException() : Exception("CurrentUser.Is property was not set.") { }
    public class InvalidTargetsException(string target1, string target2) : Exception($"{target1} and {target2} arguments cannot be null or empty strings") { }
    public class InvalidReceiver() : Exception("No Receiver information was provided") { };
    public class InvalidSender() : Exception("No Sender information was provided") { };
    public class InvalidHost() : Exception("Host was not provided") { };
    public class CredentialFailure(string text) : Exception(text) { };

    /// <summary>
    /// The Exception that is thrown when the attempt to load a DLL has failed.
    /// </summary>
    /// <param name="dllName">The DLL to load.</param>
    public class DLLLoadFailure(string dllName) : Exception($"Failed to load {dllName}") { }
    
    /// <summary>
    /// The Exception that is thrown when the attempt to load a function from an Assembly has failed. This happens if the function is not part of the Assembly.
    /// </summary>
    /// <param name="functionName">name of the function to fetch.</param>
    /// <param name="dllName">the name of the Assembly where the function was supposed to be.</param>
    public class ExtractionFunctionFailure(string functionName, string dllName) : Exception($"Failed to load {functionName} from {dllName}") { }

}
