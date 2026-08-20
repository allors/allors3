// <copyright file="StringEncoding.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database
{
    /// <summary>
    /// The encoding used for string unit roles in a serialized population.
    /// </summary>
    public enum StringEncoding
    {
        /// <summary>
        /// String unit roles are stored as raw xml text.
        /// Used by populations saved before 2023-07-05.
        /// </summary>
        Raw,

        /// <summary>
        /// String unit roles are stored as the Base64 representation of their UTF-8 bytes.
        /// This is the only encoding used from serialization version 2 onwards.
        /// </summary>
        Base64,
    }
}
