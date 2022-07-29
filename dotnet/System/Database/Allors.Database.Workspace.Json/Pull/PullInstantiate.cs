// <copyright file="PullInstantiate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Data;
    using Security;

    public class PullInstantiate
    {
        private readonly ITransaction transaction;
        private readonly Pull pull;
        private readonly IAccessControl acls;
        private readonly IPreparedSelects preparedSelects;

        public PullInstantiate(ITransaction transaction, Pull pull, IAccessControl acls, IPreparedSelects preparedSelects)
        {
            this.transaction = transaction;
            this.pull = pull;
            this.acls = acls;
            this.preparedSelects = preparedSelects;
        }

        public void Execute(PullResponseBuilder response)
        {
            var @object = this.transaction.Instantiate(this.pull.Object);

            var @class = @object.Strategy?.Class;

            if (@class != null && this.pull.ObjectType is IComposite objectType && !objectType.IsAssignableFrom(@class))
            {
                return;
            }

            if (this.pull.Results != null)
            {
                foreach (var result in this.pull.Results)
                {
                    try
                    {
                        var name = result.Name;

                        var @select = result.Select;
                        if (@select == null && result.SelectRef.HasValue)
                        {
                            @select = this.preparedSelects.Get(result.SelectRef.Value);
                        }

                        if (@select != null)
                        {
                            var include = @select.End.Include;

                            if (@select.PropertyType != null)
                            {
                                var propertyType = @select.End.PropertyType;

                                if (@select.IsOne)
                                {
                                    name ??= propertyType.SingularName;

                                    @object = (IObject)@select.Get(@object, this.acls);
                                    response.AddObject(name, @object, include);
                                }
                                else
                                {
                                    name ??= propertyType.PluralName;

                                    var stepResult = @select.Get(@object, this.acls);

                                    IObject[] objects;
                                    if (stepResult is IObject obj)
                                    {
                                        objects = new[] { obj };
                                    }
                                    else if (stepResult is IEnumerable<IObject> objs)
                                    {
                                        objects = objs.ToArray();
                                    }
                                    else if (stepResult is IEnumerable<object> outer)
                                    {
                                        var set = new HashSet<IObject>();
                                        foreach (var inner in outer)
                                        {
                                            switch (inner)
                                            {
                                                case null:
                                                    continue;
                                                case IObject innerObject:
                                                    set.Add(innerObject);
                                                    break;
                                                default:
                                                {
                                                    var innerEnumerable = (IEnumerable<IObject>)inner;
                                                    set.UnionWith(innerEnumerable);
                                                    break;
                                                }
                                            }
                                        }

                                        objects = set.ToArray();
                                    }
                                    else
                                    {
                                        objects = Array.Empty<IObject>();
                                    }

                                    response.AddCollection(name, (IComposite)@select.GetObjectType() ?? @class, objects, include);
                                }
                            }
                            else
                            {
                                name ??= this.pull.ObjectType?.Name ?? @object.Strategy.Class.SingularName;
                                response.AddObject(name, @object, include);
                            }
                        }
                        else
                        {
                            name ??= this.pull.ObjectType?.Name ?? @object.Strategy.Class.SingularName;
                            var include = result.Include;
                            response.AddObject(name, @object, include);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Instantiate: {@object?.Strategy.Class}[{@object?.Strategy.ObjectId}], {result}", e);
                    }
                }
            }
            else
            {
                var name = this.pull.ObjectType?.Name ?? @object.Strategy.Class.SingularName;
                response.AddObject(name, @object);
            }
        }
    }
}
