// <copyright file="ICaches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
