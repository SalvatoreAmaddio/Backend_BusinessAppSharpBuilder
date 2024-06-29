namespace Backend.Utils
{
    /// <summary>
    /// Provides functionality to copy files from a source path to a destination path.
    /// </summary>
    public class FileTransfer
    {
        /// <summary>
        /// Gets or sets the source file path.
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        private string _destinationFolder = string.Empty;
        /// <summary>
        /// Gets or sets the destination folder.
        /// Setting this property also updates the <see cref="DestinationFilePath"/>.
        /// </summary>
        public string DestinationFolder
        {
            get => _destinationFolder;
            set
            {
                _destinationFolder = value;
                DestinationFilePath = Path.Combine(_destinationFolder, NewFileName);
            }
        }

        private string _newFileName = string.Empty;
        /// <summary>
        /// Gets or sets the new file name for the destination file.
        /// Setting this property also updates the <see cref="DestinationFilePath"/>.
        /// </summary>
        public string NewFileName
        {
            get => _newFileName;
            set
            {
                _newFileName = value;
                DestinationFilePath = Path.Combine(DestinationFolder, _newFileName);
            }
        }

        /// <summary>
        /// Gets the full destination file path, combining <see cref="DestinationFolder"/> and <see cref="NewFileName"/>.
        /// </summary>
        public string DestinationFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Copies the file from the source path to the destination path.
        /// </summary>
        /// <param name="overwrite">Indicates whether to overwrite the destination file if it exists. Default is true.</param>
        public void Copy(bool overwrite = true)
        {
            try
            {
                File.Copy(SourceFilePath, DestinationFilePath, overwrite);
            }
            catch (IOException ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Asynchronously copies the file from the source path to the destination path.
        /// </summary>
        /// <param name="overwrite">Indicates whether to overwrite the destination file if it exists. Default is true.</param>
        /// <returns>A task that represents the asynchronous copy operation. The task result is true if the copy was successful, otherwise false.</returns>
        public async Task<bool> CopyAsync(bool overwrite = true)
        {
            try
            {
                return await Task.Run(() =>
                {
                    File.Copy(SourceFilePath, DestinationFilePath, overwrite);
                    return true;
                });
            }
            catch (IOException ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return false;
            }
        }
    }

}