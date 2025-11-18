namespace Jwl.Wadl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    public static class WadlApplicationExt
    {
        private const char UriPathFolderDelimiter = '/';

        public static IEnumerable<ComposedWadlMethodDefinition> AsComposedWadlMethodDefinitionEnumerable(this WadlApplication self)
            => self.Resources == null
            ? Enumerable.Empty<ComposedWadlMethodDefinition>()
            : FlattenWadlResources("/", Enumerable.Empty<WadlParameter>(), self.Resources);

        private static IEnumerable<ComposedWadlMethodDefinition> FlattenWadlResources(string parentPath, IEnumerable<WadlParameter> parentParameters, IEnumerable<WadlResource> resources)
        {
            foreach (WadlResource res in resources)
            {
                string resourcePath = parentPath.TrimEnd(UriPathFolderDelimiter) + UriPathFolderDelimiter + (res.Path?.Trim(UriPathFolderDelimiter) ?? string.Empty);

                IEnumerable<WadlParameter> resourceParameters = parentParameters
                    .Concat(res.Parameters ?? Enumerable.Empty<WadlParameter>());

                if (res.Methods != null)
                {
                    foreach (WadlResourceMethod method in res.Methods)
                    {
                        yield return new ComposedWadlMethodDefinition()
                        {
                            Id = method.Id,
                            ResourcePath = resourcePath,
                            HttpMethod = new HttpMethod(method.CallMethod),
                            Parameters = resourceParameters,
                            Request = method.Request,
                            Response = method.Response
                        };
                    }
                }

                if (res.Resources != null)
                {
                    foreach (ComposedWadlMethodDefinition resMethod in FlattenWadlResources(resourcePath, resourceParameters, res.Resources))
                    {
                        yield return resMethod;
                    }
                }
            }
        }
    }
}
