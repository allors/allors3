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
        private readonly ITransaction transaction;
        private readonly IMetaPopulation metaPopulation;

        public ToDatabaseVisitor(Database.ITransaction transaction)
        {
            this.transaction = transaction;
            this.metaPopulation = transaction.Database.MetaPopulation;
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

        private Database.Data.IExtent Visit(Data.IExtent ws) =>
            ws switch
            {
                Data.Extent extent => this.Visit(extent),
                Data.Except except => this.Visit(except),
                Data.Intersect intersect => this.Visit(intersect),
                Data.Union union => this.Visit(union),
                null => null,
                _ => throw new Exception($"Unknown implementation of IExtent: {ws.GetType()}")
            };

        private Database.Data.Extent Visit(Data.Extent ws) => new Database.Data.Extent((IComposite)this.Visit(ws.ObjectType));

        private Database.Data.Except Visit(Data.Except ws) => new Database.Data.Except();

        private Database.Data.Intersect Visit(Data.Intersect ws) => new Database.Data.Intersect();

        private Database.Data.Union Visit(Data.Union ws) => new Database.Data.Union();

        private Database.Meta.IObjectType Visit(Meta.IObjectType ws) => ws != null ? (IObjectType)this.metaPopulation.Find(ws.Id) : null;

        private Database.IObject Visit(IDatabaseObject ws)
        {
            if (ws != null)
            {
                throw new System.NotImplementedException();
            }

            return null;
        }

        private Database.Data.Result[] Visit(Data.Result[] ws)
        {
            if (ws != null)
            {
                throw new System.NotImplementedException();
            }

            return null;
        }

        private IDictionary<string, string> Visit(IDictionary<string, string> ws)
        {
            if (ws != null)
            {
                throw new System.NotImplementedException();
            }

            return null;
        }
    }
}
