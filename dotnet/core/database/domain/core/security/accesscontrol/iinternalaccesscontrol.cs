// <copyright file="AccessControlListFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Database.Security;

    public interface IInternalAccessControl : IAccessControl
    {
        Grant[] Filter(IEnumerable<Grant> unfilteredGrants);

        Revocation[] Filter(IEnumerable<Revocation> unfilteredRevocations);
    }
}
