// <copyright file="InvokeReadValidation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.InvokeRead
{
    using Allors.Protocol.Json.Api.Read;

    /// <summary>Validates raw trees before binding discards unsupported or missing members.</summary>
    public static class InvokeReadValidation
    {
        public static void ValidateRequest(object value) => ReadValidation.InvokeRequest(value);
        public static void ValidateReceipt(object value, object request) => ReadValidation.InvokeReceipt(value, request);
        public static void ValidateResponse(object value, object request) => ReadValidation.InvokeResponse(value, request);
    }
}
