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
    using Sort = Database.Sort;

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
                Arguments = this.Visit(ws.Arguments)
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

        private Database.Data.Extent Visit(Data.Extent ws) => new Database.Data.Extent((IComposite)this.Visit(ws.ObjectType))
        {
            Predicate = this.Visit(ws.Predicate)
        };

        private IPredicate Visit(Data.IPredicate ws) =>
            ws switch
            {
                Data.And and => this.Visit(and),
                Data.Between between => this.Visit(between),
                Data.ContainedIn containedIn => this.Visit(containedIn),
                Data.Contains contains => this.Visit(contains),
                Data.Equals equals => this.Visit(equals),
                Data.Exists exists => this.Visit(exists),
                Data.GreaterThan greaterThan => this.Visit(greaterThan),
                Data.Instanceof instanceOf => this.Visit(instanceOf),
                Data.LessThan lessThan => this.Visit(lessThan),
                Data.Like like => this.Visit(like),
                Data.Not not => this.Visit(not),
                Data.Or or => this.Visit(or),
                null => null,
                _ => throw new Exception($"Unknown implementation of IExtent: {ws.GetType()}")
            };

        private IPredicate Visit(Data.And ws) => new And(ws.Operands?.Select(this.Visit).ToArray())
        {
            Dependencies = ws.Dependencies,
        };

        private IPredicate Visit(Data.Between ws) => new Between(this.Visit(ws.RoleType))
        {
            Dependencies = ws.Dependencies,
            Parameter = ws.Parameter,
            Values = ws.Values,
        };

        private IPredicate Visit(Data.ContainedIn ws) => new ContainedIn(this.Visit(ws.PropertyType))
        {
            Dependencies = ws.Dependencies,
            Parameter = ws.Parameter,
            Objects = this.Visit(ws.Objects),
            Extent = this.Visit(ws.Extent),
        };

        private IPredicate Visit(Data.Contains ws) => new Contains(this.Visit(ws.PropertyType))
        {
            Dependencies = ws.Dependencies,
            Parameter = ws.Parameter,
            Object = this.Visit(ws.Object)
        };

        private IPredicate Visit(Data.Equals ws) => new Equals(this.Visit(ws.PropertyType))
        {
            Dependencies = ws.Dependencies,
            Parameter = ws.Parameter,
            Object = this.Visit(ws.Object),
            Value = ws.Value,
        };

        private IPredicate Visit(Data.Exists ws) => new Exists(this.Visit(ws.PropertyType))
        {
            Dependencies = ws.Dependencies,
            Parameter = ws.Parameter,
        };

        private IPredicate Visit(Data.GreaterThan ws) => new GreaterThan(this.Visit(ws.RoleType))
        {
            Dependencies = ws.Dependencies,
            Parameter = ws.Parameter,
            Value = ws.Value,
        };

        private IPredicate Visit(Data.Instanceof ws) => new Instanceof(this.Visit(ws.ObjectType))
        {
            Dependencies = ws.Dependencies,
            Parameter = ws.Parameter,
        };

        private IPredicate Visit(Data.LessThan ws) => new LessThan(this.Visit(ws.RoleType))
        {
            Dependencies = ws.Dependencies,
            Parameter = ws.Parameter,
            Value = ws.Value,
        };

        private IPredicate Visit(Data.Like ws) => new Like(this.Visit(ws.RoleType))
        {
            Dependencies = ws.Dependencies,
            Parameter = ws.Parameter,
            Value = ws.Value,
        };

        private IPredicate Visit(Data.Not ws) => new Not(this.Visit(ws.Operand)) { Dependencies = ws.Dependencies };

        private IPredicate Visit(Data.Or ws) => new Or(ws.Operands?.Select(this.Visit).ToArray()) { Dependencies = ws.Dependencies };

        private Except Visit(Data.Except ws) => new Except(ws.Operands?.Select(this.Visit).ToArray()) { Sorting = this.Visit(ws.Sorting) };

        private Intersect Visit(Data.Intersect ws) => new Intersect(ws.Operands?.Select(this.Visit).ToArray()) { Sorting = this.Visit(ws.Sorting) };

        private Union Visit(Data.Union ws) => new Union(ws.Operands?.Select(this.Visit).ToArray()) { Sorting = this.Visit(ws.Sorting) };

        private IObject Visit(Workspace.IObject ws) => ws != null ? this.transaction.Instantiate(ws.Id) : null;

        private Result[] Visit(Data.Result[] ws) =>
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

        private Database.Data.Sort[] Visit(Data.Sort[] ws) => ws.Select(v => new Database.Data.Sort { RoleType = this.Visit(v.RoleType), SortDirection = (SortDirection)(int)v.SortDirection }).ToArray();

        private IObjectType Visit(Meta.IObjectType ws) => ws != null ? (IObjectType)this.metaPopulation.FindByTag(ws.Tag) : null;

        private IComposite Visit(Meta.IComposite ws) => ws != null ? (IComposite)this.metaPopulation.FindByTag(ws.Tag) : null;

        private IPropertyType Visit(Meta.IPropertyType ws) =>
            ws switch
            {
                Meta.IAssociationType associationType => this.Visit(associationType),
                Meta.IRoleType roleType => this.Visit(roleType),
                _ => throw new ArgumentException("Invalid property type")
            };

        private IAssociationType Visit(Meta.IAssociationType ws) => ((IRelationType)this.metaPopulation.FindByTag(ws.OperandTag)).AssociationType;

        private IRoleType Visit(Meta.IRoleType ws) => ((IRelationType)this.metaPopulation.FindByTag(ws.OperandTag)).RoleType;

        private IObject[] Visit(IEnumerable<Workspace.IObject> ws) => this.transaction.Instantiate(ws.Select(v => v.Id));

        private IArguments Visit(IDictionary<string, object> ws) => new Arguments(ws);
    }
}
