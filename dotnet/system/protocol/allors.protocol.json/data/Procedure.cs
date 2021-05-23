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
        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("c")]
        public IDictionary<string, long[]> Collections { get; set; }

        [JsonPropertyName("o")]
        public IDictionary<string, long> Objects { get; set; }

        [JsonPropertyName("v")]
        public IDictionary<string, string> Values { get; set; }

        /// <summary>
        /// [][id,version]
        /// </summary>
        [JsonPropertyName("p")]
        public long[][] Pool { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitProcedure(this);
    }
}
