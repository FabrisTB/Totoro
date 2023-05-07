﻿using AnitomySharp;
using MonoTorrent;

namespace Totoro.Core.Services.StreamResolvers;

public class VideoStreamResolverFactory : IVideoStreamResolverFactory
{
    private readonly IProviderFactory _providerFactory;
    private readonly ISettings _settings;
    private readonly IDebridServiceContext _debridService;
    private readonly IKnownFolders _knownFolders;
    private readonly ITorrentEngine _torrentEngine;

    public VideoStreamResolverFactory(IProviderFactory providerFactory,
                                      ISettings settings,
                                      IDebridServiceContext debridService,
                                      IKnownFolders knownFolders,
                                      ITorrentEngine torrentEngine)
    {
        _providerFactory = providerFactory;
        _settings = settings;
        _debridService = debridService;
        _knownFolders = knownFolders;
        _torrentEngine = torrentEngine;
    }

    public IVideoStreamModelResolver CreateAnimDLResolver(string providerType, string baseUrl)
    {
        return new AnimDLVideoStreamResolver(_providerFactory.GetProvider(providerType), _settings, baseUrl);
    }

    public IVideoStreamModelResolver CreateGogoAnimDLResolver(string providerType, string baseUrlSub, string baseUrlDub)
    {
        return new AnimDLVideoStreamResolver(_providerFactory.GetProvider(providerType), _settings, baseUrlSub, baseUrlDub);
    }

    public async Task<IVideoStreamModelResolver> CreateDebridStreamResolver(string magnet)
    {
        var resolver = new DebridStreamModelResolver(_debridService, magnet);
        await resolver.Populate();
        return resolver;
    }

    public IVideoStreamModelResolver CreateWebTorrentStreamResolver(IEnumerable<Element> parsedResults, string magnet)
    {
        return new WebTorrentStreamModelResolver(_knownFolders, parsedResults, magnet);
    }

    public IVideoStreamModelResolver CreateMonoTorrentStreamResolver(IEnumerable<Element> parsedResults, string magnet)
    {
        return new MonoTorrentStreamModelResolver(_torrentEngine, parsedResults, magnet, _settings.UserTorrentsDownloadDirectory);
    }

    public IVideoStreamModelResolver CreateMonoTorrentStreamResolver(Torrent torrent)
    {
        return new MonoTorrentStreamModelResolver(_torrentEngine, torrent, _settings.UserTorrentsDownloadDirectory);
    }

    public IVideoStreamModelResolver CreateLocalStreamResolver(string directory)
    {
        return new FileSystemStreamResolver(directory);
    }
}

