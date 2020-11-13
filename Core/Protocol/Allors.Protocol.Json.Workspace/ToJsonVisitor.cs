// <copyright file="ToJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Workspace
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;

    public class ToJsonVisitor : IVisitor
    {
        private readonly Stack<Json.Extent> extents;
        private readonly Stack<Json.Predicate> predicates;
        private readonly Stack<Json.Result> results;
        private readonly Stack<Json.Fetch> fetches;
        private readonly Stack<Json.Step> steps;
        private readonly Stack<Json.Node> nodes;
        private readonly Stack<Json.Sort> sorts;

        public ToJsonVisitor()
        {
            this.extents = new Stack<Json.Extent>();
            this.predicates = new Stack<Json.Predicate>();
            this.results = new Stack<Json.Result>();
            this.fetches = new Stack<Json.Fetch>();
            this.steps = new Stack<Json.Step>();
            this.nodes = new Stack<Json.Node>();
            this.sorts = new Stack<Json.Sort>();
        }

        public Json.Pull Pull { get; private set; }

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
                predicate.Operands = new Json.Predicate[length];
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
                Values = visited.Objects?.Select(v => v.DatabaseId?.ToString()).ToArray(),
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
                Object = visited.Object?.DatabaseId?.ToString(),
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
                Object = visited.Object?.DatabaseId?.ToString(),
                Value = UnitConvert.ToString(visited.Value),
                Parameter = visited.Parameter,
            };

            this.predicates.Push(predicate);
        }

        public void VisitExcept(Except visited)
        {
            var extent = new Json.Extent
            {
                Kind = ExtentKind.Except,
            };

            this.extents.Push(extent);

            if (visited.Operands != null)
            {
                var length = visited.Operands.Length;
                extent.Operands = new Json.Extent[length];
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
                extent.Sorting = new Json.Sort[length];
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

        public void VisitExtent(Extent visited)
        {
            var extent = new Json.Extent
            {
                Kind = Json.ExtentKind.Extent,
                ObjectType = visited.ObjectType?.Id,
                Sorting = visited.Sorting?.Select(v => new Json.Sort { Descending = v.Descending, RoleType = v.RoleType?.RelationType.Id }).ToArray(),
            };

            this.extents.Push(extent);

            if (visited.Predicate != null)
            {
                visited.Predicate.Accept(this);
                extent.Predicate = this.predicates.Pop();
            }
        }

        public void VisitFetch(Fetch visited)
        {
            var fetch = new Json.Fetch();

            this.fetches.Push(fetch);

            if (visited.Step != null)
            {
                visited.Step.Accept(this);
                fetch.Step = this.steps.Pop();
            }

            if (visited.Include?.Length > 0)
            {
                var length = visited.Include.Length;
                fetch.Include = new Json.Node[length];
                for (var i = 0; i < length; i++)
                {
                    var node = visited.Include[i];
                    node.Accept(this);
                    fetch.Include[i] = this.nodes.Pop();
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
            var extent = new Json.Extent
            {
                Kind = ExtentKind.Intersect,
            };

            this.extents.Push(extent);

            if (visited.Operands != null)
            {
                var length = visited.Operands.Length;
                extent.Operands = new Json.Extent[length];
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
                extent.Sorting = new Json.Sort[length];
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

        public void VisitNode(Node visited)
        {
            var node = new Json.Node
            {
                AssociationType = (visited.PropertyType as IAssociationType)?.RelationType.Id,
                RoleType = (visited.PropertyType as IRoleType)?.RelationType.Id,
            };

            this.nodes.Push(node);

            if (visited.Nodes?.Length > 0)
            {
                var length = visited.Nodes.Length;
                node.Nodes = new Json.Node[length];
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
                predicate.Operands = new Json.Predicate[length];
                for (var i = 0; i < length; i++)
                {
                    var operand = visited.Operands[i];
                    operand.Accept(this);
                    predicate.Operands[i] = this.predicates.Pop();
                }
            }
        }

        public void VisitPull(Pull visited)
        {
            var pull = new Json.Pull
            {
                ExtentRef = visited.ExtentRef,
                ObjectType = visited.ObjectType?.Id,
                Object = visited.ObjectId ?? visited.Object?.DatabaseId?.ToString(),
            };

            if (visited.Extent != null)
            {
                visited.Extent.Accept(this);
                pull.Extent = this.extents.Pop();
            }

            if (visited.Results?.Length > 0)
            {
                var length = visited.Results.Length;

                pull.Results = new Json.Result[length];
                for (var i = 0; i < length; i++)
                {
                    var result = visited.Results[i];
                    result.Accept(this);
                    pull.Results[i] = this.results.Pop();
                }
            }

            this.Pull = pull;
        }

        public void VisitResult(Result visited)
        {
            var result = new Json.Result
            {
                FetchRef = visited.FetchRef,
                Name = visited.Name,
                Skip = visited.Skip,
                Take = visited.Take,
            };

            this.results.Push(result);

            if (visited.Fetch != null)
            {
                visited.Fetch.Accept(this);
                result.Fetch = this.fetches.Pop();
            }
        }

        public void VisitSort(Sort visited)
        {
            var sort = new Json.Sort
            {
                Descending = visited.Descending,
                RoleType = visited.RoleType?.RelationType.Id,
            };

            this.sorts.Push(sort);
        }

        public void VisitStep(Step visited)
        {
            var step = new Json.Step
            {
                AssociationType = (visited.PropertyType as IAssociationType)?.RelationType.Id,
                RoleType = (visited.PropertyType as IRoleType)?.RelationType.Id,
            };

            this.steps.Push(step);

            if (visited.Include?.Length > 0)
            {
                var length = visited.Include.Length;
                step.Include = new Json.Node[length];
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
            var extent = new Json.Extent
            {
                Kind = ExtentKind.Union,
            };

            this.extents.Push(extent);

            if (visited.Operands != null)
            {
                var length = visited.Operands.Length;
                extent.Operands = new Json.Extent[length];
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
                extent.Sorting = new Json.Sort[length];
                for (var i = 0; i < length; i++)
                {
                    var sorting = visited.Sorting[i];
                    sorting.Accept(this);
                    extent.Sorting[i] = this.sorts.Pop();
                }
            }
        }
    }
}
