// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Allors.Protocol.Json.Api.Push;
    using Meta;

    public sealed class Strategy : Adapters.Strategy
    {
        internal Strategy(Session session, IClass @class, long id) : base(session, @class, id)
        {
            if (this.Class.HasDatabaseOrigin)
            {
                this.DatabaseOriginState = new DatabaseOriginState(this, (DatabaseRecord)((DatabaseConnection)((Adapters.Session) session).Workspace.Database).GetRecord(this.Id));
            }
        }

        internal Strategy(Session session, DatabaseRecord databaseRecord) : base(session, databaseRecord) => this.DatabaseOriginState = new DatabaseOriginState(this, databaseRecord);

        public new Session Session => (Session)base.Session;

        internal PushRequestNewObject DatabasePushNew() => ((DatabaseOriginState)this.DatabaseOriginState).PushNew();

        internal PushRequestObject DatabasePushExisting() => ((DatabaseOriginState)this.DatabaseOriginState).PushExisting();
    }
}
