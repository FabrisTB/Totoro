﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.AspNetCore.WebUtilities;
using Totoro.Core.Helpers;

namespace Totoro.Core.Services;

public class TimestampsService : ITimestampsService
{

    private readonly GraphQLHttpClient _animeSkipClient = new("https://api.anime-skip.com/graphql", new SystemTextJsonSerializer());
    private readonly IAnimeIdService _animeIdService;
    private readonly Dictionary<long, List<OfflineEpisodeTimeStamp>> _offlineTimestamps;

    public TimestampsService(IAnimeIdService animeIdService,
                             IFileService fileService)
    {
        _animeIdService = animeIdService;
        _animeSkipClient.HttpClient.DefaultRequestHeaders.Add("X-Client-ID", "ZGfO0sMF3eCwLYf8yMSCJjlynwNGRXWE");

        _offlineTimestamps = fileService.Read<Dictionary<long, List<OfflineEpisodeTimeStamp>>>("", "timestamps_generated.json");
    }

    public async Task<AnimeTimeStamps> GetTimeStamps(long malId)
    {
        var animeTimeStamps = new AnimeTimeStamps();

        try
        {
            var id = await _animeIdService.GetId(AnimeTrackerType.MyAnimeList, malId);
            if (_offlineTimestamps.TryGetValue(id.AniDb, out List<OfflineEpisodeTimeStamp> value))
            {
                foreach (var item in value)
                {
                    animeTimeStamps.EpisodeTimeStamps.Add(item.Episode.ToString(), item);
                }

                return animeTimeStamps;
            }
            else
            {
                var animeSkipRequest = new GraphQLRequest
                {
                    Query = GraphQLQueries.GetTimeStamps(),
                    Variables = new { serviceId = id.AniList.ToString() }
                };

                var showResponse = await _animeSkipClient.SendQueryAsync<ShowResponse>(animeSkipRequest);

                if(showResponse is not { Data.Shows.Length: > 0})
                {
                    return animeTimeStamps;
                }

                foreach (var item in showResponse.Data.Shows[0].Episodes)
                {
                    try
                    {
                        var intro = item.TimeStamps.FirstOrDefault(x => x.Type.Description.ToLower().Contains("intro"))?.Time ?? -1.0;
                        var outro = item.TimeStamps.FirstOrDefault(x => x.Type.Description.ToLower().Contains("outro"))?.Time ?? -1.0;
                        animeTimeStamps.EpisodeTimeStamps.Add(item.EpisodeNumber, new EpisodeTimeStamp { Intro = intro, Outro = outro });
                    }
                    catch { }
                }

                return animeTimeStamps;
            }
        }
        catch { }
        return animeTimeStamps;
    }
}

public class AnimeTimeStamps
{
    public Dictionary<string, EpisodeTimeStamp> EpisodeTimeStamps { get; set; } = new();

    public double GetIntroStartPosition(string episode)
    {
        if (EpisodeTimeStamps.TryGetValue(episode, out EpisodeTimeStamp value))
        {
            return value.Intro;
        }

        return -1.0;
    }

    public TimeSpan GetIntroEndPosition(string episode)
    {
        return TimeSpan.FromSeconds(GetIntroStartPosition(episode)) + TimeSpan.FromSeconds(89);
    }

    public double GetOutroStartPosition(string episode)
    {
        if (EpisodeTimeStamps.TryGetValue(episode, out EpisodeTimeStamp value))
        {
            return value.Outro;
        }

        return -1.0;
    }
}

[ExcludeFromCodeCoverage]
public class EpisodeTimeStamp
{

    [JsonPropertyName("opening_start")]
    public double Intro { get; set; }

    [JsonPropertyName("ending_start")]
    public double Outro { get; set; }
}

[ExcludeFromCodeCoverage]
public class OfflineEpisodeTimeStamp : EpisodeTimeStamp
{
    [JsonPropertyName("episode_number")]
    public int Episode { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }
}

