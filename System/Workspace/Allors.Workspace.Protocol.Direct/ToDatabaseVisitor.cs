// <copyright file="ToJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Protocol.Direct
{
    using System;
    using System.Collections.Generic;
    using Database;
    using Database.Meta;

    public class ToDatabaseVisitor
    {
        private readonly ISession session;
        private readonly IMetaPopulation metaPopulation;

        public ToDatabaseVisitor(Database.ISession session)
        {
            this.session = session;
            this.metaPopulation = session.Database.MetaPopulation;
        }

        public Database.Data.Pull Visit(Data.Pull ws) =>
            new Database.Data.Pull
            {
                ExtentRef = ws.ExtentRef,
                Extent = this.Visit(ws.Extent),
                ObjectType = this.Visit(ws.ObjectType),
                Object = this.Visit(ws.Object),
                Results = this.Visit(ws.Results),
                Parameters = this.Visit(ws.Parameters)
            };

        private Database.Data.IExtent Visit(Data.IExtent ws)
        {
            switch (ws)
            {
                case Data.Extent extent:
                    return this.Visit(extent);
                case Data.Except except:
                    return this.Visit(except);
                case Data.Intersect intersect:
                    return this.Visit(intersect);
                case Data.Union union:
                    return this.Visit(union);
            }

            throw new Exception($"Unknown implementation of IExtent: {ws.GetType()}");
        }

        private Database.Data.Extent Visit(Data.Extent ws) => new Database.Data.Extent((IComposite)Visit(ws.ObjectType));

        private Database.Data.Except Visit(Data.Except ws) => new Database.Data.Except();

        private Database.Data.Intersect Visit(Data.Intersect ws) => new Database.Data.Intersect();

        private Database.Data.Union Visit(Data.Union ws) => new Database.Data.Union();

        private Database.Meta.IObjectType Visit(Meta.IObjectType ws) => (IObjectType)this.metaPopulation.Find(ws.Id);

        private Database.IObject Visit(IDatabaseObject ws) => throw new System.NotImplementedException();

        private Database.Data.Result[] Visit(Data.Result[] ws) => throw new System.NotImplementedException();

        private IDictionary<string, string> Visit(IDictionary<string, string> ws) => throw new System.NotImplementedException();
    }
}
