// <copyright file="ReadGrammar.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Data;
    using Allors.Protocol.Json.SystemTextJson;
    using Meta;

    /// <summary>Admits only the built-in declarative grammar, without reading domain objects.</summary>
    internal sealed class ReadGrammar
    {
        private readonly IMetaPopulation meta;
        private readonly UnitConvert convert = new UnitConvert();
        private IDictionary<string, object> arguments;

        internal ReadGrammar(IMetaPopulation meta) => this.meta = meta;

        internal static void Check(bool valid, string path, string message)
        {
            if (!valid)
            {
                throw new ReadRequestException(path + ": " + message);
            }
        }

        internal IComposite Composite(string tag, string path)
        {
            var composite = tag == null ? null : this.meta.FindByTag(tag) as IComposite;
            Check(composite?.ExistDatabaseClass == true, path, "Supply the tag of a known database composite type.");
            return composite;
        }

        internal IPropertyType Property(string association, string role, IComposite source, string path, bool optional = false)
        {
            Check(association == null || role == null, path, "Choose either an association or a role endpoint.");
            if (association == null && role == null && optional)
            {
                return null;
            }

            var relation = this.meta.FindByTag(association ?? role ?? string.Empty) as IRelationType;
            Check(relation != null, path, "Supply the tag of a known relation endpoint.");
            IPropertyType property = association != null ? relation.AssociationType : relation.RoleType;
            var owner = property is IRoleType r ? r.AssociationType.ObjectType : relation.RoleType.ObjectType as IComposite;
            Check(owner?.ExistDatabaseClass == true, path, "Use a relation endpoint defined on database objects.");
            Check(source == null || Compatible(source, owner), path, "Use a relation endpoint applicable to the selected composite type.");
            Check(owner.DatabaseClasses.Any(c => property is IRoleType rt ? c.DatabaseRoleTypes.Contains(rt) : c.DatabaseAssociationTypes.Contains((IAssociationType)property)),
                path, "Use a database relation endpoint.");
            return property;
        }

        internal void Pull(Pull pull, IComposite actualSource = null)
        {
            this.arguments = pull.a;
            Check((pull.o.HasValue ? 1 : 0) + (pull.e != null ? 1 : 0) == 1, "$pull", "Choose exactly one explicit object or declarative extent.");
            Check(pull.e == null || pull.t == null, "$pull.t", "Use the extent's object type; an object type constraint belongs only to an explicit target.");
            IComposite source = pull.t == null ? actualSource : this.Composite(pull.t, "$pull.t");
            if (pull.e != null)
            {
                source = this.Extent(pull.e, "$pull.e");
            }

            foreach (var result in pull.r ?? Array.Empty<Result>())
            {
                Check(result.i == null || result.s == null, "$pull.result", "Put includes on the select's terminal step, or use result includes without a select.");
                Check(!result.k.HasValue || !result.t.HasValue || (long)result.k.Value + result.t.Value <= int.MaxValue,
                    "$pull.result", "Reduce skip and take so their sum fits a 32-bit count.");
                Check(!pull.o.HasValue || (!result.k.HasValue && !result.t.HasValue), "$pull.result", "Apply pagination to an extent selection, not an explicit object.");
                if (result.s != null)
                {
                    this.Select(result.s, source, "$pull.result.s");
                }
                else
                {
                    this.Nodes(result.i, source, "$pull.result.i", 0);
                }
            }
        }

        internal IComposite Extent(Extent extent, string path, int depth = 0)
        {
            Shape(extent, path, depth);
            IComposite source;
            if (extent.k == ExtentKind.Filter)
            {
                Check(extent.o == null, path, "A filter extent cannot contain extent operands.");
                source = this.Composite(extent.t, path + ".t");
                if (extent.p != null)
                {
                    this.Predicate(extent.p, source, path + ".p", depth + 1);
                }
            }
            else
            {
                Check(extent.k == ExtentKind.Union || extent.k == ExtentKind.Intersect || extent.k == ExtentKind.Except, path, "Use a supported built-in extent kind.");
                Check(extent.t == null && extent.p == null, path, "An extent operator contains operands and sorting only.");
                Check(extent.o?.Length == 2, path, "Supply exactly two operands for a built-in extent operator.");
                source = this.Extent(extent.o[0], path + ".o[0]", depth + 1);
                var other = this.Extent(extent.o[1], path + ".o[1]", depth + 1);
                Check(Compatible(source, other), path, "Combine extents whose composite types share database classes.");
            }

            foreach (var sort in extent.s ?? Array.Empty<Sort>())
            {
                Shape(sort, path + ".s", depth + 1);
                var role = this.Property(null, sort.r, source, path + ".s.r") as IRoleType;
                Check(role?.ObjectType.IsUnit == true, path + ".s.r", "Sort by a unit role of the selected object type.");
                Check(!sort.d.HasValue || sort.d == SortDirection.Ascending || sort.d == SortDirection.Descending, path + ".s.d", "Use ascending or descending sorting.");
            }

            return source;
        }

        internal void Select(Select select, IComposite source, string path, int depth = 0)
        {
            Shape(select, path, depth);
            var property = this.Property(select.a, select.r, source, path, true);
            Check(depth == 0 || property != null, path, "Every next select step requires a composite endpoint; put terminal includes on the preceding step.");
            Check(property == null || property.ObjectType.IsComposite, path, "Select a composite endpoint; unit roles belong in record payloads.");
            Check(property != null || select.n == null, path, "A select step with a next step requires a composite endpoint.");
            var target = (IComposite)property?.ObjectType ?? source;
            if (select.o != null)
            {
                var ofType = this.Composite(select.o, path + ".o");
                Check(property != null && Compatible(target, ofType), path + ".o", "Apply a compatible OfType constraint to a composite select endpoint.");
                target = ofType;
            }

            Check(select.n == null || select.i == null, path, "Put includes on the terminal select step.");
            if (select.n != null)
            {
                this.Select(select.n, target, path + ".n", depth + 1);
            }
            else
            {
                this.Nodes(select.i, target, path + ".i", depth + 1);
            }
        }

        private static bool Compatible(IComposite first, IComposite second) => first.DatabaseClasses.Intersect(second.DatabaseClasses).Any();

        private static void Shape<T>(T value, string path, int depth) where T : class
        {
            Check(value != null && value.GetType() == typeof(T), path, "Supply a built-in declarative DTO, not a null or custom executable definition.");
            Check(depth < 48, path, "Reduce declaration nesting and remove cycles.");
        }

        private void Nodes(Node[] nodes, IComposite source, string path, int depth)
        {
            foreach (var node in nodes ?? Array.Empty<Node>())
            {
                Shape(node, path, depth);
                var property = this.Property(node.a, node.r, source, path);
                Check(property.ObjectType.IsComposite, path, "Include a composite endpoint; unit roles are materialized with their record.");
                this.Nodes(node.n, (IComposite)property.ObjectType, path + ".n", depth + 1);
            }
        }

        private void Predicate(Predicate predicate, IComposite source, string path, int depth)
        {
            Shape(predicate, path, depth);
            var allowed = predicate.k switch
            {
                PredicateKind.And or PredicateKind.Or => "ops d",
                PredicateKind.Not => "op d",
                PredicateKind.InstanceOf => "a r o p d",
                PredicateKind.Exists => "a r d",
                PredicateKind.Equals => "a r ob v pa p d",
                PredicateKind.Contains => "a r ob p d",
                PredicateKind.ContainedIn => "a r obs e p d",
                PredicateKind.Between => "r vs pas p d",
                PredicateKind.GreaterThan or PredicateKind.LessThan => "r v pa p d",
                PredicateKind.Like => "r v p d",
                _ => throw new ReadRequestException(path + ": Use a supported built-in predicate kind."),
            };
            var fields = new (string Name, object Value)[]
            {
                ("a", predicate.a), ("r", predicate.r), ("o", predicate.o), ("p", predicate.p), ("d", predicate.d),
                ("op", predicate.op), ("ops", predicate.ops), ("ob", predicate.ob), ("obs", predicate.obs),
                ("v", predicate.v), ("vs", predicate.vs), ("pa", predicate.pa), ("pas", predicate.pas), ("e", predicate.e),
            };
            foreach (var field in fields)
            {
                Check(field.Value == null || allowed.Split(' ').Contains(field.Name), path + "." + field.Name, "Remove fields that do not belong to this predicate kind.");
            }

            if (predicate.k == PredicateKind.And || predicate.k == PredicateKind.Or)
            {
                Check(predicate.ops?.Length > 0, path, "Supply at least one boolean operand.");
                foreach (var operand in predicate.ops)
                {
                    this.Predicate(operand, source, path + ".ops", depth + 1);
                }

                return;
            }

            if (predicate.k == PredicateKind.Not)
            {
                Check(predicate.op != null, path, "Supply the negated operand.");
                this.Predicate(predicate.op, source, path + ".op", depth + 1);
                return;
            }

            var property = this.Property(predicate.a, predicate.r, source, path,
                predicate.k == PredicateKind.InstanceOf || predicate.k == PredicateKind.Equals);
            var role = property as IRoleType;
            if (predicate.k == PredicateKind.Exists)
            {
                return;
            }

            var sources = (predicate.o != null ? 1 : 0) + (predicate.ob.HasValue ? 1 : 0) + (predicate.obs != null ? 1 : 0)
                + (predicate.v != null ? 1 : 0) + (predicate.vs != null ? 1 : 0) + (predicate.pa != null ? 1 : 0)
                + (predicate.pas != null ? 1 : 0) + (predicate.e != null ? 1 : 0) + (predicate.p != null ? 1 : 0);
            Check(sources == 1, path, "Supply exactly one value, object, extent, path, or parameter source for this predicate.");
            if (predicate.k == PredicateKind.InstanceOf)
            {
                Check(property == null || property.ObjectType.IsComposite, path, "Use InstanceOf on objects or composite endpoints.");
                var tag = predicate.o;
                if (predicate.p != null && this.arguments?.TryGetValue(predicate.p, out var argument) == true)
                {
                    tag = this.convert.StringFromJson(argument);
                }

                if (tag != null)
                {
                    var ofType = this.Composite(tag, path + ".o");
                    Check(Compatible((IComposite)property?.ObjectType ?? source, ofType), path, "Use a type compatible with the predicate's object endpoint.");
                }

                return;
            }

            if (predicate.k == PredicateKind.Contains || predicate.k == PredicateKind.ContainedIn ||
                (predicate.k == PredicateKind.Equals && (property == null || property.ObjectType.IsComposite)))
            {
                Check(property == null || property.ObjectType.IsComposite, path, "Compare object references on composite endpoints.");
                Check(predicate.k != PredicateKind.Contains || property.IsMany, path, "Use Contains with a to-many endpoint; use Equals with a to-one endpoint.");
                Check(predicate.k != PredicateKind.Equals || property == null || property.IsOne, path, "Use Equals with a to-one endpoint; use Contains with a to-many endpoint.");
                Check(predicate.pa == null && predicate.v == null, path, "Encode an object comparison using ob or a parameter containing an object ID.");
                if (predicate.e != null)
                {
                    var target = this.Extent(predicate.e, path + ".e", depth + 1);
                    Check(Compatible((IComposite)property.ObjectType, target), path, "Use an extent compatible with the compared endpoint.");
                }

                if (predicate.p != null && this.arguments?.TryGetValue(predicate.p, out var argument) == true)
                {
                    if (predicate.k == PredicateKind.ContainedIn)
                    {
                        Check(this.convert.LongArrayFromJson(argument)?.All(id => id > 0 && id <= 9007199254740991) == true, path, "Supply positive committed object IDs for this parameter.");
                    }
                    else
                    {
                        var id = this.convert.LongFromJson(argument);
                        Check(id > 0 && id <= 9007199254740991, path, "Supply a positive committed object ID for this parameter.");
                        if (predicate.k == PredicateKind.Equals && property == null)
                        {
                            // The built-in self-equality predicate consumes its Object member.
                            predicate.ob = id;
                            predicate.p = null;
                        }
                    }
                }

                return;
            }

            Check(role?.ObjectType.IsUnit == true, path, "Use this comparison with a unit role.");
            Check(!predicate.ob.HasValue, path, "Compare unit roles using encoded values, matching role paths, or parameters.");
            Check(predicate.k != PredicateKind.Like || role.ObjectType.Tag == UnitTags.String, path, "Use Like with a string role.");
            if (predicate.pa != null)
            {
                this.Path(predicate.pa, role, source, path);
            }

            if (predicate.pas != null)
            {
                Check(predicate.pas.Length == 2, path, "Supply exactly two Between role paths.");
                foreach (var tag in predicate.pas)
                {
                    this.Path(tag, role, source, path);
                }
            }

            if (predicate.vs != null)
            {
                Check(predicate.vs.Length == 2, path, "Supply exactly two Between values.");
                foreach (var value in predicate.vs)
                {
                    this.Unit(role, value, path);
                }
            }

            if (predicate.v != null)
            {
                this.Unit(role, predicate.v, path);
            }

            if (predicate.p != null && this.arguments?.TryGetValue(predicate.p, out var parameter) == true)
            {
                if (predicate.k == PredicateKind.Between)
                {
                    Check(parameter is JsonElement { ValueKind: JsonValueKind.Array } array && array.GetArrayLength() == 2, path, "Supply exactly two Between parameter values.");
                    foreach (var value in ((JsonElement)parameter).EnumerateArray())
                    {
                        this.Unit(role, value, path);
                    }
                }
                else
                {
                    this.Unit(role, parameter, path);
                }
            }
        }

        private void Path(string tag, IRoleType role, IComposite source, string path)
        {
            var other = this.Property(null, tag, source, path) as IRoleType;
            Check(other?.ObjectType == role.ObjectType, path, "Compare role paths with the same unit type.");
        }

        private void Unit(IRoleType role, object value, string path)
        {
            try
            {
                var converted = this.convert.UnitFromJson(role.ObjectType.Tag, value);
                var valid = role.ObjectType.Tag switch
                {
                    UnitTags.Binary => converted is byte[],
                    UnitTags.Boolean => converted is bool,
                    UnitTags.DateTime => converted is DateTime,
                    UnitTags.Decimal => converted is decimal,
                    UnitTags.Float => converted is double number && !double.IsNaN(number) && !double.IsInfinity(number),
                    UnitTags.Integer => converted is int,
                    UnitTags.String => converted is string,
                    UnitTags.Unique => converted is Guid,
                    _ => false,
                };
                Check(valid, path, "Supply a nonnull value encoded for the selected unit role.");
            }
            catch (Exception exception) when (exception is InvalidCastException || exception is InvalidOperationException || exception is ArgumentException || exception is OverflowException)
            {
                throw new ReadRequestException(path + ": Supply a value encoded for the selected unit role.", exception);
            }
        }
    }
}
