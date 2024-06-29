using Backend.Exceptions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Backend.Utils
{
    /// <summary>
    /// Represents an object holding a reference to an external assembly.
    /// This class also uses the Win32 API to load functions from assemblies.
    /// </summary>
    public class LoadedAssembly
    {
        #region Win32
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedAssembly"/> class.
        /// </summary>
        /// <param name="path">The path where the assembly is located.</param>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="architecture">The architecture of the assembly.</param>
        public LoadedAssembly(string path, string name, string architecture)
        {
            Path = path;
            Name = name;
            Architecture = architecture;
        }

        /// <summary>
        /// Gets the architecture of the loaded assembly.
        /// </summary>
        public string Architecture { get; }

        /// <summary>
        /// Gets the name of the DLL.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the path of the DLL.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the actual loaded assembly.
        /// </summary>
        /// <returns>An <see cref="Assembly"/> object.</returns>
        public Assembly? Assembly { get; private set; }

        /// <summary>
        /// Loads the assembly from the specified path.
        /// </summary>
        /// <returns>True if the assembly could be loaded; otherwise, false.</returns>
        public bool Load()
        {
            try
            {
                Assembly = Assembly.LoadFile(Path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a function from the assembly as the specified delegate type. This method uses the Win32 API.
        /// </summary>
        /// <typeparam name="D">The type of delegate.</typeparam>
        /// <param name="functionName">The name of the function to load.</param>
        /// <returns>A delegate of type <typeparamref name="D"/>.</returns>
        /// <exception cref="DLLLoadFailure">Thrown when the provided DLL path is incorrect or the DLL cannot be loaded.</exception>
        /// <exception cref="ExtractionFunctionFailure">Thrown when the specified function cannot be found in the DLL.</exception>
        public D LoadFunction<D>(string functionName)
        {
            IntPtr hModule = LoadLibrary(Path);
            if (hModule == IntPtr.Zero)
                throw new DLLLoadFailure(Name);

            IntPtr pFunc = GetProcAddress(hModule, functionName);
            if (pFunc == IntPtr.Zero)
            {
                FreeLibrary(hModule);
                throw new ExtractionFunctionFailure(functionName, Name);
            }

            D? delegateFunction = Marshal.GetDelegateForFunctionPointer<D>(pFunc);
            FreeLibrary(hModule);
            return delegateFunction;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string? ToString() => $"{Name}.dll - Architecture: {Architecture}";
    }

}
