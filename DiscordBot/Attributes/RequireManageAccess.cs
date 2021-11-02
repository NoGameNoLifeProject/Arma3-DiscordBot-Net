using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Configs;
using Discord.WebSocket;

namespace DiscordBot.Attributes
{
    /// <summary>
    ///     Requires the user invoking the command to have a ManageServer permission.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireManageAccess : PreconditionAttribute
    {
        public override string ErrorMessage { get; set; }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as SocketGuildUser;
            if (Program.Configuration.DiscordAdminRoleAccess && user.GuildPermissions.Administrator)
                return PreconditionResult.FromSuccess();

            foreach (var role in user.Roles)
            {
                if (Program.Configuration.DiscordManageRoleId.Contains(role.Id))
                {
                    return PreconditionResult.FromSuccess();
                }
            }

            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
                    if (context.User.Id == application.Owner.Id)
                        return PreconditionResult.FromSuccess();
                    break;
            }
            return PreconditionResult.FromError(ErrorMessage ?? "Недостаточно прав для использования команды");
        }
    }
}
