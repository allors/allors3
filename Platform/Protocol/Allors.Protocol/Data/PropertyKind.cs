// <copyright file="PredicateKind.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Data
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PropertyKind
    {
        [EnumMember(Value = "A")]
        Association = 1,

        [EnumMember(Value = "R")]
        Role = 2,
    }
}
