#pragma warning disable S1104
namespace Jwl.Wadl
{
    using System.Collections.Generic;
    using System.Net.Http;

    public struct ComposedWadlMethodDefinition
    {
        public string ResourcePath;
        public HttpMethod HttpMethod;
        public IEnumerable<WadlParameter> Parameters;
        public string? Id;
        public WadlResourceRequest? Request;
        public WadlResourceResponse? Response;
    }
}
