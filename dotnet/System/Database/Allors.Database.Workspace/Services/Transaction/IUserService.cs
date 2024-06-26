// <copyright file="IUserService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Services
{
    using System;
    using Security;

    public interface IUserService
    {
        event EventHandler UserChanged;

        IUser User { get; set; }
    }
}
