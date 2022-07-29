// <copyright file="Subscriber.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Default type.</summary>
//------------------------------------------------------------------------------------------------

namespace Allors.Database
{
    using Meta;

    public class DefaultDomainDatabaseServices : IDomainDatabaseServices
    {
        public void OnInit(IDatabase database) => this.M = (MetaPopulation)database.ObjectFactory.MetaPopulation;

        public ITransactionServices CreateTransactionServices() => new DefaultDomainTransactionServices();

        public MetaPopulation M { get; private set; }

        public T Get<T>() => default;

        public void Dispose() { }
    }
}
