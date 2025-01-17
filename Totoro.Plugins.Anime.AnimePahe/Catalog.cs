﻿using System.Text.Json.Nodes;
using Flurl;
using Flurl.Http;
using Splat;
using Totoro.Plugins.Anime.Contracts;
using Totoro.Plugins.Contracts.Optional;

namespace Totoro.Plugins.Anime.AnimePahe;

internal class Catalog : IAnimeCatalog, IEnableLogger
{
    class SearchResult : ICatalogItem, IHaveSeason, IHaveImage, IHaveStatus
    {
        required public string Season { get; init; }
        required public string Status { get; init; }
        required public string Image { get; init; }
        required public string Year { get; init; }
        required public string Title { get; init; }
        required public string Url { get; init; }
    }

    public async IAsyncEnumerable<ICatalogItem> Search(string query)
    {
        var json = await Config.Url
            .AppendPathSegment("api")
            .SetQueryParams(new
            {
                m = "search",
                q = query,
                l = 8
            })
            .GetStringAsync();


        if (string.IsNullOrEmpty(json))
        {
            this.Log().Error("did not get a response");
            yield break;
        }

        var jObject = JsonNode.Parse(json);

        if (jObject is null)
        {
            this.Log().Error("unable to parse {Json}", json);
            yield break;
        }

        if (jObject["data"]?.AsArray() is not { } results)
        {
            this.Log().Error("there is not data");
            yield break;
        }

        var baseAnimeUrl = new Url(Config.Url).AppendPathSegment("anime");
        foreach (var item in results)
        {
            yield return new SearchResult
            {
                Title = $"{item?["title"]}",
                Image = $"{item?["poster"]}",
                Url = Url.Combine(baseAnimeUrl, $"{item?["session"]}"),
                Status = $"{item?["status"]}",
                Season = $"{item?["season"]}",
                Year = $"{item?["year"]}"
            };
        }

    }
}