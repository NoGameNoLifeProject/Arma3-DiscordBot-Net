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

namespace DiscordBot.Common
{
    public class Arma3Server
    {
        private static Arma3Config _config {  get; set; }
        public static Process Arma3Process { get; set; }

        public static Arma3Config Config { get => _config ??= BuildConfig(); }

        private static string _curProcessOwner { get; set; }
        private static string CurProcessOwner { get => _curProcessOwner ??= GetProcessOwner(Process.GetCurrentProcess().Id); }

        public static bool StartServer()
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
                Console.WriteLine("Starting Arma 3 server: " + Arma3Process.StartInfo.Arguments);
                Arma3Process.Start();
                return true;
            } catch (Exception e)
            {
                Console.WriteLine($"Ошибка при запуске сервера {e.Message}");
                return false;
            }
        }

        public static bool StopServer()
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
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка при остановке сервера {e.Message}");
                return false;
            }
        }

        public static string RestartServer()
        {
            bool res;
            res = StopServer();
            if (!res)
                return "Ошибка при остановке сервера";

            res = StartServer();
            if (!res)
                return "Ошибка при запуске сервера";

            return "Сервер успешно перезагружен";
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
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        try
                        {
                            File.Delete(file);
                        }catch  (Exception e2)
                        {
                            Console.WriteLine(e2.Message);
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

        public static bool MoveNewMission(string ms)
        {
            try
            {
                File.Move(ms, Program.Configuration.A3serverPath + "\\mpmissions\\" + Path.GetFileName(ms), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            Console.WriteLine($"Загруженная миссия перемещна в по пути {Program.Configuration.A3serverPath}\\mpmissions\\{Path.GetFileName(ms)}");
            return true;
        }

        public static bool SetMS(string ms)
        {
            var path = Program.Configuration.A3serverPath + "\\" + Program.Configuration.A3ServerConfigName;
            bool success = false;
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
                foreach (string line in newFile) {
                    sw.WriteLine(line);
                }
                success = true;
            }
            if (success) 
                Console.WriteLine($"Миссия изменена на {ms}");
            else
                Console.WriteLine($"Ошибка при записи миссии {ms} в конфиг");
            return success;
        }

        public static void UpdateMission()
        {
            var ms = GetNewMission();
            if (ms == null) return;

            Thread.Sleep(5000);
            var move = MoveNewMission(ms);
            if (move == false) return;

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
