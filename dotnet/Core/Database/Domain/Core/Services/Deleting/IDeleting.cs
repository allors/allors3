// <copyright file="ICaches.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public interface IDeleting
    {
        void OnBeginDelete(long id);

        void OnEndDelete(long id);

        bool IsDeleting(long id);
    }
}
