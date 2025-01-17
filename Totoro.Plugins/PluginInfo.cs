﻿namespace Totoro.Plugins;

#nullable disable

public class PluginInfo
{
    public PluginInfo()
    {
    }

    public string Name { get; init; }
    public string DisplayName { get; init; }
    public Version Version { get; init; }
    public string Description { get; init; }
    public Stream Icon { get; init; }
}

public record PluginInfoSlim(string FileName, string Version);
