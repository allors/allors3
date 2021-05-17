// <copyright file="FromJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Data;
    using Meta;
    using Extent = Data.Extent;
    using Node = Data.Node;
    using Pull = Data.Pull;
    using Result = Data.Result;
    using Select = Data.Select;
    using Sort = Data.Sort;
    using Step = Data.Step;
    using Procedure = Data.Procedure;

    public class FromJsonVisitor : Allors.Protocol.Json.Data.IVisitor
    {
        private readonly ITransaction transaction;
        private IMetaPopulation metaPopulation;

        private readonly Stack<Data.IExtent> extents;
        private readonly Stack<Data.IPredicate> predicates;
        private readonly Stack<Result> results;
        private readonly Stack<Select> selects;
        private readonly Stack<Step> steps;
        private readonly Stack<Node> nodes;
        private readonly Stack<Sort> sorts;

        public FromJsonVisitor(ITransaction transaction)
        {
            this.transaction = transaction;
            this.metaPopulation = this.transaction.Database.ObjectFactory.MetaPopulation;

            this.extents = new Stack<Data.IExtent>();
            this.predicates = new Stack<Data.IPredicate>();
            this.results = new Stack<Result>();
            this.selects = new Stack<Select>();
            this.steps = new Stack<Step>();
            this.nodes = new Stack<Node>();
            this.sorts = new Stack<Sort>();
        }

        public Pull Pull { get; private set; }

        public Data.IExtent Extent => this.extents?.Peek();

        public Select Select => this.selects?.Peek();

        public Procedure Procedure { get; private set; }

        public void VisitExtent(Allors.Protocol.Json.Data.Extent visited)
        {
            Data.IExtentOperator extentOperator = null;
            Data.IExtent sortable = null;

            switch (visited.Kind)
            {
                case ExtentKind.Extent:
                    if (!visited.ObjectType.HasValue)
                    {
                        throw new Exception("Unknown extent kind " + visited.Kind);
                    }

                    var objectType = (IComposite)this.metaPopulation.FindByTag(visited.ObjectType.Value);
                    var extent = new Extent(objectType);
                    sortable = extent;

                    this.extents.Push(extent);

                    if (visited.Predicate != null)
                    {
                        visited.Predicate.Accept(this);
                        extent.Predicate = this.predicates.Pop();
                    }

                    break;

                case ExtentKind.Union:
                    extentOperator = new Data.Union();
                    break;

                case ExtentKind.Except:
                    extentOperator = new Data.Except();
                    break;

                case ExtentKind.Intersect:
                    extentOperator = new Data.Intersect();
                    break;

                default:
                    throw new Exception("Unknown extent kind " + visited.Kind);
            }

            sortable ??= extentOperator;

            if (visited.Sorting?.Length > 0)
            {
                var length = visited.Sorting.Length;

                sortable.Sorting = new Data.Sort[length];
                for (var i = 0; i < length; i++)
                {
                    var sorting = visited.Sorting[i];
                    sorting.Accept(this);
                    sortable.Sorting[i] = this.sorts.Pop();
                }
            }

            if (extentOperator != null)
            {
                this.extents.Push(extentOperator);

                if (visited.Operands?.Length > 0)
                {
                    var length = visited.Operands.Length;

                    extentOperator.Operands = new Data.IExtent[length];
                    for (var i = 0; i < length; i++)
                    {
                        var operand = visited.Operands[i];
                        operand.Accept(this);
                        extentOperator.Operands[i] = this.extents.Pop();
                    }
                }
            }
        }

        public void VisitSelect(Allors.Protocol.Json.Data.Select visited)
        {
            var @select = new Select(this.transaction.Database.MetaPopulation);

            this.selects.Push(@select);

            if (visited.Step != null)
            {
                visited.Step.Accept(this);
                @select.Step = this.steps.Pop();
            }

            if (visited.Include?.Length > 0)
            {
                @select.Include = new Node[visited.Include.Length];
                for (var i = 0; i < visited.Include.Length; i++)
                {
                    visited.Include[i].Accept(this);
                    @select.Include[i] = this.nodes.Pop();
                }
            }
        }

        public void VisitNode(Allors.Protocol.Json.Data.Node visited)
        {
            var propertyType = (IPropertyType)this.metaPopulation.FindAssociationType(visited.AssociationType) ?? this.metaPopulation.FindRoleType(visited.RoleType);
            var node = new Node(propertyType);

            this.nodes.Push(node);

            if (visited.Nodes?.Length > 0)
            {
                foreach (var childNode in visited.Nodes)
                {
                    childNode.Accept(this);
                    node.Add(this.nodes.Pop());
                }
            }
        }

        public void VisitPredicate(Predicate visited)
        {
            switch (visited.Kind)
            {
                case PredicateKind.And:
                    var and = new Data.And
                    {
                        Dependencies = visited.Dependencies,
                    };

                    this.predicates.Push(and);

                    if (visited.Operands?.Length > 0)
                    {
                        var length = visited.Operands.Length;

                        and.Operands = new Data.IPredicate[length];
                        for (var i = 0; i < length; i++)
                        {
                            var operand = visited.Operands[i];
                            operand.Accept(this);
                            and.Operands[i] = this.predicates.Pop();
                        }
                    }

                    break;

                case PredicateKind.Or:
                    var or = new Data.Or
                    {
                        Dependencies = visited.Dependencies
                    };

                    this.predicates.Push(or);

                    if (visited.Operands?.Length > 0)
                    {
                        var length = visited.Operands.Length;

                        or.Operands = new Data.IPredicate[length];
                        for (var i = 0; i < length; i++)
                        {
                            var operand = visited.Operands[i];
                            operand.Accept(this);
                            or.Operands[i] = this.predicates.Pop();
                        }
                    }

                    break;

                case PredicateKind.Not:
                    var not = new Data.Not
                    {
                        Dependencies = visited.Dependencies,
                    };

                    this.predicates.Push(not);

                    if (visited.Operand != null)
                    {
                        visited.Operand.Accept(this);
                        not.Operand = this.predicates.Pop();
                    }

                    break;

                default:
                    var associationType = this.metaPopulation.FindAssociationType(visited.AssociationType);
                    var roleType = this.metaPopulation.FindRoleType(visited.RoleType);
                    var propertyType = (IPropertyType)associationType ?? roleType;

                    switch (visited.Kind)
                    {
                        case PredicateKind.InstanceOf:

                            var instanceOf = new Data.Instanceof(propertyType)
                            {
                                Dependencies = visited.Dependencies,
                                ObjectType = visited.ObjectType != null ? (IComposite)this.transaction.Database.MetaPopulation.FindByTag(visited.ObjectType.Value) : null
                            };

                            this.predicates.Push(instanceOf);
                            break;

                        case PredicateKind.Exists:

                            var exists = new Data.Exists(propertyType)
                            {
                                Dependencies = visited.Dependencies,
                                Parameter = visited.Parameter,
                            };

                            this.predicates.Push(exists);
                            break;

                        case PredicateKind.Contains:

                            var contains = new Data.Contains(propertyType)
                            {
                                Dependencies = visited.Dependencies,
                                Parameter = visited.Parameter,
                                Object = visited.Object.HasValue ? this.transaction.Instantiate(visited.Object.Value) : null,
                            };

                            this.predicates.Push(contains);
                            break;

                        case PredicateKind.ContainedIn:

                            var containedIn = new Data.ContainedIn(propertyType)
                            {
                                Dependencies = visited.Dependencies,
                                Parameter = visited.Parameter
                            };

                            this.predicates.Push(containedIn);

                            if (visited.Objects != null)
                            {
                                containedIn.Objects = visited.Objects.Select(this.transaction.Instantiate).ToArray();
                            }
                            else if (visited.Extent != null)
                            {
                                visited.Extent.Accept(this);
                                containedIn.Extent = this.extents.Pop();
                            }

                            break;

                        case PredicateKind.Equals:

                            var equals = new Data.Equals(propertyType)
                            {
                                Dependencies = visited.Dependencies,
                                Parameter = visited.Parameter
                            };

                            this.predicates.Push(equals);

                            if (visited.Object != null)
                            {
                                equals.Object = visited.Object.HasValue
                                    ? this.transaction.Instantiate(visited.Object.Value)
                                    : null;
                            }
                            else if (visited.Value != null)
                            {
                                var value = UnitConvert.FromJson(((IRoleType)propertyType).ObjectType.Tag, visited.Value);
                                equals.Value = value;
                            }

                            break;

                        case PredicateKind.Between:

                            var between = new Data.Between(roleType)
                            {
                                Dependencies = visited.Dependencies,
                                Parameter = visited.Parameter,
                                Values = visited.Values?.Select(v => UnitConvert.FromJson(roleType.ObjectType.Tag, v)).ToArray(),
                            };

                            this.predicates.Push(between);

                            break;

                        case PredicateKind.GreaterThan:

                            var greaterThan = new Data.GreaterThan(roleType)
                            {
                                Dependencies = visited.Dependencies,
                                Parameter = visited.Parameter,
                                Value = UnitConvert.FromJson(roleType.ObjectType.Tag, visited.Value),
                            };

                            this.predicates.Push(greaterThan);

                            break;

                        case PredicateKind.LessThan:

                            var lessThan = new Data.LessThan(roleType)
                            {
                                Dependencies = visited.Dependencies,
                                Parameter = visited.Parameter,
                                Value = UnitConvert.FromJson(roleType.ObjectType.Tag, visited.Value),
                            };

                            this.predicates.Push(lessThan);

                            break;

                        case PredicateKind.Like:

                            var like = new Data.Like(roleType)
                            {
                                Dependencies = visited.Dependencies,
                                Parameter = visited.Parameter,
                                Value = UnitConvert.FromJson(roleType.ObjectType.Tag, visited.Value)?.ToString(),
                            };

                            this.predicates.Push(like);

                            break;

                        default:
                            throw new Exception("Unknown predicate kind " + visited.Kind);
                    }

                    break;
            }
        }

        public void VisitPull(Allors.Protocol.Json.Data.Pull visited)
        {
            var pull = new Pull
            {
                ExtentRef = visited.ExtentRef,
                ObjectType = visited.ObjectType.HasValue ? (IObjectType)this.transaction.Database.MetaPopulation.FindByTag(visited.ObjectType.Value) : null,
                Object = visited.Object != null ? this.transaction.Instantiate(visited.Object.Value) : null,
                Arguments = new Arguments(visited.Arguments),
            };

            if (visited.Extent != null)
            {
                visited.Extent.Accept(this);
                pull.Extent = this.extents.Pop();
            }

            if (visited.Results?.Length > 0)
            {
                var length = visited.Results.Length;

                pull.Results = new Result[length];
                for (var i = 0; i < length; i++)
                {
                    var result = visited.Results[i];
                    result.Accept(this);
                    pull.Results[i] = this.results.Pop();
                }
            }

            this.Pull = pull;
        }

        public void VisitResult(Allors.Protocol.Json.Data.Result visited)
        {
            var result = new Result
            {
                SelectRef = visited.SelectRef,
                Name = visited.Name,
                Skip = visited.Skip,
                Take = visited.Take,
            };

            if (visited.Select != null)
            {
                visited.Select.Accept(this);
                result.Select = this.selects.Pop();
            }

            this.results.Push(result);
        }

        public void VisitSort(Allors.Protocol.Json.Data.Sort visited)
        {
            var sort = new Sort
            {
                SortDirection = visited.SortDirection,
                RoleType = visited.RoleType != null ? (IRoleType)this.transaction.Database.ObjectFactory.MetaPopulation.FindByTag(visited.RoleType.Value) : null,
            };

            this.sorts.Push(sort);
        }

        public void VisitStep(Allors.Protocol.Json.Data.Step visited)
        {
            var propertyType = (IPropertyType)this.metaPopulation.FindAssociationType(visited.AssociationType) ?? this.metaPopulation.FindRoleType(visited.RoleType);

            var step = new Step
            {
                PropertyType = propertyType,
            };

            this.steps.Push(step);

            if (visited.Next != null)
            {
                visited.Next.Accept(this);
                step.Next = this.steps.Pop();
            }

            if (visited.Include?.Length > 0)
            {
                step.Include = new Node[visited.Include.Length];
                for (var i = 0; i < visited.Include.Length; i++)
                {
                    visited.Include[i].Accept(this);
                    step.Include[i] = this.nodes.Pop();
                }
            }
        }

        public void VisitProcedure(Allors.Protocol.Json.Data.Procedure procedure) =>
            this.Procedure = new Procedure(procedure.Name)
            {
                Collections = procedure.Collections?.ToDictionary(kvp => kvp.Key, kvp => this.transaction.Instantiate(kvp.Value)),
                Objects = procedure.Objects?.ToDictionary(kvp => kvp.Key, kvp => this.transaction.Instantiate(kvp.Value)),
                Values = procedure.Values,
                Pool = procedure.Pool?.ToDictionary(v => this.transaction.Instantiate(v[0]), v => v[1])
            };
    }
}
