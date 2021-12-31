using Dawn;
using Discord;
using Discord.WebSocket;
using DiscordBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules.Commands
{
    public static class Utils
    {
        private static DiscordSocketClient _client;
        public static DiscordSocketClient Client => _client ??= Program.Client;
        public static ulong? ApplicationOwnerID { get; private set; }

        public static long ConvertLong(string Long)
        {
            Guard.Argument(Long, nameof(Long)).NotNull().NotEmpty();
            var success = long.TryParse(Long, out long cLong);
            if (success)
            {
                return cLong;
            }
            throw new Exception("Передано некорректное значение");
        }

        public static int ConvertInt(string Int)
        {
            Guard.Argument(Int, nameof(Int)).NotNull().NotEmpty();
            var success = int.TryParse(Int, out int cInt);
            if (success)
            {
                return cInt;
            }
            return 1;
        }

        public static (int, DateTime) ConvertBanTime(string bantime)
        {
            var endTime = DateTime.Now;
            int infinity = 0;
            if (string.IsNullOrEmpty(bantime))
            {
                infinity = 1;
            }
            else
            {
                var banTime = TimeSpan.Zero;
                int temp;
                var success = int.TryParse(bantime, out temp);
                if (success)
                {
                    if (temp == 0)
                    {
                        infinity = 1;
                    }
                }
                else
                {
                    try
                    {
                        banTime = bantime.ParseTimeSpan();
                        endTime = endTime + banTime;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw new Exception("Указано неверное время бана");
                    }
                }
            }
            return (infinity, endTime);
        }

        public static async Task CheckPermissions(SocketGuildUser user, PermissionsEnumCommands permissions)
        {
            Guard.Argument(user, nameof(user)).NotNull();
            if (Program.Configuration.DiscordAdminRoleAccess && user.GuildPermissions.Administrator)
                return;

            if (ApplicationOwnerID == null)
            {
                switch (Client.TokenType)
                {
                    case TokenType.Bot:
                        var application = await Client.GetApplicationInfoAsync().ConfigureAwait(false);
                        ApplicationOwnerID = application.Owner.Id;
                        break;
                }
            }

            if (user.Id == ApplicationOwnerID)
                return;

            bool manage = false;
            foreach (var role in user.Roles)
            {
                if (Program.Configuration.DiscordManageRoleId.Contains(role.Id))
                {
                    manage = true;
                }
            }

            switch (permissions)
            {
                case PermissionsEnumCommands.Manage:
                    if (manage)
                        return;
                    break;
                case PermissionsEnumCommands.Restart:
                    if (manage)
                        return;
                    foreach (var role in user.Roles)
                    {
                        if (Program.Configuration.DiscordServerRestartRoleId.Contains(role.Id))
                        {
                            return;
                        }
                    }
                    break;
                case PermissionsEnumCommands.Ban:
                    if (manage || user.GuildPermissions.BanMembers)
                        return;
                    break;
                case PermissionsEnumCommands.UnBan:
                    if (manage || user.GuildPermissions.BanMembers)
                        return;
                    break;
                case PermissionsEnumCommands.Kick:
                    if (manage || user.GuildPermissions.BanMembers || user.GuildPermissions.KickMembers)
                        return;
                    break;
                case PermissionsEnumCommands.Zeus:
                    if (manage)
                        return;
                    break;
                case PermissionsEnumCommands.Infistar:
                    if (manage)
                        return;
                    break;
            }

            throw new UnauthorizedAccessException("Недостаточно прав для использования команды");
        }
    }
}
