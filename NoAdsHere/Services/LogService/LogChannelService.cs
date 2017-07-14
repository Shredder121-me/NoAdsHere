﻿using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using NoAdsHere.Services.Configuration;
using NoAdsHere.Services.Events;

namespace NoAdsHere.Services.LogService
{
    public class LogChannelService
    {
        private bool IsEnabled;
        private readonly DiscordWebhookClient _client;

        public LogChannelService(Config config)
        {
            if (config.WebHookLogger.Token == null || config.WebHookLogger.Id == 0)
                IsEnabled = false;
            else IsEnabled = true;

            _client = new DiscordWebhookClient(config.WebHookLogger.Id, config.WebHookLogger.Token);
            _client.Log += EventHandlers.WebhookLogger;
        }

        internal async Task SendMessageAsync(string message)
        {
            if (IsEnabled)
                await _client.SendMessageAsync(message);
        }

        internal async Task LogMessageAsync(DiscordShardedClient client, DiscordSocketClient shard, Emote emote,
            string message)
        {
            if (IsEnabled)
                await _client.SendMessageAsync($"`[{shard.ShardId:00} / {client.Shards.Count:00}]` " +
                    $"<:{emote.Name}:{emote.Id}> " +
                    $"{message}", false, null, shard.CurrentUser.Username,
                    shard.CurrentUser.GetAvatarUrl(ImageFormat.Png));
        }
    }
}