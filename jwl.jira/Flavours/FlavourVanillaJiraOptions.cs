﻿namespace jwl.jira.Flavours;

public class FlavourVanillaJiraOptions
    : IFlavourOptions
{
    public string PluginBaseUri { get; init; } = @"rest/api/2";
}
