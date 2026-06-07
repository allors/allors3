// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using DataUtils;
    using Meta;

    public class MediaRule : Rule
    {
        public MediaRule(MetaPopulation m) : base(m, new Guid("436E574A-FE3E-46ED-8AD2-A59CACC2C9C4")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Media, m.Media.InType),
                new RolePattern(m.Media, m.Media.InData),
                new RolePattern(m.Media, m.Media.InDataUri),
                new RolePattern(m.Media, m.Media.InFileName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var media in matches.Cast<Media>())
            {
                var InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
                var InvalidFileNames = new[]
                {
                            "CON", "PRN", "AUX", "NUL", "COM", "LPT"
                        };

                media.Revision = Guid.NewGuid();

                byte[] data = null;
                string type = null;

                if (media.ExistInData)
                {
                    data = media.InData;
                    type = media.InType ?? MediaContents.Sniff(media.InData, media.InFileName);
                }
                else if (media.ExistInDataUri)
                {
                    var dataUrl = new DataUrl(media.InDataUri);

                    data = Convert.FromBase64String(dataUrl.ReadAsBase64EncodedString());
                    type = MediaContents.Sniff(data, media.InFileName);
                }

                // Consume every data input so a leftover role cannot re-trigger this rule (and reprocess) next cycle.
                media.RemoveInType();
                media.RemoveInData();
                media.RemoveInDataUri();

                if (data != null && data.Length == 0)
                {
                    // Reject an empty update without mutating: do not build a fresh content or discard the
                    // existing one, which would destroy data on an invalid update.
                    cycle.Validation.AddError("Media data may not be empty.");
                    continue;
                }

                if (data != null)
                {
                    // MediaContent is write-once: build a fresh one and discard the previous.
                    var previousMediaContent = media.MediaContent;

                    var mediaContent = MediaContents.Create(media.Strategy.Transaction);
                    mediaContent.Type = type;
                    mediaContent.Data = data;

                    media.MediaContent = mediaContent;

                    if (previousMediaContent != null)
                    {
                        previousMediaContent.CascadingDelete();
                    }
                }

                if (media.ExistInFileName)
                {
                    media.Name = System.IO.Path.GetFileNameWithoutExtension(media.InFileName);
                    media.RemoveInFileName();
                }

                media.Type = media.MediaContent?.Type;

                var name = !string.IsNullOrWhiteSpace(media.Name) ? media.Name : media.UniqueId.ToString();
                var fileName = $"{name}.{MediaContents.GetExtension(media.Type)}";
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
