// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Meta;
    using Protocol.Database.Push;

    public interface IDatabaseStrategy : IStrategy
    {
        void Reset();

        void Refresh(bool merge = false);

        object GetAssociationForDatabase(IRoleType roleType);

        DatabaseObject DatabaseObject { get; }

        PushRequestObject SaveExisting();
      }
}
