extern alias obsjava;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DiscordBot.Common;
using DiscordBot.Common.Entities;
using DiscordBot.Configs;
using ikvm.runtime;
using java.nio.file;
using Microsoft.Extensions.Configuration;
using obsjava::com.obs.services;
using obsjava::com.obs.services.model;
using Steamworks;
using Path = System.IO.Path;

namespace DiscordBot.OBS;

public class ObsDownload
{
    private static ObsConfig _config { get; set; }

    public static ObsConfig Config => _config ??= BuildConfig();

    public ObsClient Client { get; set; }
    
    public ObsDownload()
    {
        Client = new ObsClient(Config.AccessKey, Config.SecurityKey, Config.ObsEndpoint);
        var bucketExist = Client.headBucket(Config.BucketName);
        if (!bucketExist) throw new Exception("Отсутствует запрашиваемый бакет, все сломалось...");
    }
    
    public async Task DonwloadFiles(List<LauncherModsCustomFiles> files, ObsProgressListener progressListener)
    {
        var modsPath = $"{SteamApps.AppInstallDir(Arma3Server.Config.A3ServerId)}\\{Arma3Server.Config.A3CustomModsPath}";
        foreach (var file in files)
        {
            GetObjectRequest request = new GetObjectRequest(Config.BucketName, file.Name);
            request.setProgressListener(progressListener);
            request.setProgressInterval(15 * 1024 * 1024L);
            ObsObject obsObject = Client.getObject(request);
            var input = obsObject.getObjectContent();
            var path = Paths.get($"{modsPath}\\{file.Name}");
            Files.copy(input, path, StandardCopyOption.REPLACE_EXISTING);
        }
    }
    
    public async Task ProcessNewAddon(LauncherModsCustom mod, ObsProgressListener progressListener)
    {
        var modsPath =  $"{SteamApps.AppInstallDir(Arma3Server.Config.A3ServerId)}\\{Arma3Server.Config.A3CustomModsPath}";
        Directory.CreateDirectory($"{modsPath}\\{mod.Name}");
        var files = await MySQLClient.GetCustomModsFilesList(mod.ID);
        foreach (var file in files)
        {
            var path = $"{modsPath}\\{file.Name}";
            path = path.Substring(0, path.IndexOf(Path.GetFileName(file.Name)));
            Directory.CreateDirectory(path);
        }

        await DonwloadFiles(files, progressListener);
    }
    
    private static ObsConfig BuildConfig()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        return builder.GetSection("ObsConfig").Get<ObsConfig>();
    }

    ~ObsDownload()
    {
        Client.close();
    }
}