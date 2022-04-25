using System;
using System.Threading;
using System.Threading.Tasks;
using Lavalink4NET.Player;

namespace DiscordBot.Services.Artwork;

public interface  IArtworkService
{
    ValueTask<Uri> ResolveAsync(LavalinkTrack lavalinkTrack, CancellationToken cancellationToken = default);
}