using Backend.Database;
using Backend.Model;
using System.Reflection;
using System.Runtime.InteropServices;
using Backend.Enums;

namespace Backend.Utils
{
    /// <summary>
    /// Provides system-level utilities and information about the current application.
    /// </summary>
    public class Sys
    {
        /// <summary>
        /// Creates a folder at the specified path if it does not already exist.
        /// </summary>
        /// <param name="folderPath">The path of the folder to create.</param>
        public static void CreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        /// <summary>
        /// Attempts to delete a file at the specified path.
        /// </summary>
        /// <param name="filePath">The path of the file to delete.</param>
        /// <returns>True if the file was successfully deleted, or if the file path is null or empty; otherwise, false.</returns>
        public static bool AttemptFileDelete(string? filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath)) return true;
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the path of the current application.
        /// </summary>
        /// <returns>The base directory of the current application.</returns>
        public static string AppPath() => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Extracts the <see cref="TimeSpan"/> from a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="date">The date from which to extract the time.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the time component of the specified date, or null if the date is null.</returns>
        public static TimeSpan? GetTime(DateTime? date)
        {
            if (date == null) return null;
            return new TimeSpan(date.Value.Hour, date.Value.Minute, date.Value.Second);
        }

        /// <summary>
        /// Stores a new user in the database.
        /// </summary>
        /// <param name="user">The user to store.</param>
        public static void SaveNewUser(IUser user)
        {
            Encrypter encrypter = new(user.Password);
            user.Password = encrypter.Encrypt();
            List<QueryParameter> para = new() { new("UserName", user.UserName), new("Password", user.Password) };
            DatabaseManager.Find("User")?.Crud(CRUD.INSERT, $"INSERT INTO User (UserName, Password) VALUES (@UserName, @Password)", para);
            encrypter.ReplaceStoredKeyIV(SysCredentailTargets.UserLoginEncrypterSecretKey, SysCredentailTargets.UserLoginEncrypterIV);
        }

        /// <summary>
        /// Checks if the database has any users. If no user is found, updates the <see cref="FirstTimeLogin"/> setting.
        /// </summary>
        public static void OnNoUsers()
        {
            UpdateFirstTimeLogin(DatabaseManager.Find("User")?.CountRecords() == 0);
        }

        /// <summary>
        /// Gets the email username from the application settings.
        /// </summary>
        public static string EmailUserName => Properties.Backend.Default.EmailUserName;

        /// <summary>
        /// Updates the email username in the application settings.
        /// </summary>
        /// <param name="value">The new email username.</param>
        public static void UpdateEmailUserName(string value)
        {
            Properties.Backend.Default.EmailUserName = value;
            Properties.Backend.Default.Save();
        }

        /// <summary>
        /// Gets the first time login status from the application settings.
        /// </summary>
        public static bool FirstTimeLogin => Properties.Backend.Default.FirstTimeLogin;

        /// <summary>
        /// Updates the first time login status in the application settings.
        /// </summary>
        /// <param name="value">The new first time login status.</param>
        public static void UpdateFirstTimeLogin(bool value)
        {
            Properties.Backend.Default.FirstTimeLogin = value;
            Properties.Backend.Default.Save();
        }

        /// <summary>
        /// Gets the path to the desktop.
        /// </summary>
        public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        /// <summary>
        /// Gets the application's assembly.
        /// </summary>
        public static Assembly? AppAssembly { get; } = Assembly.GetEntryAssembly();

        /// <summary>
        /// Gets the application's name.
        /// </summary>
        public static string? AppName { get; } = AppAssembly?.GetName()?.Name;

        /// <summary>
        /// Gets the application's version.
        /// </summary>
        public static string? AppVersion => AppAssembly?.GetName()?.Version?.ToString();

        /// <summary>
        /// Gets the collection of loaded assemblies. See <see cref="LoadedAssembly"/>.
        /// </summary>
        public static List<LoadedAssembly> LoadedDLL { get; } = new();

        /// <summary>
        /// Checks if an object is a number.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if the object is a number; otherwise, false.</returns>
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
        /// Loads all embedded resource DLLs.
        /// </summary>
        /// <exception cref="Exception">Thrown when a resource is not found or a DLL fails to load.</exception>
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
        /// Loads an embedded resource DLL.
        /// </summary>
        /// <param name="dllName">The name of the DLL.</param>
        /// <param name="nameSpace">The namespace where the DLL is located. Default is "Backend.Database".</param>
        /// <exception cref="Exception">Thrown when the resource is not found or the DLL fails to load.</exception>
        public static void LoadEmbeddedDll(string dllName, string nameSpace = "Backend.Database")
        {
            string architecture = IntPtr.Size == 8 ? "x64" : "x86";
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string resourceName = $"{nameSpace}.{architecture}.{dllName}.dll";

            using Stream? stream = executingAssembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new Exception($"Resource {resourceName} not found.");

            string tempFile = Path.Combine(Path.GetTempPath(), dllName);
            using FileStream fs = new(tempFile, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fs);

            if (!NativeLibrary.TryLoad(tempFile, out IntPtr handle)) throw new Exception($"Failed to load DLL: {tempFile}");

            LoadedAssembly assembly = new(tempFile, dllName, IntPtr.Size == 8 ? "x64" : "x86");
            assembly.Load();
            LoadedDLL.Add(assembly);
        }
    }

}
