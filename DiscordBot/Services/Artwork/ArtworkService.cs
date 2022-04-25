using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Lavalink4NET;
using Lavalink4NET.Player;

namespace DiscordBot.Services.Artwork;

public class ArtworkService : IArtworkService, IDisposable
{
    private readonly ILavalinkCache? _cache;
    private HttpClient? _httpClient;
    private bool _disposed;

    public ArtworkService(ILavalinkCache? cache = null)
    {
        _cache = cache;
    }

    /// <inheritdoc/>
    public virtual ValueTask<Uri> ResolveAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default) => lavalinkTrack.Provider switch
    {
        StreamProvider.YouTube => ResolveArtworkForYouTubeAsync(lavalinkTrack, cancellationToken),
        StreamProvider.SoundCloud => ResolveArtworkForSoundCloudAsync(lavalinkTrack, cancellationToken),
        StreamProvider.Bandcamp => ResolveArtworkForBandcampAsync(lavalinkTrack, cancellationToken),
        StreamProvider.Vimeo => ResolveArtworkForVimeoAsync(lavalinkTrack, cancellationToken),
        StreamProvider.Twitch => ResolveArtworkForTwitchAsync(lavalinkTrack, cancellationToken),
        StreamProvider.Local => ResolveArtworkForLocalAsync(lavalinkTrack, cancellationToken),
        StreamProvider.Http => ResolveArtworkForHttpAsync(lavalinkTrack, cancellationToken),
        _ => ResolveArtworkForCustomTrackAsync(lavalinkTrack, cancellationToken),
    };


    protected virtual ValueTask<Uri?> ResolveArtworkForCustomTrackAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureNotDisposed();
        return default;
    }

    protected virtual ValueTask<Uri?> ResolveArtworkForYouTubeAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureNotDisposed();

        var uri = new Uri($"https://img.youtube.com/vi/{lavalinkTrack.TrackIdentifier}/maxresdefault.jpg");
        return CreateResultFromSynchronous(uri);
    }

    protected virtual async ValueTask<Uri?> ResolveArtworkForSoundCloudAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureNotDisposed();

        var cacheKey = default(string?);
        if (_cache is not null)
        {
            cacheKey = $"soundcloud-artwork-{lavalinkTrack.TrackIdentifier}";

            if (_cache.TryGetItem<Uri>(cacheKey, out var cacheItem))
            {
                return cacheItem;
            }
        }

        var requestUri = new Uri($"https://soundcloud.com/oembed?url={lavalinkTrack.TrackIdentifier}&format=json");
        var thumbnailUri = await FetchTrackUriFromOEmbedCompatibleResourceAsync(requestUri, cancellationToken).ConfigureAwait(false);

        if (_cache is not null)
        {
            _cache.AddItem(cacheKey!, thumbnailUri, DateTimeOffset.UtcNow + TimeSpan.FromMinutes(60));
        }

        return thumbnailUri;
    }

    protected virtual ValueTask<Uri?> ResolveArtworkForBandcampAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureNotDisposed();
        return default;
    }

    protected virtual ValueTask<Uri?> ResolveArtworkForVimeoAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureNotDisposed();

        var uri = new Uri($"https://i.vimeocdn.com/video/{lavalinkTrack.TrackIdentifier}.png");
        return CreateResultFromSynchronous(uri);
    }

    protected virtual ValueTask<Uri?> ResolveArtworkForTwitchAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureNotDisposed();
        return default;
    }

    protected virtual ValueTask<Uri?> ResolveArtworkForLocalAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureNotDisposed();
        return default;
    }

    protected virtual ValueTask<Uri?> ResolveArtworkForHttpAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureNotDisposed();
        return default;
    }

    private static ValueTask<Uri?> CreateResultFromSynchronous(Uri? resultUri = null)
    {
#if NET5_0_OR_GREATER
        return ValueTask.FromResult(resultUri);
#else
        return new ValueTask<Uri?>(Task.FromResult<Uri?>(resultUri));
#endif
    }

    protected HttpClient GetHttpClient()
    {
        EnsureNotDisposed();
        return _httpClient ??= new HttpClient();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (disposing)
        {
            _httpClient?.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private async ValueTask<Uri?> FetchTrackUriFromOEmbedCompatibleResourceAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        var httpResponse = await GetHttpClient()
            .GetFromJsonAsync<JsonObject>(requestUri, cancellationToken)
            .ConfigureAwait(false);

        // OEmbed responses contain a thumbnail_uri property as per standard
        var thumbnailUriNode = httpResponse?["thumbnail_url"]
            ?? throw new InvalidOperationException("Unable to find thumbnail URI in response.");

        var rawThumbnailUri = thumbnailUriNode.GetValue<string>();
        return rawThumbnailUri is null ? default : new Uri(rawThumbnailUri);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ArtworkService));
        }
    }
}