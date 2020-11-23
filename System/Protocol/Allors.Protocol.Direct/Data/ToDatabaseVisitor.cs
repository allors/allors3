// <copyright file="ToJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Data;
    using Database.Meta;
    using Workspace;
    using IObject = Database.IObject;
    using ISession = Database.ISession;

    public class ToDatabaseVisitor : Workspace.Data.IVisitor
    {
        private readonly ISession session;
        private readonly IMetaPopulation metaPopulation;

        private readonly Stack<IExtent> extents;
        private readonly Stack<IPredicate> predicates;
        private readonly Stack<Result> results;
        private readonly Stack<Fetch> fetches;
        private readonly Stack<Step> steps;
        private readonly Stack<Node> nodes;
        private readonly Stack<Sort> sorts;

        public ToDatabaseVisitor(ISession session)
        {
            this.session = session;
            this.metaPopulation = this.session.Database.ObjectFactory.MetaPopulation;

            this.extents = new Stack<IExtent>();
            this.predicates = new Stack<IPredicate>();
            this.results = new Stack<Result>();
            this.fetches = new Stack<Fetch>();
            this.steps = new Stack<Step>();
            this.nodes = new Stack<Node>();
            this.sorts = new Stack<Sort>();
        }

        public Pull Pull { get; private set; }

        public IExtent Extent => this.extents?.Peek();

        public Fetch Fetch => this.fetches?.Peek();

        public void VisitAnd(Workspace.Data.And visited)
        {
            var and = new And
            {
                Dependencies = visited.Dependencies,
            };

            this.predicates.Push(and);

            if (visited.Operands?.Length > 0)
            {
                var length = visited.Operands.Length;

                and.Operands = new IPredicate[length];
                for (var i = 0; i < length; i++)
                {
                    var operand = visited.Operands[i];
                    operand.Accept(this);
                    and.Operands[i] = this.predicates.Pop();
                }
            }
        }

        public void VisitBetween(Workspace.Data.Between visited)
        {
            var between = new Between(this.FindRoleType(visited.RoleType))
            {
                Dependencies = visited.Dependencies,
                Parameter = visited.Parameter,
                Values = visited.Values,
            };

            this.predicates.Push(between);
        }

        public void VisitContainedIn(Workspace.Data.ContainedIn visited)
        {
            var containedIn = new ContainedIn(this.FindPropertyType(visited.PropertyType))
            {
                Dependencies = visited.Dependencies,
                Parameter = visited.Parameter
            };

            this.predicates.Push(containedIn);

            if (visited.Objects != null)
            {
                containedIn.Objects = this.FindObjects(visited.Objects);
            }
            else if (visited.Extent != null)
            {
                visited.Extent.Accept(this);
                containedIn.Extent = this.extents.Pop();
            }
        }

        public void VisitContains(Workspace.Data.Contains visited)
        {
            var contains = new Contains
            {
                Dependencies = visited.Dependencies,
                PropertyType = this.FindPropertyType(visited.PropertyType),
                Parameter = visited.Parameter,
                Object = this.FindObject(visited.Object),
            };

            this.predicates.Push(contains);
        }

        public void VisitEquals(Workspace.Data.Equals visited)
        {
            var equals = new Equals(this.FindPropertyType(visited.PropertyType))
            {
                Dependencies = visited.Dependencies,
                Parameter = visited.Parameter
            };

            this.predicates.Push(equals);

            if (visited.Object != null)
            {
                equals.Object = this.FindObject(visited.Object);
            }
            else if (visited.Value != null)
            {
                equals.Value = visited.Value;
            }
        }

        public void VisitExcept(Workspace.Data.Except visited)
        {
            var extent = new Except();

            this.extents.Push(extent);

            if (visited.Operands?.Length > 0)
            {
                var length = visited.Operands.Length;

                extent.Operands = new IExtent[length];
                for (var i = 0; i < length; i++)
                {
                    var operand = visited.Operands[i];
                    operand.Accept(this);
                    extent.Operands[i] = this.extents.Pop();
                }
            }

            if (visited.Sorting?.Length > 0)
            {
                var length = visited.Sorting.Length;

                extent.Sorting = new Sort[length];
                for (var i = 0; i < length; i++)
                {
                    var sorting = visited.Sorting[i];
                    sorting.Accept(this);
                    extent.Sorting[i] = this.sorts.Pop();
                }
            }
        }

        public void VisitExists(Workspace.Data.Exists visited)
        {
            var exists = new Exists
            {
                Dependencies = visited.Dependencies,
                PropertyType = this.FindPropertyType(visited.PropertyType),
                Parameter = visited.Parameter,
            };

            this.predicates.Push(exists);
        }

        public void VisitExtent(Workspace.Data.Extent visited)
        {
            if (visited.ObjectType == null)
            {
                throw new Exception("Extent requires an ObjectType");
            }

            var extent = new Extent(this.FindComposite(visited.ObjectType));

            this.extents.Push(extent);

            if (visited.Predicate != null)
            {
                visited.Predicate.Accept(this);
                extent.Predicate = this.predicates.Pop();
            }

            if (visited.Sorting?.Length > 0)
            {
                var length = visited.Sorting.Length;

                extent.Sorting = new Sort[length];
                for (var i = 0; i < length; i++)
                {
                    var sorting = visited.Sorting[i];
                    sorting.Accept(this);
                    extent.Sorting[i] = this.sorts.Pop();
                }
            }
        }

        public void VisitFetch(Workspace.Data.Fetch visited)
        {
            var fetch = new Fetch(this.session.Database.MetaPopulation);

            this.fetches.Push(fetch);

            if (visited.Step != null)
            {
                visited.Step.Accept(this);
                fetch.Step = this.steps.Pop();
            }

            if (visited.Include?.Length > 0)
            {
                fetch.Include = new Node[visited.Include.Length];
                for (var i = 0; i < visited.Include.Length; i++)
                {
                    visited.Include[i].Accept(this);
                    fetch.Include[i] = this.nodes.Pop();
                }
            }
        }

        public void VisitGreaterThan(Workspace.Data.GreaterThan visited)
        {
            var greaterThan = new GreaterThan(this.FindRoleType(visited.RoleType))
            {
                Dependencies = visited.Dependencies,
                Parameter = visited.Parameter,
                Value = visited.Value,
            };

            this.predicates.Push(greaterThan);

        }

        public void VisitInstanceOf(Workspace.Data.Instanceof visited)
        {
            var instanceOf = new Instanceof(this.FindComposite(visited.ObjectType))
            {
                Dependencies = visited.Dependencies,
                PropertyType = this.FindPropertyType(visited.PropertyType),
            };

            this.predicates.Push(instanceOf);
        }

        public void VisitIntersect(Workspace.Data.Intersect visited)
        {
            var extent = new Intersect();

            this.extents.Push(extent);

            if (visited.Operands?.Length > 0)
            {
                var length = visited.Operands.Length;

                extent.Operands = new IExtent[length];
                for (var i = 0; i < length; i++)
                {
                    var operand = visited.Operands[i];
                    operand.Accept(this);
                    extent.Operands[i] = this.extents.Pop();
                }
            }

            if (visited.Sorting?.Length > 0)
            {
                var length = visited.Sorting.Length;

                extent.Sorting = new Sort[length];
                for (var i = 0; i < length; i++)
                {
                    var sorting = visited.Sorting[i];
                    sorting.Accept(this);
                    extent.Sorting[i] = this.sorts.Pop();
                }
            }
        }

        public void VisitLessThan(Workspace.Data.LessThan visited)
        {
            var lessThan = new LessThan(this.FindRoleType(visited.RoleType))
            {
                Dependencies = visited.Dependencies,
                Parameter = visited.Parameter,
                Value = visited.Value,
            };

            this.predicates.Push(lessThan);
        }

        public void VisitLike(Workspace.Data.Like visited)
        {
            var like = new Like(this.FindRoleType(visited.RoleType))
            {
                Dependencies = visited.Dependencies,
                Parameter = visited.Parameter,
                Value = visited.Value,
            };

            this.predicates.Push(like);
        }

        public void VisitNode(Workspace.Data.Node visited)
        {
            var node = new Node(this.FindPropertyType(visited.PropertyType));

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

        public void VisitNot(Workspace.Data.Not visited)
        {
            var not = new Not
            {
                Dependencies = visited.Dependencies,
            };

            this.predicates.Push(not);

            if (visited.Operand != null)
            {
                visited.Operand.Accept(this);
                not.Operand = this.predicates.Pop();
            }
        }

        public void VisitOr(Workspace.Data.Or visited)
        {
            var or = new Or
            {
                Dependencies = visited.Dependencies
            };

            this.predicates.Push(or);

            if (visited.Operands?.Length > 0)
            {
                var length = visited.Operands.Length;

                or.Operands = new IPredicate[length];
                for (var i = 0; i < length; i++)
                {
                    var operand = visited.Operands[i];
                    operand.Accept(this);
                    or.Operands[i] = this.predicates.Pop();
                }
            }
        }

        public void VisitPull(Workspace.Data.Pull visited)
        {
            var pull = new Pull
            {
                ExtentRef = visited.ExtentRef,
                ObjectType = this.FindComposite(visited.ObjectType),
                Object = this.FindObject(visited.Object),
                Parameters = visited.Parameters,
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

        public void VisitResult(Workspace.Data.Result visited)
        {
            var result = new Result
            {
                FetchRef = visited.FetchRef,
                Name = visited.Name,
                Skip = visited.Skip,
                Take = visited.Take,
            };

            if (visited.Fetch != null)
            {
                visited.Fetch.Accept(this);
                result.Fetch = this.fetches.Pop();
            }

            this.results.Push(result);
        }

        public void VisitSort(Workspace.Data.Sort visited)
        {
            var sort = new Sort
            {
                Descending = visited.Descending,
                RoleType = this.FindRoleType(visited.RoleType),
            };

            this.sorts.Push(sort);
        }

        public void VisitStep(Workspace.Data.Step visited)
        {
            var propertyType = this.FindPropertyType(visited.PropertyType);

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

        public void VisitUnion(Workspace.Data.Union visited)
        {
            var extent = new Union();

            this.extents.Push(extent);

            if (visited.Operands?.Length > 0)
            {
                var length = visited.Operands.Length;

                extent.Operands = new IExtent[length];
                for (var i = 0; i < length; i++)
                {
                    var operand = visited.Operands[i];
                    operand.Accept(this);
                    extent.Operands[i] = this.extents.Pop();
                }
            }

            if (visited.Sorting?.Length > 0)
            {
                var length = visited.Sorting.Length;

                extent.Sorting = new Sort[length];
                for (var i = 0; i < length; i++)
                {
                    var sorting = visited.Sorting[i];
                    sorting.Accept(this);
                    extent.Sorting[i] = this.sorts.Pop();
                }
            }
        }

        private IComposite FindComposite(Workspace.Meta.IObjectType workspaceComposite) => workspaceComposite != null ? (IComposite)this.metaPopulation.Find(workspaceComposite.Id) : null;

        private IRoleType FindRoleType(Workspace.Meta.IRoleType workspaceRoleType) => workspaceRoleType != null ? ((IRelationType)this.metaPopulation.Find(workspaceRoleType.RelationType.Id)).RoleType : null;

        private IPropertyType FindPropertyType(Workspace.Meta.IPropertyType workspacePropertyType) =>
            workspacePropertyType switch
            {
                Workspace.Meta.IAssociationType workspaceAssociationType => ((IRelationType)this.metaPopulation.Find(workspaceAssociationType.RelationType.Id)).AssociationType,
                Workspace.Meta.IRoleType workspaceRoleType => ((IRelationType)this.metaPopulation.Find(workspaceRoleType.RelationType.Id)).RoleType,
                _ => null
            };

        private IObject FindObject(IDatabaseObject @object) => @object?.DatabaseId.HasValue == true ? this.session.Instantiate(@object.DatabaseId.Value) : null;

        private IObject FindObject(IDatabaseStrategy strategy) => strategy?.DatabaseId.HasValue == true ? this.session.Instantiate(strategy.DatabaseId.Value) : null;

        private IEnumerable<IObject> FindObjects(IEnumerable<IDatabaseStrategy> strategies) =>
            strategies
                ?.Where(v => v.DatabaseId.HasValue)
                .Select(v => this.session.Instantiate(v.DatabaseId.Value))
                ?? Array.Empty<IObject>();
    }
}
