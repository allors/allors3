// <copyright file="DefaultDomainTransactionServices.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Default type.</summary>
//------------------------------------------------------------------------------------------------

namespace Allors.Database
{
    public class DefaultDomainTransactionServices : IDomainTransactionServices
    {
        public void OnInit(ITransaction transaction) { }

        public void Dispose() { }

        public T Get<T>() => default;
    }
}
