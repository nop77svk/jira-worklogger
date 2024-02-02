using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace jwl.jira.ictime
{
    public struct ComposedResourceMethod
    {
        public string ResourcePath;
        public HttpMethod HttpMethod;
        public IEnumerable<WadlResourceParameter> ResourceParameters;
        public string? Id;
        public WadlResourceRequest? Request;
        public WadlResourceResponse? Response;
    }
}
