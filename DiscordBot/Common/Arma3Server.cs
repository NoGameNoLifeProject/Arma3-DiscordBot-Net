using DiscordBot.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Management;
using Serilog;

namespace DiscordBot.Common
{
    public class Arma3Server
    {
        private static Arma3Config _config {  get; set; }
        public static Process Arma3Process { get; set; }

        public static Arma3Config Config { get => _config ??= BuildConfig(); }

        private static string _curProcessOwner { get; set; }
        private static string CurProcessOwner { get => _curProcessOwner ??= GetProcessOwner(Process.GetCurrentProcess().Id); }

        public static void StartServer()
        {
            UpdateMission();
            Arma3Process = new Process();
            try
            {
                Arma3Process.StartInfo.UseShellExecute = true;
                Arma3Process.StartInfo.FileName = Config.A3serverPath + "\\arma3server_x64.exe";
                Arma3Process.StartInfo.CreateNoWindow = false;
                Arma3Process.StartInfo.Arguments = $"" +
                    $"-profiles={Config.A3ProfilesPath} " +
                    $"-cfg={Config.A3NetworkConfigName} " +
                    $"-config={Config.A3ServerConfigName} " +
                    $"-mod={string.Join(";", Config.Mods.Keys.ToList().Select(x => Config.A3ModsPath + x))} " +
                    $"-servermod={string.Join(";", Config.ServerMods.Keys.ToList().Select(x => Config.A3ModsPath + x))} " +
                    $"-world=empty -autoInit -bepath=BattlEye " +
                    $"-port=" + Program.Configuration.ServerGamePort;
                Log.Information("Starting Arma 3 server {Args}", Arma3Process.StartInfo.Arguments);
                Arma3Process.Start();
            } catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при запуске сервера");
                throw new Exception("Ошибка при запуске сервера");
            }
        }

        public static void StopServer()
        {
            try
            {
                WebSocketClient.SocketClose();
                if (Arma3Process != null) { Arma3Process.Kill(); }
                Process[] arma3process = Process.GetProcessesByName("arma3server_x64");
                foreach (Process process in arma3process)
                {
                    var pOwner = GetProcessOwner(process.Id);
                    if (pOwner != null && pOwner == CurProcessOwner)
                    {
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при остановке сервера");
                throw new Exception("Ошибка при остановке сервера");
            }
        }

        public static void RestartServer()
        {
            StopServer();
            StartServer();
        }

        public static void ClearDownloadFolder()
        {
            var files = Directory.GetFiles("Downloads", "*.pbo");
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    try
                    {
                        File.Move(file, Program.Configuration.A3serverPath + "\\mpmissions\\" + file);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Ошибка при преносе миссии из временного каталога");
                        try
                        {
                            File.Delete(file);
                        }catch  (Exception ex2)
                        {
                            Log.Error(ex2, "Ошибка при удалении мисии из временного каталога");
                        }
                    }
                }
            }
        }

        public static List<string> GetAvailableMissions()
        {
            var files = Directory.GetFiles($"{Program.Configuration.A3serverPath}\\mpmissions\\", "*.pbo");
            var list = new List<string>(files);
            return list;
        }

        public static string GetNewMission()
        {
            if (!Directory.Exists("Downloads")) 
                return null;

            var files = Directory.GetFiles("Downloads", "*.pbo");
            if (files.Length > 0)
                return files[0];

            return null;
        }

        public static void MoveNewMission(string ms)
        {
            try
            {
                File.Move(ms, Program.Configuration.A3serverPath + "\\mpmissions\\" + Path.GetFileName(ms), true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при переносе новой миссии");
                throw new Exception("Ошибка при переносе новой миссии");
            }
            Log.Information("Загруженная миссия перемещна в по пути {Path}", Program.Configuration.A3serverPath + "\\mpmissions\\" + Path.GetFileName(ms));
        }

        public static void SetMS(string ms)
        {
            var path = Program.Configuration.A3serverPath + "\\" + Program.Configuration.A3ServerConfigName;
            List<string> newFile = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (sr.Peek() >= 0)
                {
                    string newLine = sr.ReadLine();
                    if (newLine.Contains("template"))
                    {
                        newLine = $"\t\ttemplate = \"{Path.GetFileNameWithoutExtension(ms)}\";";
                    }
                    newFile.Add(newLine);
                }
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                try
                {
                    foreach (string line in newFile)
                    {
                        sw.WriteLine(line);
                    }
                } catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при записи миссии {Mission} в конфиг", ms);
                    throw new Exception($"Ошибка при записи миссии {ms} в конфиг");
                }
            }
            Log.Information("Миссия изменена на {Mission}", ms);
        }

        public static void UpdateMission()
        {
            var ms = GetNewMission();
            if (ms == null) return;

            Thread.Sleep(5000);
            MoveNewMission(ms);
            SetMS(ms);
        }

        private static string GetProcessOwner(int processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    return argList[1] + "\\" + argList[0];
                }
            }

            return "NO OWNER";
        }

        private static Arma3Config BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            return builder.GetSection("Arma3Config").Get<Arma3Config>();
        }
    }
}
