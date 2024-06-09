using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Utils
{
    public class FileTransfer
    {
        public string SourceFilePath { get; set; } = string.Empty;

        private string _destinationFolder = string.Empty;
        public string DestinationFolder 
        { 
            get => _destinationFolder; 
            set 
            { 
                _destinationFolder = value;
                DestinationFolder = Path.Combine(_destinationFolder, NewFileName);
            }
        } 
        
        private string _newFileName = string.Empty;
        public string NewFileName 
        { 
            get => _newFileName;
            set 
            { 
                _newFileName = value;
                DestinationFilePath = Path.Combine(DestinationFolder, _newFileName);
            }
        }
        public string DestinationFilePath { get; private set; } = string.Empty;

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

        public async Task<bool> CopyAsync(bool overwrite = true) 
        {
            try
            {
                return await Task.Run(() =>
                                        { 
                                            File.Copy(SourceFilePath, DestinationFilePath, overwrite); 
                                            return true; 
                                        }
                                     );
            }
            catch (IOException ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return false;
            }
        }
    }
}