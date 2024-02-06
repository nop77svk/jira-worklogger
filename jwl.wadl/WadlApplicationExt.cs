﻿namespace jwl.wadl
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class WadlApplicationExt
    {
        public static IEnumerable<ComposedWadlMethodDefinition> AsComposedWadlMethodDefinitionEnumerable(this WadlApplication self)
        {
            if (self.Resources == null)
                return Enumerable.Empty<ComposedWadlMethodDefinition>();
            else
                return FlattenWadlResources("/", Enumerable.Empty<WadlParameter>(), self.Resources);
        }

        private static IEnumerable<ComposedWadlMethodDefinition> FlattenWadlResources(string parentPath, IEnumerable<WadlParameter> parentParameters, IEnumerable<WadlResource> resources)
        {
            foreach (WadlResource res in resources)
            {
                string resourcePath = parentPath.TrimEnd('/') + '/' + (res.Path?.Trim('/') ?? string.Empty);

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
                        yield return resMethod;
                }
            }
        }
    }
}
