// <copyright file="ToJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Protocol.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Data;
    using Data;
    using Meta;
    using Extent = Allors.Protocol.Json.Data.Extent;
    using IVisitor = Data.IVisitor;
    using Node = Allors.Protocol.Json.Data.Node;
    using Procedure = Allors.Protocol.Json.Data.Procedure;
    using Pull = Allors.Protocol.Json.Data.Pull;
    using Result = Allors.Protocol.Json.Data.Result;
    using Select = Allors.Protocol.Json.Data.Select;
    using Sort = Allors.Protocol.Json.Data.Sort;
    using Step = Allors.Protocol.Json.Data.Step;

    public class ToJsonVisitor : IVisitor
    {
        private readonly Stack<Extent> extents;
        private readonly Stack<Predicate> predicates;
        private readonly Stack<Result> results;
        private readonly Stack<Select> selects;
        private readonly Stack<Step> steps;
        private readonly Stack<Node> nodes;
        private readonly Stack<Sort> sorts;

        public ToJsonVisitor()
        {
            this.extents = new Stack<Extent>();
            this.predicates = new Stack<Predicate>();
            this.results = new Stack<Result>();
            this.selects = new Stack<Select>();
            this.steps = new Stack<Step>();
            this.nodes = new Stack<Node>();
            this.sorts = new Stack<Sort>();
        }

        public Pull Pull { get; private set; }

        public Extent Extent => this.extents?.Peek();

        public Select Select => this.selects?.Peek();

        public Procedure Procedure { get; private set; }

        public void VisitAnd(And visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.And,
                Dependencies = visited.Dependencies,
            };

            this.predicates.Push(predicate);

            if (visited.Operands != null)
            {
                var length = visited.Operands.Length;
                predicate.Operands = new Predicate[length];
                for (var i = 0; i < length; i++)
                {
                    var operand = visited.Operands[i];
                    operand.Accept(this);
                    predicate.Operands[i] = this.predicates.Pop();
                }
            }
        }

        public void VisitBetween(Between visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.Between,
                Dependencies = visited.Dependencies,
                RoleType = visited.RoleType?.RelationType.Id,
                Values = visited.Values.Select(UnitConvert.ToString).ToArray(),
                Parameter = visited.Parameter,
            };

            this.predicates.Push(predicate);
        }

        public void VisitContainedIn(ContainedIn visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.ContainedIn,
                Dependencies = visited.Dependencies,
                AssociationType = (visited.PropertyType as IAssociationType)?.RelationType.Id,
                RoleType = (visited.PropertyType as IRoleType)?.RelationType.Id,
                Values = visited.Objects?.Select(v => v.Identity.ToString()).ToArray(),
                Parameter = visited.Parameter,
            };

            this.predicates.Push(predicate);

            if (visited.Extent != null)
            {
                visited.Extent.Accept(this);
                predicate.Extent = this.extents.Pop();
            }
        }

        public void VisitContains(Contains visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.Contains,
                Dependencies = visited.Dependencies,
                AssociationType = (visited.PropertyType as IAssociationType)?.RelationType.Id,
                RoleType = (visited.PropertyType as IRoleType)?.RelationType.Id,
                Object = visited.Object?.Identity.ToString(),
                Parameter = visited.Parameter,
            };

            this.predicates.Push(predicate);
        }

        public void VisitEquals(Equals visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.Equals,
                Dependencies = visited.Dependencies,
                AssociationType = (visited.PropertyType as IAssociationType)?.RelationType.Id,
                RoleType = (visited.PropertyType as IRoleType)?.RelationType.Id,
                Object = visited.Object?.Identity.ToString(),
                Value = UnitConvert.ToString(visited.Value),
                Parameter = visited.Parameter,
            };

            this.predicates.Push(predicate);
        }

        public void VisitExcept(Except visited)
        {
            var extent = new Extent
            {
                Kind = ExtentKind.Except,
            };

            this.extents.Push(extent);

            if (visited.Operands != null)
            {
                var length = visited.Operands.Length;
                extent.Operands = new Extent[length];
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

        public void VisitExists(Exists visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.Exists,
                Dependencies = visited.Dependencies,
                AssociationType = (visited.PropertyType as IAssociationType)?.RelationType.Id,
                RoleType = (visited.PropertyType as IRoleType)?.RelationType.Id,
                Parameter = visited.Parameter,
            };

            this.predicates.Push(predicate);
        }

        public void VisitExtent(Allors.Workspace.Data.Extent visited)
        {
            var extent = new Extent
            {
                Kind = ExtentKind.Extent,
                ObjectType = visited.ObjectType?.Id,
                Sorting = visited.Sorting?.Select(v => new Sort { Descending = v.Descending, RoleType = v.RoleType?.RelationType.Id }).ToArray(),
            };

            this.extents.Push(extent);

            if (visited.Predicate != null)
            {
                visited.Predicate.Accept(this);
                extent.Predicate = this.predicates.Pop();
            }
        }

        public void VisitSelect(Allors.Workspace.Data.Select visited)
        {
            var @select = new Select();

            this.selects.Push(@select);

            if (visited.Step != null)
            {
                visited.Step.Accept(this);
                @select.Step = this.steps.Pop();
            }

            if (visited.Include?.Length > 0)
            {
                var length = visited.Include.Length;
                @select.Include = new Node[length];
                for (var i = 0; i < length; i++)
                {
                    var node = visited.Include[i];
                    node.Accept(this);
                    @select.Include[i] = this.nodes.Pop();
                }
            }
        }

        public void VisitGreaterThan(GreaterThan visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.GreaterThan,
                Dependencies = visited.Dependencies,
                RoleType = visited.RoleType?.RelationType.Id,
                Value = UnitConvert.ToString(visited.Value),
                Parameter = visited.Parameter,
            };

            this.predicates.Push(predicate);
        }

        public void VisitInstanceOf(Instanceof visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.InstanceOf,
                Dependencies = visited.Dependencies,
                ObjectType = visited.ObjectType?.Id,
                AssociationType = (visited.PropertyType as IAssociationType)?.RelationType.Id,
                RoleType = (visited.PropertyType as IRoleType)?.RelationType.Id,
            };

            this.predicates.Push(predicate);
        }

        public void VisitIntersect(Intersect visited)
        {
            var extent = new Extent
            {
                Kind = ExtentKind.Intersect,
            };

            this.extents.Push(extent);

            if (visited.Operands != null)
            {
                var length = visited.Operands.Length;
                extent.Operands = new Extent[length];
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

        public void VisitLessThan(LessThan visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.LessThan,
                Dependencies = visited.Dependencies,
                RoleType = visited.RoleType?.RelationType.Id,
                Value = UnitConvert.ToString(visited.Value),
                Parameter = visited.Parameter,
            };

            this.predicates.Push(predicate);
        }

        public void VisitLike(Like visited)
        {
            var predicate = new Predicate
            {
                Kind = PredicateKind.Like,
                Dependencies = visited.Dependencies,
                RoleType = visited.RoleType?.RelationType.Id,
                Value = UnitConvert.ToString(visited.Value),
                Parameter = visited.Parameter,
            };

            this.predicates.Push(predicate);
        }

        public void VisitNode(Allors.Workspace.Data.Node visited)
        {
            var node = new Node
            {
                AssociationType = (visited.PropertyType as IAssociationType)?.RelationType.Id,
                RoleType = (visited.PropertyType as IRoleType)?.RelationType.Id,
            };

            this.nodes.Push(node);

            if (visited.Nodes?.Length > 0)
            {
                var length = visited.Nodes.Length;
                node.Nodes = new Node[length];
                for (var i = 0; i < length; i++)
                {
                    visited.Nodes[i].Accept(this);
                    node.Nodes[i] = this.nodes.Pop();
                }
            }
        }

        public void VisitNot(Not visited)
        {
            var predicate = new Predicate()
            {
                Kind = PredicateKind.Not,
                Dependencies = visited.Dependencies,
            };

            this.predicates.Push(predicate);

            if (visited.Operand != null)
            {
                visited.Operand.Accept(this);
                predicate.Operand = this.predicates.Pop();
            }
        }

        public void VisitOr(Or visited)
        {
            var predicate = new Predicate()
            {
                Kind = PredicateKind.Or,
                Dependencies = visited.Dependencies,
            };

            this.predicates.Push(predicate);

            if (visited.Operands != null)
            {
                var length = visited.Operands.Length;
                predicate.Operands = new Predicate[length];
                for (var i = 0; i < length; i++)
                {
                    var operand = visited.Operands[i];
                    operand.Accept(this);
                    predicate.Operands[i] = this.predicates.Pop();
                }
            }
        }

        public void VisitPull(Allors.Workspace.Data.Pull visited)
        {
            var pull = new Pull
            {
                ExtentRef = visited.ExtentRef,
                ObjectType = visited.ObjectType?.Id,
                Object = visited.ObjectId ?? visited.Object?.Identity.ToString(),
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

        public void VisitResult(Allors.Workspace.Data.Result visited)
        {
            var result = new Result
            {
                SelectRef = visited.SelectRef,
                Name = visited.Name,
                Skip = visited.Skip,
                Take = visited.Take,
            };

            this.results.Push(result);

            if (visited.Select != null)
            {
                visited.Select.Accept(this);
                result.Select = this.selects.Pop();
            }
        }

        public void VisitSort(Allors.Workspace.Data.Sort visited)
        {
            var sort = new Sort
            {
                Descending = visited.Descending,
                RoleType = visited.RoleType?.RelationType.Id,
            };

            this.sorts.Push(sort);
        }

        public void VisitStep(Allors.Workspace.Data.Step visited)
        {
            var step = new Step
            {
                AssociationType = (visited.PropertyType as IAssociationType)?.RelationType.Id,
                RoleType = (visited.PropertyType as IRoleType)?.RelationType.Id,
            };

            this.steps.Push(step);

            if (visited.Include?.Length > 0)
            {
                var length = visited.Include.Length;
                step.Include = new Node[length];
                for (var i = 0; i < length; i++)
                {
                    var node = visited.Include[i];
                    node.Accept(this);
                    step.Include[i] = this.nodes.Pop();
                }
            }

            if (visited.Next != null)
            {
                visited.Next.Accept(this);
                step.Next = this.steps.Pop();
            }
        }

        public void VisitUnion(Union visited)
        {
            var extent = new Extent
            {
                Kind = ExtentKind.Union,
            };

            this.extents.Push(extent);

            if (visited.Operands != null)
            {
                var length = visited.Operands.Length;
                extent.Operands = new Extent[length];
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

        public void VisitProcedure(Data.Procedure procedure) => this.Procedure = new Allors.Protocol.Json.Data.Procedure
        {
            Name = procedure.Name,
            CollectionByName = procedure.CollectionByName.ToJsonForCollectionByName(),
            ObjectByName = procedure.ObjectByName.ToJsonForObjectByName(),
            ValueByName = procedure.ValueByName.ToJsonForValueByName(),
            VersionByObject = procedure.VersionByObject.ToJsonForVersionByObject(),
        };
    }
}
