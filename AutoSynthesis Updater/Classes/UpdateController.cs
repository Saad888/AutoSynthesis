using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;

namespace AutoSynthesis_Updater
{
    class UpdateController
    {
        #region Properties
        // UI Elements
        private ProgressBar PGBUpdate { get; set; }

        // Arguments
        private const string Arguments = "-isupdated";

        // URLs
        private string UpdateMetadataURL { get; set; }
        private string AutoSynthesisFileName { get; set; } = "AutoSynthesis.exe";
        private string BaseURL { get; set; }

        // Data Objects
        private UpdateMetadata UpdateMD { get; set; }
        #endregion

        public UpdateController(ProgressBar pGBUpdate)
        {
            PGBUpdate = pGBUpdate;
            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            BaseURL = System.IO.Path.GetDirectoryName(path).Replace(@"file:\", "") + @"\";

            if (Debugger.IsAttached)
                UpdateMetadataURL = "https://autosynthesis.blob.core.windows.net/autosynthesis/Dev/AutoSynthesisUpdate.json";
            else
                UpdateMetadataURL = "https://autosynthesis.blob.core.windows.net/autosynthesis/Releases/AutoSynthesisUpdate.json";


        }

        public void UpdateAutoSynthesis()
        {
            // PROCESS:
            // Download update version update doc (json file)
            // Check Current AutoSynthesis Version
            // If Autosynth version is lower than the doc version, initiate update
            // Download the files with version files
            // Extract files in current folder overwriting existing files

            try
            {

                // Download JSON File with Metadata for the Update
                GetUpdateMetadata();
                // Get Current version added to UpdateMD
                GetCurrentVersion();

                // If Current Version is up to date, terminate
                if (UpdateMD.CurrentVersion == UpdateMD.UpdateVersion)
                    return;

                // Download Most Recent File From UpdaterJson
                GetUpdateFiles();
            } 
            finally
            {
                Thread.Sleep(1000);
                var url = BaseURL + AutoSynthesisFileName;
                Process.Start(url, Arguments);
                PGBUpdate.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
            }


        }

        #region Get Metadata Information
        /// <summary>
        /// Aquire the Metadata information from Azure
        /// </summary>
        private void GetUpdateMetadata()
        {
            Log("Getting Metadata");
            var startTime = DateTime.Now;

            string contents;
            using (var webClient = new WebClient())
            {
                contents = webClient.DownloadString(UpdateMetadataURL);
            }

            UpdateMD = JsonConvert.DeserializeObject<UpdateMetadata>(contents);

            var diff = (DateTime.Now - startTime).TotalMilliseconds;
            Log($"Metadata Aquired in {diff}ms");
        }

        /// <summary>
        /// Aquire version information for existing application
        /// </summary>
        private void GetCurrentVersion()
        {
            Log("Getting Current Version Info");
            try
            {
                var currentAutoSynthesisVersion = FileVersionInfo.GetVersionInfo(BaseURL + AutoSynthesisFileName);
                UpdateMD.CurrentVersion = currentAutoSynthesisVersion.FileVersion.ToString();
                Log("Current AutoSynthesis Version Aquired: " + UpdateMD.CurrentVersion);
            } 
            catch
            {
                Log("File Not Found");
                UpdateMD.CurrentVersion = "null";
            }
        }
        #endregion

        #region Download Update Files
        private void GetUpdateFiles()
        {
            foreach(var key in UpdateMD.UpdateFiles.Keys)
            {
                var fileName = key;
                var fileURL = UpdateMD.UpdateFiles[key];

                Log("Downloading Files for New Version " + UpdateMD.UpdateVersion);
                var time = DateTime.Now;

                var downloadedUpdateUrl = BaseURL + fileName;
                using (var webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                    webClient.DownloadFileAsync(new Uri(fileURL), downloadedUpdateUrl);
                }

                var diff = (DateTime.Now - time).TotalMilliseconds;
                Log($"File Downloaded in {diff}ms");
            }

        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            PGBUpdate.Dispatcher.Invoke(() => { PGBUpdate.Value = int.Parse(Math.Truncate(percentage).ToString()); });
            
        }

        #endregion


        #region Logging Method
        private void Log(string content)
        {
            Console.WriteLine(DateTime.Now + " - " + content);
        }
        #endregion
    }
}
