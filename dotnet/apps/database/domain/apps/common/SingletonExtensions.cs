// <copyright file="SingletonExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static partial class SingletonExtensions
    {
        public static Template CreateOpenDocumentTemplate<T>(this Singleton @this, string fileName, byte[] content)
        {
            var media = new MediaBuilder(@this.Strategy.Transaction).WithInFileName(fileName).WithInData(content).Build();
            var templateType = new TemplateTypes(@this.Strategy.Transaction).OpenDocumentType;
            var template = new TemplateBuilder(@this.Strategy.Transaction).WithMedia(media).WithTemplateType(templateType).WithArguments<T>().Build();
            return template;
        }

        public static byte[] GetResourceBytes(this Singleton @this, string name)
        {
            var assembly = @this.GetType().GetTypeInfo().Assembly;
            var manifestResourceName = assembly.GetManifestResourceNames().First(v => v.Contains(name));
            var resource = assembly.GetManifestResourceStream(manifestResourceName);
            if (resource != null)
            {
                using (var ms = new MemoryStream())
                {
                    resource.CopyTo(ms);
                    return ms.ToArray();
                }
            }

            return null;
        }
    }
}
