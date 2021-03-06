﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NoAdsHere.Database;
using NoAdsHere.Database.Models.Global;

namespace NoAdsHere.Common.Preconditions
{
    public class RequirePermissionAttribute : PreconditionAttribute
    {
        private readonly AccessLevel _level;

        public RequirePermissionAttribute(AccessLevel level)
        {
            _level = level;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var level = await GetLevel(context, services.GetService<MongoClient>()).ConfigureAwait(false);

            return level >= _level ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"Insufficient permissions! Required level: {_level}");
        }

        private static async Task<AccessLevel> GetLevel(ICommandContext context, MongoClient mongo)
        {
            if (context.User.IsBot)
                return AccessLevel.Blocked;

            var application = await context.Client.GetApplicationInfoAsync();
            if (application.Owner.Id == context.User.Id)
                return AccessLevel.God;

            var masters = await mongo.GetCollection<Master>(context.Client).GetMastersAsync();
            if (masters.Any(master => master.UserId == context.User.Id))
                return AccessLevel.Master;

            if (!(context.User is IGuildUser guildUser)) return AccessLevel.Private;
            
            if (context.Guild.OwnerId == context.User.Id)
                return AccessLevel.Owner;
            
            if (guildUser.GuildPermissions.Administrator)
                return AccessLevel.Admin;

            if (guildUser.GuildPermissions.BanMembers)
                return AccessLevel.HighModerator;

            return guildUser.GuildPermissions.ManageMessages ? AccessLevel.Moderator : AccessLevel.User;
        }
    }
}