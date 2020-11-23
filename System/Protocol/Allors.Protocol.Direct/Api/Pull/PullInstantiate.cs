// <copyright file="PullInstantiate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ISessionExtension type.</summary>

namespace Allors.Protocol.Direct.Api.Pull
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Database;
    using Database.Data;
    using Database.Meta;
    using Database.Security;
    using Extent = Database.Extent;

    public class PullInstantiate
    {
        public PullInstantiate(Pull pull) => this.Pull = pull;

        private Pull Pull { get; }

        public void Execute(PullResponseBuilder response)
        {
            // TODO:
            //var @object = response.Api.Session.Instantiate(this.Pull.Object);

            //var @class = @object.Strategy?.Class;

            //if (@class != null && this.Pull.ObjectType is IComposite objectType)
            //{
            //    if (!objectType.IsAssignableFrom(@class))
            //    {
            //        return;
            //    }
            //}

            //if (this.Pull.Results != null)
            //{
            //    foreach (var result in this.Pull.Results)
            //    {
            //        try
            //        {
            //            var name = result.Name;

            //            var fetch = result.Fetch;
            //            if ((fetch == null) && result.FetchRef.HasValue)
            //            {
            //                fetch = response.Api.PreparedFetches.Get(result.FetchRef.Value);
            //            }

            //            if (fetch != null)
            //            {
            //                var include = fetch.Include ?? fetch.Step?.End.Include;

            //                if (fetch.Step != null)
            //                {
            //                    var propertyType = fetch.Step.End.PropertyType;

            //                    if (fetch.Step.IsOne)
            //                    {
            //                        name ??= propertyType.SingularName;

            //                        @object = (IObject)fetch.Step.Get(@object, response.Api.AccessControlLists);
            //                        response.AddObject(name, @object, include);
            //                    }
            //                    else
            //                    {
            //                        name ??= propertyType.PluralName;

            //                        var stepResult = fetch.Step.Get(@object, response.Api.AccessControlLists);
            //                        var objects = stepResult is HashSet<object> set ? set.Cast<IObject>().ToArray() : ((Extent)stepResult)?.ToArray() ?? new IObject[0];

            //                        if (result.Skip.HasValue || result.Take.HasValue)
            //                        {
            //                            var paged = result.Skip.HasValue ? objects.Skip(result.Skip.Value) : objects;
            //                            if (result.Take.HasValue)
            //                            {
            //                                paged = paged.Take(result.Take.Value);
            //                            }

            //                            paged = paged.ToArray();

            //                            response.AddValue(name + "_total", objects.Length.ToString());
            //                            response.AddCollection(name, paged, include);
            //                        }
            //                        else
            //                        {
            //                            response.AddCollection(name, objects, include);
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    name ??= this.Pull.ObjectType?.Name ?? @object.Strategy.Class.SingularName;
            //                    response.AddObject(name, @object, include);
            //                }
            //            }
            //            else
            //            {
            //                name ??= this.Pull.ObjectType?.Name ?? @object.Strategy.Class.SingularName;
            //                response.AddObject(name, @object);
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            throw new Exception($"Instantiate: {@object?.Strategy.Class}[{@object?.Strategy.ObjectId}], {result}", e);
            //        }
            //    }
            //}
            //else
            //{
            //    var name = this.Pull.ObjectType?.Name ?? @object.Strategy.Class.SingularName;
            //    response.AddObject(name, @object);
            //}
        }
    }
}
