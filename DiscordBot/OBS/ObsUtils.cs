extern alias obsjava;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dasync.Collections;
using DiscordBot.Common;
using DiscordBot.Common.Entities;
using Steamworks;

namespace DiscordBot.OBS;

public static class ObsUtils
{
    public static string CalculateMD5(string path)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 128 * 1024 * 1024))
            {
                var hash = md5.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
        }
    }

    public static async Task ValidateInstalledAddons(ObsProgressListener progressListener, Action<string> status)
    {
        var modsPath = $"{SteamApps.AppInstallDir(Arma3Server.Config.A3ServerId)}\\{Arma3Server.Config.A3CustomModsPath}";
        if (!Directory.Exists(modsPath))
        {
            return;
        }

        var obsDownload = new ObsDownload();
        var customMods = await MySQLClient.GetCustomModsList();
        foreach (var mod in customMods)
        {
            if (Directory.Exists($"{modsPath}\\{mod.Name}"))
            {
                status.Invoke($"Проверяем мод [{mod.Name}]");
                var files = await ValidateAddon(mod);
                if (files is not null && files.Count > 0)
                {
                    await obsDownload.DonwloadFiles(files, progressListener);
                }
            }
            else
            {
                await obsDownload.ProcessNewAddon(mod, progressListener);
            }
        }
    }

    public static async Task<List<LauncherModsCustomFiles>> ValidateAddon(LauncherModsCustom mod)
    {
        var filesForUpdate = new List<LauncherModsCustomFiles>();
        var modsPath = $"{SteamApps.AppInstallDir(Arma3Server.Config.A3ServerId)}\\{Arma3Server.Config.A3CustomModsPath}";
        var files = await MySQLClient.GetCustomModsFilesList(mod.ID);
        await files.ParallelForEachAsync(async (file) =>
        {
            if (File.Exists($"{modsPath}\\{file.Name}"))
            {
                if (CalculateMD5($"{modsPath}\\{file.Name}") != file.MD5)
                {
                    filesForUpdate.Add(file);
                }
            }
            else
            {
                filesForUpdate.Add(file);
            }
        });
        await RemoveNeedlessFiles(mod);
        return filesForUpdate;
    }

    public static async Task RemoveNeedlessFiles(LauncherModsCustom mod)
    {
        var addonFiles = await MySQLClient.GetCustomModsFilesDictionary(mod.ID);
        await Directory.GetFiles($"{SteamApps.AppInstallDir(Arma3Server.Config.A3ServerId)}\\{Arma3Server.Config.A3CustomModsPath}\\{mod.Name}", "*.*", SearchOption.AllDirectories).ParallelForEachAsync(
            async (file) =>
            {
                var index = file.IndexOf(Path.GetFileName(mod.Name), StringComparison.Ordinal);
                var path = file.Substring(index == -1 ? 0 : index).Replace("\\", "/");
                if (!addonFiles.ContainsKey(path))
                {
                    File.Delete(file);
                }
            });
    }
}