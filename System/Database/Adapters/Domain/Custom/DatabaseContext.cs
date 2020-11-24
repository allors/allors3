// <copyright file="Subscriber.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Default type.</summary>
//------------------------------------------------------------------------------------------------

namespace Allors.Database
{
    using Meta;

    public class DatabaseContext : IDatabaseContext
    {
        public void OnInit(IDatabase database) => this.M = new M((MetaPopulation)database.ObjectFactory.MetaPopulation);

        public ISessionLifecycle CreateSessionInstance() => new SessionContext();

        public M M { get; set; }
        public void Dispose() { }
    }
}