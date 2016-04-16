using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdaterForWPF
{
    public enum AutoUpdateResult
    {
        NoUpdates,
        NoNewerUpdates,
        UpdateNotWanted,
        UpdateInitiated
    }

    public class AutoUpdater
    {
        public AutoUpdateResult DoUpdate(string baseUrl)
        {
            var request = (HttpWebRequest)WebRequest.Create(baseUrl + "/Versions.json");
            var response = (HttpWebResponse)request.GetResponse();
            var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var versions = JsonConvert.DeserializeObject<VersionList>(rawJson);
            if (!versions.Versions.Any())
                return AutoUpdateResult.NoUpdates;

            var mainExecutable = Assembly.GetEntryAssembly();
            var currentVersion = mainExecutable.GetName().Version;
            var proposedVersion = versions.Versions.OrderByDescending(x => x.FullVersion).First();
            if (proposedVersion.FullVersion < currentVersion)
                return AutoUpdateResult.NoNewerUpdates;

            var window = new AutoUpdaterWindow(versions.AppName, proposedVersion);
            if (!window.ShowDialog().Value)
                return AutoUpdateResult.UpdateNotWanted;

            // Initiate the Slave
            var slave = Path.GetDirectoryName(mainExecutable.Location) + "/AutoUpdaterSlave.exe";

            // Url to download folder, application path, restart exe

            var arg0 = proposedVersion.BaseUrl;
            var arg1 = Path.GetDirectoryName(mainExecutable.Location);
            var arg2 = Path.GetFileName(mainExecutable.Location);

            var args = arg0 + " " + arg1 + " " + arg2;

            Process.Start(slave, args);
            return AutoUpdateResult.UpdateInitiated;
        }
    }

    public class VersionList
    {
        public string AppName { get; set; }
        public IEnumerable<VersionData> Versions { get; set; }
    }

    public class VersionData
    {
        public Version FullVersion { get { return System.Version.Parse(Version); } }

        public string Version { get; set; }
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string ReleaseNotes { get; set; }
    }
}
