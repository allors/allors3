// <copyright file="PushReadOutcome.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.PushRead
{
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.Api.Push;

    public class PushReadOutcome
    {
        /// <summary>
        /// Mutation outcome: committed, rejected, or unknown.
        /// </summary>
        public string k { get; set; }

        /// <summary>
        /// Committed mappings for every submitted new object, even if subsequently deleted.
        /// </summary>
        public PushResponseNewObject[] n { get; set; }

        /// <summary>
        /// Actionable diagnostics; a committed outcome remains committed.
        /// </summary>
        public ReadError[] e { get; set; }

        /// <summary>
        /// Legacy Push error message.
        /// </summary>
        public string _e { get; set; }

        /// <summary>
        /// Legacy Push version errors.
        /// </summary>
        public long[] _v { get; set; }

        /// <summary>
        /// Legacy Push access errors.
        /// </summary>
        public long[] _a { get; set; }

        /// <summary>
        /// Legacy Push missing-object errors.
        /// </summary>
        public long[] _m { get; set; }

        /// <summary>
        /// Legacy Push derivation errors.
        /// </summary>
        public ResponseDerivationError[] _d { get; set; }
    }
}
