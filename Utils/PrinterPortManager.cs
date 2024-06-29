using Backend.Exceptions;
using System.Management;
using System.Runtime.InteropServices;

namespace Backend.Utils
{
    /// <summary>
    /// This class follows the Singleton pattern to load the CreateDeletePort function from the PrinterPortManager.dll.
    /// This class works together with <see cref="PrinterPortManager"/>.
    /// </summary>
    public sealed class CreateDeletePort
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate uint CreateDeletePortDelegate(int action, string portName);

        private static readonly Lazy<CreateDeletePort> lazyInstance = new(() => new CreateDeletePort());

        /// <summary>
        /// Gets the singleton instance of the <see cref="CreateDeletePortDelegate"/>.
        /// </summary>
        public static CreateDeletePortDelegate Execute => lazyInstance.Value.CreateDeletePortDel;

        private readonly CreateDeletePortDelegate CreateDeletePortDel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDeletePort"/> class.
        /// </summary>
        internal CreateDeletePort()
        {
            CreateDeletePortDel = Sys.LoadedDLL.First(s => s.Name.Contains("PrinterPortManager.dll"))
                                         .LoadFunction<CreateDeletePortDelegate>("CreateDeletePort");
        }
    }

    /// <summary>
    /// This class interacts with PDFDriverHelper.dll to add and remove PDF Printer's ports.
    /// Then, the <see cref="ManagementScope"/> sets the port as the default port that the printer will use while printing.
    /// <para/>
    /// <c>IMPORTANT:</c>
    /// Ensure that your application runs with the appropriate permissions by setting the following in the app manifest:
    /// <para/>
    /// <c>&lt;requestedExecutionLevel level="requireAdministrator" uiAccess="false"/></c>
    /// </summary>
    public class PrinterPortManager
    {
        internal enum PortAction
        {
            ADD = 0,
            REMOVE = 1
        }

        /// <summary>
        /// Gets or sets the name of the new port.
        /// </summary>
        public string NewPortName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the directory path for the port.
        /// </summary>
        public string DirectoryPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets the full file path for the new port.
        /// </summary>
        public string FilePath => Path.Combine(DirectoryPath, $"{NewPortName}.pdf");

        private readonly string originalPort = "PORTPROMPT:";
        private readonly string printerName = "Microsoft Print To PDF";
        private ManagementScope? managementScope;

        /// <summary>
        /// Initiates the connection to the <see cref="ManagementScope"/>.
        /// This method is called by <see cref="GetPrinter"/>.
        /// </summary>
        private void Connect()
        {
            managementScope = new ManagementScope(ManagementPath.DefaultPath, new ConnectionOptions
            {
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.PacketPrivacy,
                EnablePrivileges = true
            });

            managementScope.Connect();
        }

        /// <summary>
        /// Gets the PDF printer.
        /// </summary>
        /// <returns>A <see cref="ManagementObject"/> representing the printer.</returns>
        private ManagementObject? GetPrinter()
        {
            Connect();
            SelectQuery query = new(@"SELECT * FROM Win32_Printer WHERE Name = '" + printerName.Replace("\\", "\\\\") + "'");
            ManagementObjectSearcher searcher = new(managementScope, query);
            return searcher.Get().Cast<ManagementObject>().FirstOrDefault();
        }

        /// <summary>
        /// Sets the default port to PORTPROMPT: and deletes the newly created port.
        /// </summary>
        public void ResetPort()
        {
            SetDefaultPort(true);
            CreateDeletePort.Execute((int)PortAction.REMOVE, FilePath);
        }

        /// <summary>
        /// Creates a new port and sets it as the default port using a <see cref="ManagementObject"/>.
        /// </summary>
        public void SetPort()
        {
            CreateDeletePort.Execute((int)PortAction.ADD, FilePath);
            SetDefaultPort();
        }

        /// <summary>
        /// Sets the default printer port.
        /// </summary>
        /// <param name="useOriginal">If true, sets the default port to PORTPROMPT:.</param>
        /// <exception cref="PrinterNotFoundException">Thrown when the printer is not found.</exception>
        private void SetDefaultPort(bool useOriginal = false)
        {
            ManagementObject? printer = GetPrinter() ?? throw new PrinterNotFoundException(printerName);
            printer["PortName"] = useOriginal ? originalPort : FilePath;
            try
            {
                printer.Put();
            }
            catch
            {
                throw new RunAsAdminException();
            }
        }
    }

}