// <copyright file="IMediaContentStorage.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public interface IMediaContentStorage
    {
        bool Exists(long id);

        byte[] Read(long id);

        void Write(long id, byte[] data);

        void Delete(long id);
    }
}
