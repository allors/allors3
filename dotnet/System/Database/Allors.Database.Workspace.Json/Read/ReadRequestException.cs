// <copyright file="ReadRequestException.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;

    /// <summary>Actionable public request diagnostics authored by the protected reader.</summary>
    internal sealed class ReadRequestException : FormatException
    {
        internal ReadRequestException(string message) : base(message)
        {
        }

        internal ReadRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
