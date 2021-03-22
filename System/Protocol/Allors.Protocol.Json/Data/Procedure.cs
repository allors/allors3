// <copyright file="Pull.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class Procedure : IVisitable
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("namedCollections")]
        public string[][] CollectionByName { get; set; }

        [JsonPropertyName("namedObjects")]
        public string[][] ObjectByName { get; set; }

        [JsonPropertyName("namedValues")]
        public string[][] ValueByName { get; set; }

        [JsonPropertyName("objectVersions")]
        public string[][] VersionByObject { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitProcedure(this);
    }
}
