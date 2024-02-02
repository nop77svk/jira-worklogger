using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;

namespace jwl.jira.ictime
{
    public static class WadlApplicationExt
    {
        public static IEnumerable<ComposedResourceMethod> AsEnumerable(this WadlApplication self)
        {
            if (self.Resources == null)
                return Enumerable.Empty<ComposedResourceMethod>();
            else
                return FlattenWadlResources("/", Enumerable.Empty<WadlResourceParameter>(), self.Resources);
        }

        private static IEnumerable<ComposedResourceMethod> FlattenWadlResources(string parentPath, IEnumerable<WadlResourceParameter> parentParameters, IEnumerable<WadlResource> resources)
        {
            foreach (WadlResource res in resources)
            {
                string resourcePath = parentPath.TrimEnd('/') + '/' + (res.Path?.Trim('/') ?? string.Empty);

                IEnumerable<WadlResourceParameter> resourceParameters = parentParameters
                    .Concat(res.Parameters ?? Enumerable.Empty<WadlResourceParameter>());

                if (res.Methods != null)
                {
                    foreach (WadlResourceMethod method in res.Methods)
                    {
                        yield return new ComposedResourceMethod()
                        {
                            Id = method.Id,
                            ResourcePath = resourcePath,
                            HttpMethod = new HttpMethod(method.CallMethod),
                            ResourceParameters = resourceParameters,
                            Request = method.Request,
                            Response = method.Response
                        };
                    }
                }

                if (res.Resources != null)
                {
                    foreach (ComposedResourceMethod resMethod in FlattenWadlResources(resourcePath, resourceParameters, res.Resources))
                        yield return resMethod;
                }
            }
        }
    }
}
