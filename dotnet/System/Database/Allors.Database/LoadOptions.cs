// <copyright file="LoadOptions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database
{
    using System.Xml;

    /// <summary>
    /// Options for <see cref="IDatabase.Load(XmlReader, LoadOptions)"/>.
    /// </summary>
    public sealed class LoadOptions
    {
        /// <summary>
        /// Gets the encoding of the string unit roles in a version 1 population.
        /// </summary>
        /// <remarks>
        /// Serialization version 1 is ambiguous. Populations saved before 2023-07-05 store string
        /// unit roles as raw xml text, later ones store them Base64 encoded. Both declare version 1,
        /// so the encoding can not be derived from the document and has to be supplied by the caller.
        /// Loading a version 1 population without this option throws an <see cref="System.ArgumentException"/>.
        /// Ignored from version 2 onwards, where the encoding is always <see cref="StringEncoding.Base64"/>.
        /// </remarks>
        public StringEncoding? Version1StringEncoding { get; init; }
    }
}
