// <copyright file="IDomainDatabaseServices.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial interface IDomainDatabaseServices
    {
        ITemplateObjectCache TemplateObjectCache { get; }

        IMailer Mailer { get; }

        ISingletonId SingletonId { get; }

        IBarcodeGenerator BarcodeGenerator { get; }
    }
}
