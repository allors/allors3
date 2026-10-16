// <copyright file="PushReadValidation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.PushRead
{
    using Allors.Protocol.Json.Api.Read;

    /// <summary>Validates raw PushRead trees before binding discards unsupported or missing fields.</summary>
    public static class PushReadValidation
    {
        public static void ValidateRequest(object value) => ReadValidation.PushRequest(value);

        public static void ValidateReceipt(object value, object request) => ReadValidation.PushReceipt(value, request);

        public static void ValidateResponse(object value, object request) => ReadValidation.PushResponse(value, request);
    }
}
