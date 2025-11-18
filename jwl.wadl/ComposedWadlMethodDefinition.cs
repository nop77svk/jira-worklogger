namespace jwl.Wadl
{
    using System.Collections.Generic;
    using System.Net.Http;

    public struct ComposedWadlMethodDefinition
    {
        public string ResourcePath { get; set; }
        public HttpMethod HttpMethod { get; set; }
        public IEnumerable<WadlParameter> Parameters { get; set; }
        public string? Id { get; set; }
        public WadlResourceRequest? Request { get; set; }
        public WadlResourceResponse? Response { get; set; }
    }
}
