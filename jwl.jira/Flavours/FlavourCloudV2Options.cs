namespace jwl.jira.Flavours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FlavourCloudV2Options : IFlavourOptions
{
    public string PluginBaseUri { get; init; } = @"rest/api/2";
}
