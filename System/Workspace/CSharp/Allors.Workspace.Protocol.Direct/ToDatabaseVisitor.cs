// <copyright file="ToJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Protocol.Direct
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Database.Data;
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

        public Database.Data.Procedure Visit(Data.Procedure ws) =>
            new Database.Data.Procedure(ws.Name)
            {
                Collections = ws.Collections?.ToDictionary(v => v.Key, v => this.transaction.Instantiate(v.Value?.Select(w => w.Id))),
                Objects = ws.Objects?.ToDictionary(v => v.Key, v => v.Value != null ? this.transaction.Instantiate(v.Value.Id) : null),
                Values = ws.Values,
                Pool = ws.Pool?.ToDictionary(v => this.transaction.Instantiate(v.Key.Id), v => v.Value),
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

        private Database.Meta.IObjectType Visit(Meta.IObjectType ws) => ws != null ? (IObjectType)this.metaPopulation.FindByTag(ws.Tag) : null;

        private Database.IObject Visit(Workspace.IObject ws) => ws != null ? this.transaction.Instantiate(ws.Id) : null;

        private Database.Data.Result[] Visit(Data.Result[] ws) =>
            ws?.Select(v => new Database.Data.Result
            {
                Name = v.Name,
                Select = this.Visit(v.Select),
                SelectRef = v.SelectRef,
                Skip = v.Skip,
                Take = v.Take,
            }).ToArray();

        private Select Visit(Data.Select ws) => ws != null ? new Select { Include = this.Visit(ws.Include), Step = this.Visit(ws.Step), } : null;

        private Node[] Visit(IEnumerable<Data.Node> ws) => ws?.Select(this.Visit).ToArray();

        private Node Visit(Data.Node ws) => ws != null ? new Node(this.Visit(ws.PropertyType), ws.Nodes?.Select(this.Visit).ToArray()) : null;

        private Step Visit(Data.Step ws)
        {
            if (ws != null)
            {
                return new Step
                {
                    PropertyType = this.Visit(ws.PropertyType),
                    Include = this.Visit(ws.Include),
                };
            }

            return null;
        }

        private IPropertyType Visit(Meta.IPropertyType ws)
        {
            switch (ws)
            {
                case Meta.IAssociationType associationType:
                    return ((IRelationType)this.metaPopulation.FindByTag(associationType.OperandTag)).AssociationType;

                case Meta.IRoleType roleType:
                    return ((IRelationType)this.metaPopulation.FindByTag(roleType.OperandTag)).RoleType;

                default:
                    throw new ArgumentException("Invalid property type");
            }
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
