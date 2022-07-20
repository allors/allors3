// <copyright file="ISessionExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain
{
    using System;
    using Workspace;

    public static partial class ISessionExtensions
    {
        public static DateTime Now(this ISession transaction)
        {
            var now = DateTime.UtcNow;

            var timeService = ((IWorkspaceServices)transaction.Workspace.Services).Get<ITime>();
            var timeShift = timeService.Shift;
            if (timeShift != null)
            {
                now = now.Add((TimeSpan)timeShift);
            }

            return now;
        }
    }
}
