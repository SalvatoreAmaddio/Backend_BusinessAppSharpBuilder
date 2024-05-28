using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Backend.Utils
{
    public class Sys
    {
        /// <summary>
        /// Gets the Desktop's Path.
        /// </summary>
        public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        /// <summary>
        /// Gets the Application's Assembly.
        /// </summary>
        public static Assembly? AppAssembly { get; } = Assembly.GetEntryAssembly();
        
        /// <summary>
        /// Gets the Application's Name
        /// </summary>
        public static string? AppName { get; } = AppAssembly?.GetName()?.Name;

        /// <summary>
        /// Gets the Application's Version
        /// </summary>
        public static string? AppVersion { get => AppAssembly?.GetName()?.Version?.ToString(); }

        /// <summary>
        /// Collection of Loaded Assemblies. See <see cref="LoadedAssembly"/>
        /// </summary>
        public static List<LoadedAssembly> LoadedDLL { get; } = [];

        /// <summary>
        /// Check if an object is a number.
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>True if the object is a number</returns>
        public static bool IsNumber(object? obj)
        {
            if (obj == null) return false;

            Type objType = obj.GetType();
            return objType == typeof(int) || objType == typeof(double) ||
                   objType == typeof(decimal) || objType == typeof(float) ||
                   objType == typeof(long) || objType == typeof(short) ||
                   objType == typeof(ulong) || objType == typeof(ushort) ||
                   objType == typeof(sbyte) || objType == typeof(byte);
        }

        /// <summary>
        /// It loads all EmbeddedResource dll.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static void LoadAllEmbeddedDll()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string[] resources = executingAssembly.GetManifestResourceNames();

            foreach (string resourceName in resources) 
            {
                if (IsDLL(resourceName)) 
                {
                    using (Stream? stream = executingAssembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream == null) throw new Exception($"Resource {resourceName} not found.");

                        string tempFile = Path.Combine(Path.GetTempPath(), resourceName);
                        using (FileStream fs = new(tempFile, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fs);
                        }

                        if (!NativeLibrary.TryLoad(tempFile, out IntPtr handle)) throw new Exception($"Failed to load DLL: {tempFile}");

                        LoadedAssembly assembly = new(tempFile, resourceName, IntPtr.Size == 8 ? "x64" : "x86");
                        assembly.Load();
                        LoadedDLL.Add(assembly);
                    }
                }     
            }
        }

        private static bool IsDLL(string resource) => resource.EndsWith("dll");

        /// <summary>
        /// It loads a EmbeddedResource dll 
        /// </summary>
        /// <param name="dllName">The name of the dll</param>
        /// <exception cref="Exception">Resource not found Exception</exception>
        public static void LoadEmbeddedDll(string dllName, string nameSpace = "Backend.Database")
        {
            string architecture = IntPtr.Size == 8 ? "bit64" : "x86";
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string resourceName = $"{nameSpace}.{architecture}.{dllName}.dll";

            using (Stream? stream = executingAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) throw new Exception($"Resource {resourceName} not found.");

                string tempFile = Path.Combine(Path.GetTempPath(), dllName);
                using (FileStream fs = new(tempFile, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fs);
                }

                if (!NativeLibrary.TryLoad(tempFile, out IntPtr handle)) throw new Exception($"Failed to load DLL: {tempFile}");

                LoadedAssembly assembly = new(tempFile, dllName, IntPtr.Size == 8 ? "x64" : "x86");
                assembly.Load();
                LoadedDLL.Add(assembly);
            }
        }

    }
}
