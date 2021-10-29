// <copyright file="Test.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;

    public static class ISessionExtensions
    {
        public static async Task<T> PullObject<T>(this ISession @this, string name) where T : class, IObject
        {
            var objectType = (IComposite)@this.Workspace.Configuration.ObjectFactory.GetObjectType<T>();
            var roleType = objectType.RoleTypes.First(v => v.Name.Equals("Name"));
            var pull = new Pull { Extent = new Filter(objectType) { Predicate = new Equals(roleType) { Value = name } } };
            var result = await @this.PullAsync(pull);
            var collection = result.GetCollection<T>();
            return collection[0];
        }
    }
}
