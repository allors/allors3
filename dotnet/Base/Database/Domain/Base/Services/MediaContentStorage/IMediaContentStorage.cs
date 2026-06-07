// <copyright file="IMediaContentStorage.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;

    public interface IMediaContentStorage
    {
        bool Exists(long id);

        /// <summary>
        /// The byte length of the stored item, or -1 when it does not exist. Lets callers test presence and
        /// emptiness without reading the bytes.
        /// </summary>
        long Length(long id);

        byte[] Read(long id);

        void Write(long id, byte[] data);

        void Delete(long id);

        /// <summary>
        /// Enumerates the object id of every stored item. Used by <see cref="ExternalMediaContents.ReconcileFiles"/>
        /// to find files that no live content owns.
        /// </summary>
        IEnumerable<long> Enumerate();
    }
}
