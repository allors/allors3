// <copyright file="ReadRecord.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadRecord : ReadRecordHeader
    {
        /// <summary>
        /// Complete readable roles or a reuse reference to the request's record inventory.
        /// </summary>
        public ReadRecordPayload p { get; set; }
    }
}
