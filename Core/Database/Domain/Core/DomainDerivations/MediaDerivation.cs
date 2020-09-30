// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using DataUtils;
    using HeyRed.Mime;
    using Meta;

    public class MediaDerivation : DomainDerivation
    {
        public MediaDerivation(M m) : base(m, new Guid("436E574A-FE3E-46ED-8AD2-A59CACC2C9C4")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.Media.Class),
                new ChangedRolePattern(m.Media.InType),
                new ChangedRolePattern(m.Media.InData),
                new ChangedRolePattern(m.Media.InDataUri),
                new ChangedRolePattern(m.Media.InFileName),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var media in matches.Cast<Media>())
            {
                var InvalidFileNameChars = Path.GetInvalidFileNameChars();
                var InvalidFileNames = new[]
                {
                            "CON", "PRN", "AUX", "NUL", "COM", "LPT"
                        };

                media.Revision = Guid.NewGuid();

                if (media.ExistInData || media.ExistInDataUri)
                {
                    if (!media.ExistMediaContent)
                    {
                        media.MediaContent = new MediaContentBuilder(media.Strategy.Session).Build();
                    }
                }

                if (media.ExistInData)
                {
                    media.MediaContent.Data = media.InData;
                    media.MediaContent.Type = media.InType ?? MediaContents.Sniff(media.InData, media.InFileName);

                    media.RemoveInType();
                    media.RemoveInData();
                }

                if (media.ExistInFileName)
                {
                    media.Name = Path.GetFileNameWithoutExtension(media.InFileName);
                    media.RemoveInFileName();
                }

                if (media.ExistInDataUri)
                {
                    var dataUrl = new DataUrl(media.InDataUri);

                    media.MediaContent.Data = Convert.FromBase64String(dataUrl.ReadAsBase64EncodedString());
                    media.MediaContent.Type = dataUrl.ContentType;

                    media.RemoveInDataUri();
                }

                media.Type = media.MediaContent?.Type;

                var name = !string.IsNullOrWhiteSpace(media.Name) ? media.Name : media.UniqueId.ToString();
                var fileName = $"{name}.{MimeTypesMap.GetExtension(media.Type)}";
                var safeFileName = new string(fileName.Where(ch => !InvalidFileNameChars.Contains(ch)).ToArray());

                var uppercaseSafeFileName = safeFileName.ToUpperInvariant();
                if (InvalidFileNames.Any(invalidFileName => uppercaseSafeFileName.StartsWith(invalidFileName)))
                {
                    safeFileName += "_" + safeFileName;
                }

                media.FileName = safeFileName;
            }
        }
    }
}
