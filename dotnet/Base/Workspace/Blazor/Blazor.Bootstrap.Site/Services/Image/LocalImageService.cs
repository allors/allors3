namespace Allors.Services
{
    using System.Collections.Generic;
    using Allors.Workspace.Domain;
    using Workspace.Blazor;

    public partial class LocalImageService : IImageService
    {
        private readonly LocalImageServiceConfig? config;

        public LocalImageService(LocalImageServiceConfig config = null) => this.config = config;

        public string Source(Media media, int? width = null, int? quality = null, string type = null, string background = "FFF")
        {
            var parameters = new List<string>();
            if (width != null)
            {
                parameters.Add($"w={width}");
            }

            if (quality != null)
            {
                parameters.Add($"q={quality}");
            }

            if (type != null)
            {
                parameters.Add($"t={type}");
            }

            if (!"png".Equals(type) && "image/png".Equals(media.Type))
            {
                parameters.Add($"b={background}");
            }

            return $"{this.config?.Url}/allors/image/{media.UniqueId}/{media.Revision}{(parameters.Count > 0 ? "?" : string.Empty)}{string.Join("&", parameters)}";
        }
    }
}
