using DiscordBot.Configs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using DiscordBot.OBS;
using Serilog;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;

namespace DiscordBot.Common
{
    public class Arma3Server
    {
        private static Arma3Config _config { get; set; }
        public static Process Arma3Process { get; set; }
        public static Process Arma3HCProcess { get; set; }

        public static Arma3Config Config { get => _config ??= BuildConfig(); }

        private static string _curProcessOwner { get; set; }
        private static string CurProcessOwner { get => _curProcessOwner ??= Utils.GetProcessOwner(Process.GetCurrentProcess().Id); }
        
        public static string Arma3Path { get; set; }

        public static event Action<Item, float> OnDownloadProgress;
        public static event Action<Item> OnDownloadEnd;

        public static async Task StartServer()
        {
            UpdateMission();
            Arma3Process = new Process();
            try
            {
                var workshopPaths = await GetWorkshopModsPathsList();
                var customPaths = await GetCustomModsPathsList();
                var mods = $"{string.Join(";", workshopPaths)};{string.Join(";", customPaths)}";

                Arma3Process.StartInfo.UseShellExecute = true;
                Arma3Process.StartInfo.FileName = Arma3Path + "\\arma3server_x64.exe";
                Arma3Process.StartInfo.CreateNoWindow = false;
                Arma3Process.StartInfo.Arguments = $"" +
                                                   $"-profiles={Config.A3ProfilesPath} " +
                                                   $"-cfg={Config.A3NetworkConfigName} " +
                                                   $"-config={Config.A3ServerConfigName} " +
                                                   $"-mod={mods} " +
                                                   $"-servermod={string.Join(";", GetServerModsList())} " +
                                                   $"{Config.A3ServerLaunchParams} " +
                                                   $"-port=" + Program.Configuration.ServerGamePort;
                Log.Information("Starting Arma 3 server {Args}", Arma3Process.StartInfo.Arguments);
                Arma3Process.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при запуске сервера");
                throw new Exception("Ошибка при запуске сервера");
            }

            if (Config.UseHClient)
                await StartHeadlessClient();
        }
        
        public static async Task StartHeadlessClient()
        {
            Arma3HCProcess = new Process();
            try
            {
                var workshopPaths = await GetWorkshopModsPathsList();
                var customPaths = await GetCustomModsPathsList();
                var mods = $"{string.Join(";", workshopPaths)};{string.Join(";", customPaths)}";
                
                Arma3HCProcess.StartInfo.UseShellExecute = true;
                Arma3HCProcess.StartInfo.FileName = Arma3Path + "\\arma3server_x64.exe";
                Arma3HCProcess.StartInfo.CreateNoWindow = false;
                Arma3HCProcess.StartInfo.Arguments = $"" +
                                                     $"-profiles={Config.A3ProfilesPath} " +
                                                     $"-mod={mods} " +
                                                     $"-servermod={string.Join(";", GetServerModsList())} " +
                                                     $"{Config.A3HCLaunchParams} " +
                                                     $"-port=" + Program.Configuration.ServerGamePort;
                Log.Information("Starting Arma 3 Headless Client {Args}", Arma3HCProcess.StartInfo.Arguments);
                Arma3HCProcess.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при запуске Headless клиента");
                throw new Exception("Ошибка при запуске Headless клиента");
            }
        }

        public static bool IsServerRunning()
        {
            var arma3process = Process.GetProcessesByName("arma3server_x64");
            return arma3process.Select(process => Utils.GetProcessOwner(process.Id)).Any(pOwner => pOwner is not null && pOwner == CurProcessOwner);
        }

        public static void StopServer()
        {
            try
            {
                WebSocketClient.SocketClose();
                if (Arma3Process is not null) { Arma3Process.Kill(); }
                if (Arma3HCProcess is not null) { Arma3HCProcess.Kill(); }
                var arma3process = Process.GetProcessesByName("arma3server_x64");
                foreach (Process process in arma3process)
                {
                    var pOwner = Utils.GetProcessOwner(process.Id);
                    if (pOwner is not null && pOwner == CurProcessOwner)
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

        public static async Task RestartServer(Action<string> status = null)
        {
            StopServer();
            status?.Invoke("Сервер остановлен, проверяем обновления");
            await CheckForUpdates(status);
            status?.Invoke("Запускаем сервер");
            await StartServer();
        }

        public static async Task CheckForUpdates(Action<string> status)
        {
            status.Invoke("Проверяем моды [Steam]");
            await ProcessSteamMods(
                (item, progress) =>
                {
                    Log.Information("Обновление мода [{Mod}]: {Progress}%", item.Title, progress * 100);
                },
                (item) =>
                {
                    status.Invoke($"Мод {item.Title} обновлен");
                    Log.Information("Мод [{Mod}] обновлен", item.Title);
                });
            status.Invoke("Проверяем моды [Custom]");
            Directory.CreateDirectory($"{Arma3Path}\\{Config.A3CustomModsPath}");

            var progressListener = new ObsProgressListener();
            progressListener.Status += progressStatus => Log.Information("{Progress}% {AverageSpeed}Мбит/сек",
                progressStatus.getTransferPercentage(), Math.Round(progressStatus.getAverageSpeed() / 125000, 2));
            await ObsUtils.ValidateInstalledAddons(progressListener, status);
            status.Invoke("Проверки завершены");
        }

        public static async Task ProcessSteamMods(Action<Item, float> onDownloadProgress, Action<Item> onDownloadEnd)
        {
            var modsForUpdate = await GetModsForUpdate();
            await DownloadMods(modsForUpdate, onDownloadProgress, onDownloadEnd);
        }

        public static async Task DownloadMods(List<Item?> mods, Action<Item, float> onDownloadProgress, Action<Item> onDownloadEnd)
        {
            SteamUGC.OnDownloadItemResult += (result) => Log.Information("Результат обновления мода {Result}", result);
            OnDownloadEnd+= onDownloadEnd;
            OnDownloadProgress += onDownloadProgress;
            foreach (var item in mods)
            {
                if (!(item?.IsSubscribed ?? false))
                {
                    await item?.Subscribe();
                }
                await DownloadModAsync((PublishedFileId)item?.Id, 3);
            }
        }

        public static async Task<List<Item?>> GetModsForUpdate()
        {
            var mods = await MySQLClient.GetWorkshopList();
            var modsForUpdate = new ConcurrentBag<Item?>();
            await mods.ParallelForEachAsync(async (mod) =>
            {
                var itemInfo = await SteamUGC.QueryFileAsync(mod.ModID) ?? new Item(mod.ModID);
                bool needUpdate = false;
                long lastUpdate = 0;
                if (itemInfo.IsInstalled && File.Exists($"{itemInfo.Directory}\\meta.cpp"))
                {
                    using (var reader = File.OpenText($"{itemInfo.Directory}\\meta.cpp"))
                    {
                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (line is null) break;
                            if (line.Contains("timestamp"))
                            {
                                long.TryParse(line[12..^1], out lastUpdate);
                                break;
                            }
                        }
                    }
                }
                else
                    needUpdate = true;

                if (lastUpdate != 0)
                {
                    var lastUpdateDate = DateTime.FromBinary(lastUpdate);
                    if (Math.Abs((itemInfo.Updated - lastUpdateDate).TotalMinutes) > 5)
                    {
                        needUpdate = true;
                        await UpdateModDate(itemInfo.Updated, itemInfo.Directory);
                    }
                }

                if (!itemInfo.IsInstalled || itemInfo.NeedsUpdate || needUpdate)
                    modsForUpdate.Add(itemInfo);

                if (!itemInfo.IsSubscribed)
                    await itemInfo.Subscribe();
            });
            return modsForUpdate.ToList();
        }

        private static async Task<bool> DownloadModAsync(PublishedFileId fileId, int secondsUpdateDelay = 5, int secondsCancellationTokenDelay = 240)
        {
            var item = new Steamworks.Ugc.Item(fileId);
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(secondsCancellationTokenDelay)).Token;

            OnDownloadProgress?.Invoke(item, 0.0f);
            if (item.Download(true) == false)
                return item.IsInstalled;

            await Task.Delay(secondsUpdateDelay * 1000);

            while (true)
            {
                if (ct.IsCancellationRequested)
                    break;

                OnDownloadProgress?.Invoke(item, item.DownloadAmount);

                if (!item.IsDownloading && item.IsInstalled)
                    break;

                await Task.Delay(secondsUpdateDelay * 1000);
            }

            OnDownloadProgress?.Invoke(item, 1.0f);
            OnDownloadEnd?.Invoke(item);

            return item.IsInstalled;
        }
        
        public static async Task<List<string>> GetWorkshopModsPathsList()
        {
            var mods = await MySQLClient.GetWorkshopList();
            var modsPaths = new ConcurrentBag<string>();
            await mods.ParallelForEachAsync(async (mod) =>
            {
                var itemInfo = await SteamUGC.QueryFileAsync(mod.ModID);
                modsPaths.Add($"\"{itemInfo?.Directory}\"");
            });
            return modsPaths.ToList();
        }

        public static async Task<List<string>> GetCustomModsPathsList()
        {
            var mods = await MySQLClient.GetCustomModsList();
            var modsPaths = new List<string>();
            foreach (var mod in mods)
            {
                modsPaths.Add($"\"{Arma3Path}\\mods\\{mod.Name}\"");
            }
            return modsPaths;
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
                        File.Move(file, Arma3Path + "\\mpmissions\\" + file);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Ошибка при преносе миссии из временного каталога");
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex2)
                        {
                            Log.Error(ex2, "Ошибка при удалении мисии из временного каталога");
                        }
                    }
                }
            }
        }

        public static List<string> GetAvailableMissions()
        {
            var files = Directory.GetFiles($"{Arma3Path}\\mpmissions\\", "*.pbo");
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
                File.Move(ms, Arma3Path + "\\mpmissions\\" + Path.GetFileName(ms), true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при переносе новой миссии");
                throw new Exception("Ошибка при переносе новой миссии");
            }
            Log.Information("Загруженная миссия перемещна в по пути {Path}", Arma3Path + "\\mpmissions\\" + Path.GetFileName(ms));
        }

        public static void SetMS(string ms)
        {
            var path = Arma3Path + "\\" + Config.A3ServerConfigName;
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
                }
                catch (Exception ex)
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

            Thread.Sleep(5000); // TODO: Вспомнить зачем тут этот костыль
            MoveNewMission(ms);
            SetMS(ms);
        }

        private static async Task UpdateModDate(DateTime updated, string modPath)
        {
            if (!File.Exists($"{modPath}\\meta.cpp")) return;

            List<string> newFile = new List<string>();
            using (var sr = new StreamReader($"{modPath}\\meta.cpp"))
            {
                while (sr.Peek() >= 0)
                {
                    string line = await sr.ReadLineAsync();
                    if (line.Contains("timestamp"))
                    {
                        line = $"timestamp = {updated.ToBinary()};";
                    }
                    newFile.Add(line);
                }
            }

            using (var sw = new StreamWriter($"{modPath}\\meta.cpp"))
            {
                try
                {
                    foreach (var line in newFile)
                    {
                        await sw.WriteLineAsync(line);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Ошибка при попытке изменить дату обновления мода {ModPath}", modPath);
                }
            }
        }

        private static List<string> GetServerModsList()
        {
            return Directory.GetDirectories(Arma3Path + "\\" + Config.A3ServerModsPath).Select(mod => $"\"{mod}\"").ToList();
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
