using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBot.Common.SteamBridge
{
    public delegate void SteamInstalled(object sender, FileInfo SteamCMDExe);
    public delegate void SteamInstallationError(object sender, Exception ex);

    public class SteamInstaller
    {
        public SteamInstalled SteamInstalled;
        public SteamInstallationError SteamInstallationError;

        public DirectoryInfo Folder { get; set; }
        public bool Installed { get { return SteamCMDExe.Exists; } }
        private FileInfo SteamCMDExe
        {
            get
            {
                return new FileInfo(Folder.FullName + "\\steamcmd.exe");
            }
        }

        private FileInfo SteamCMDZip
        {
            get
            {
                return new FileInfo(Folder.FullName + "\\steamcmd.zip");
            }
        }

        public Uri DownloadLink
        {
            get
            {
                return new Uri("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
            }
        }
        
        public SteamInstaller(string folder)
        {
            Folder = new DirectoryInfo(folder);
        }

        private void prepareForInstallation()
        {
            if (!Folder.Exists)
            {
                Folder.Create();
            }

            clearCacheFile();

            if (SteamCMDExe.Exists)
            {
                SteamCMDExe.Delete();
            }
        }

        private void clearCacheFile()
        {
            if (SteamCMDZip.Exists)
            {
                SteamCMDZip.Delete();
            }
        }

        public async Task installSteam()
        {
            try
            {
                prepareForInstallation();
                await downloadPackage();
            }
            catch (Exception ex)
            {
                SteamInstallationError?.Invoke(this, ex);
            }
        }

        private async Task downloadPackage()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(DownloadLink);
            using (var stream = new FileStream(SteamCMDZip.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                response.Content.CopyToAsync(stream).Wait();
            }
            unzipPackage();
        }

        private void unzipPackage()
        {
            FastZip archive = new FastZip();
            archive.CreateEmptyDirectories = true;
            archive.ExtractZip(SteamCMDZip.FullName, Folder.FullName, "");
            clearCacheFile();

            SteamInstalled?.Invoke(this, SteamCMDExe);
        }
    }
}
