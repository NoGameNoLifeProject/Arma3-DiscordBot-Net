using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Common.SteamBridge
{
    public delegate void SteamOutput(object sender, string text);

    public delegate void SteamStarted(object sender);
    public delegate void SteamExited(object sender, SteamExitReason reason);

    public delegate void LoggedIn(object sender);

    public delegate void LoginFailed(object sender, LoginResult reason);

    public delegate void AppUpdated(object sender, bool error = false);
    public delegate void AppUpdateStateChanged(object sender, SteamAppUpdateState state);

    public delegate void ModDownloaded(object sender, string folder);


    public partial class SteamInstance
    {

        public event AppUpdated AppUpdated;
        public event SteamExited SteamExited;
        public event LoggedIn LoggedIn;
        public event LoginFailed LoginFailed;
        public event SteamOutput SteamOutput;
        public event ModDownloaded ModDownloaded;
        public event AppUpdateStateChanged AppUpdateStateChanged;

        private FileInfo SteamExeFile;
        public Process Steam { set; get; }

        private static string _curProcessOwner { get; set; }
        private static string CurProcessOwner { get => _curProcessOwner ??= Utils.GetProcessOwner(Process.GetCurrentProcess().Id); }

        public bool LoginState { get; private set; }


        public SteamInstance(FileInfo pSteamExeFile)
        {
            SteamExeFile = pSteamExeFile;
        }

        public void Start(string args = "", bool asAdmin = false)
        {
            Steam = new Process();
            Steam.StartInfo.FileName = SteamExeFile.FullName;

            Steam.StartInfo.RedirectStandardError = true;
            Steam.StartInfo.RedirectStandardInput = true;
            Steam.StartInfo.RedirectStandardOutput = true;
            Steam.StartInfo.UseShellExecute = false;

            Steam.StartInfo.CreateNoWindow = true;

            Steam.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Steam.StartInfo.Arguments = args;

            if (asAdmin) Steam.StartInfo.Verb = "runas";

            Steam.Start();

            Steam.StandardInput.AutoFlush = true;
            Steam.StandardInput.WriteLine();

            Steam.EnableRaisingEvents = true;

            Steam.ErrorDataReceived += Steam_DataReceived;
            Steam.OutputDataReceived += Steam_DataReceived;
            Steam.Exited += Steam_Exited;

            Steam.BeginErrorReadLine();
            Steam.BeginOutputReadLine();
        }

        private void Steam_Exited(object sender, EventArgs e)
        {
            SteamExited?.Invoke(this, SteamExitReason.NothingSpecial);
        }

        private void Steam_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            string line = e.Data;

            SteamOutput?.Invoke(this, line);

            if (line.Contains("cannot run from a folder path that includes non-English characters"))
            {
                close(SteamExitReason.NonEnglishCharachers);
            }
            else if (line.Equals("FAILED with result code 5") | line.Equals("Login with cached credentials FAILED with result code 5") | line.Equals("password: FAILED (Invalid Password)"))
            {
                LoginFailed?.Invoke(this, LoginResult.WrongInformation);
            }
            else if (line.Equals("FAILED with result code 88"))
            {
                LoginFailed?.Invoke(this, LoginResult.TwoFactorWrong);
            }
            else if (line.Equals("FAILED with result code 65"))
            {
                LoginFailed?.Invoke(this, LoginResult.SteamGuardCodeWrong);
            }
            else if (line.Equals("FAILED with result code 71"))
            {
                LoginFailed?.Invoke(this, LoginResult.ExpiredCode);
            }
            else if (line.Equals("FAILED with result code 84"))
            {
                LoginFailed?.Invoke(this, LoginResult.RateLimitedExceeded);
            }
            else if (line.Contains("using 'set_steam_guard_code'"))
            {
                LoginFailed?.Invoke(this, LoginResult.SteamGuardNotSupported);
            }
            else if (line.Contains("Enter the current code from your Steam Guard Mobile Authenticator app"))
            {
                LoginFailed?.Invoke(this, LoginResult.WaitingForSteamGuard);
            }
            else if (line.Contains("FAILED with result code 50"))
            {
                LoginFailed?.Invoke(this, LoginResult.AlreadyLoggedIn);
            }
            else if (LoginState == false & (line.Contains("Waiting for user info...OK") | line.Contains("Logged in OK")))
            {
                LoginState = true;
                LoggedIn?.Invoke(this);
            }
            else if (Regex.IsMatch(line, "ERROR! Download item [0-9]+ failed (Access Denied)."))
            {
                ModDownloaded?.Invoke(this, null);
            }
            else if (Regex.IsMatch(line, "Error! App '[0-9]+' state is 0x[0-9]+ after update job."))
            {
                AppUpdated?.Invoke(this, false);
            }
            else if (Regex.IsMatch(line, @"Update state \(0x5\) verifying install, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)"))
            {
                Regex pattern = new Regex(@"Update state \(0x5\) verifying install, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)");
                Match match = pattern.Match(line);


                SteamAppUpdateState state = new SteamAppUpdateState();
                state.percentage = Convert.ToInt32(match.Groups[1].Value);
                state.receivedBytes = Convert.ToInt64(match.Groups[3].Value);
                state.totalBytes = Convert.ToInt64(match.Groups[4].Value);
                state.stage = UpdateStateStage.Validating;

                AppUpdateStateChanged?.Invoke(this, state);
            }
            else if (Regex.IsMatch(line, @"Update state \(0x61\) downloading, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)"))
            {
                Regex pattern = new Regex(@"Update state \(0x61\) downloading, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)");
                Match match = pattern.Match(line);


                SteamAppUpdateState state = new SteamAppUpdateState();
                state.percentage = Convert.ToInt32(match.Groups[1].Value);
                state.receivedBytes = Convert.ToInt64(match.Groups[3].Value);
                state.totalBytes = Convert.ToInt64(match.Groups[4].Value);
                state.stage = UpdateStateStage.Downloading;

                AppUpdateStateChanged?.Invoke(this, state);
            }
            else if (Regex.IsMatch(line, @"Update state \(0x81\) commiting, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)"))
            {
                Regex pattern = new Regex(@"Update state \(0x81\) commiting, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)");
                Match match = pattern.Match(line);


                SteamAppUpdateState state = new SteamAppUpdateState();
                state.percentage = Convert.ToInt32(match.Groups[1].Value);
                state.receivedBytes = Convert.ToInt64(match.Groups[3].Value);
                state.totalBytes = Convert.ToInt64(match.Groups[4].Value);
                state.stage = UpdateStateStage.Commiting;

                AppUpdateStateChanged?.Invoke(this, state);
            }
            else if (line.Contains("Success! App '"))
               AppUpdated?.Invoke(this, true);
            else if (line.Contains("Success. Downloaded item") & line.Contains("bytes"))
            {
                ModDownloaded?.Invoke(this, line.Split('"')[1]);
            }

        }

        public async Task close(SteamExitReason reason = SteamExitReason.NothingSpecial)
        {
            await Task.Run(() =>
            {
                Steam.Kill();
            });
        }

        public static void killAll()
        {
            foreach (Process process in Process.GetProcessesByName("steamcmd"))
            {
                try
                {
                    var pOwner = Utils.GetProcessOwner(process.Id);
                    if (pOwner != null && pOwner == CurProcessOwner)
                    {
                        process.Kill();
                    }
                }
                catch (Exception) { }
            }
        }
    }

    public class SteamAppUpdateState
    {
        public UpdateStateStage stage;
        public int percentage = 0;
        public long receivedBytes = 0;
        public long totalBytes = 0;
    }
}
