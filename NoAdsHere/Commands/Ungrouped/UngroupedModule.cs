﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using MongoDB.Driver;
using NoAdsHere.Common;
using NoAdsHere.Common.Preconditions;
using NoAdsHere.Database;
using NoAdsHere.Database.Models.GuildSettings;
using NoAdsHere.Database.Models.Violator;
using NoAdsHere.Services.Configuration;
using NoAdsHere.Services.Violations;

namespace NoAdsHere.Commands.Ungrouped
{
    [Name("Not Grouped")]
    public class UngroupedModule : ModuleBase
    {
        private readonly MongoClient _mongo;
        private readonly Config _config;

        public UngroupedModule(MongoClient mongo, Config config)
        {
            _mongo = mongo;
            _config = config;
        }

        [Command("Github")]
        [RequirePermission(AccessLevel.User)]
        public async Task Github()
        {
            await ReplyAsync(
                "You can find my source-code & Invite here: https://github.com/Nanabell/NoAdsHere\nPlease visit the Wiki tab for documentations.");
        }

        [Command("Documentation"), Alias("Docs")]
        [RequirePermission(AccessLevel.User)]
        public async Task Docs()
        {
            await ReplyAsync(
                "Documentation for NAH can be found on the Github-Wiki pages!\nhttps://github.com/Nanabell/NoAdsHere/wiki");
        }

        [Command("My Points")]
        [RequirePermission(AccessLevel.User)]
        public async Task My_Points()
        {
            var violator = await _mongo.GetCollection<Violator>(Context.Client).GetUserAsync(Context.Guild.Id, Context.User.Id);
            var penalties = await _mongo.GetCollection<Penalty>(Context.Client).GetPenaltiesAsync(Context.Guild.Id);

            violator = await Violations.TryDecreasePoints(Context, violator);

            var nextPenalty = penalties.OrderBy(p => p.RequiredPoints).FirstOrDefault(penalty => penalty.RequiredPoints > violator.Points);

            var until = TimeSpan.Zero;
            if (violator.Points > 0)
                until = violator.LatestViolation.AddHours(_config.PointDecreaseHours) - DateTime.UtcNow;
            await ReplyAsync(
                // ReSharper disable once UseFormatSpecifierInInterpolation
                $"You currently have {violator.Points} points. {(until != TimeSpan.Zero ? $"You will lose one point in {until.ToString(@"hh'h'\:mm'm'\:ss's'")}" : "")}" +
                $"{(nextPenalty != null ? $"\nThe next Penalty*({nextPenalty.PenaltyType})* is at {nextPenalty.RequiredPoints} points" : "")}");
        }
    }
}