using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoUpdaterSlave
{
    public partial class MainForm : Form
    {
        private string _downloadUrl { get; set; }
        private string _applicationPath { get; set; }
        private string _restartPath { get; set; }

        /// <summary>
        /// Constructor purely for designer
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(string downloadUrl, string applicationPath, string restartPath)
        {
            _downloadUrl = downloadUrl;
            _applicationPath = applicationPath;
            _restartPath = restartPath;

            InitializeComponent();

            this.Shown += MainForm_Shown;
        }

        void MainForm_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            ProcessUpdate();
        }

        /// <summary>
        /// Retrieve a list of files which tells us everything that lives in the update. Then
        /// pull the files one by one and place them where required.
        /// </summary>
        private void ProcessUpdate()
        {
            label1.Text = "Getting file list";
            Application.DoEvents();
            // A bit of a sleep to allow our caller to clear
            Thread.Sleep(2000);

            var request = (HttpWebRequest)WebRequest.Create(_downloadUrl + "/FileList.json");
            var response = (HttpWebResponse)request.GetResponse();
            var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var fileList = JsonConvert.DeserializeObject<FileList>(rawJson);
            var numberOfFilesToGet = fileList.Files.Count();
            // TODO this would be much nicer as tasks and callbacks
            progressBar1.Maximum = numberOfFilesToGet + 1;
            progressBar1.PerformStep();
            Application.DoEvents();

            // Create the target folder, if required
            if (!Directory.Exists(_applicationPath))
            {
                Directory.CreateDirectory(_applicationPath);
            }
            // Get the files and put them in the target folder
            foreach (var file in fileList.Files)
            {
                label1.Text = "Downloading " + file.Name;
                Application.DoEvents();

                request = (HttpWebRequest)WebRequest.Create(_downloadUrl + "/" + file.Name);
                response = (HttpWebResponse)request.GetResponse();
                using (var inStream = response.GetResponseStream())
                using (var outStream = new FileStream(_applicationPath + "/" + file.Name,
                    FileMode.OpenOrCreate, FileAccess.Write))
                {
                    inStream.CopyTo(outStream);
                }

                progressBar1.PerformStep();
                Application.DoEvents();
            }

            label1.Text = "Done";
            Application.DoEvents();

            Thread.Sleep(2000);

            // Restart the main application
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _applicationPath + "/" + _restartPath,
                UseShellExecute = false
            };
            Process.Start(processStartInfo);

            // ... and we are done
            this.Close();
        }
    }

    public class FileList
    {
        public IEnumerable<File> Files { get; set; }
    }

    public class File
    {
        public string Name { get; set; }
    }
}