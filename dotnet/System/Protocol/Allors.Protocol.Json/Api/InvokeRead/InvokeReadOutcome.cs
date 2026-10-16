// <copyright file="InvokeReadOutcome.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.InvokeRead
{
    using Allors.Protocol.Json.Api.Read;

    /// <summary>A mutation receipt without local-edit acknowledgements or new-object mappings.</summary>
    public class InvokeReadOutcome
    {
        public string k { get; set; }
        public ReadError[] e { get; set; }
        public string _e { get; set; }
        public long[] _v { get; set; }
        public long[] _a { get; set; }
        public long[] _m { get; set; }
        public ResponseDerivationError[] _d { get; set; }
    }
}
