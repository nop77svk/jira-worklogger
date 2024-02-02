namespace jwl.wadl
{
    using System.Collections.Generic;
    using System.Net.Http;

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
