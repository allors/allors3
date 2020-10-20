// <copyright file="IDatabaseState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using State;

    public partial interface IDatabaseState
    {
        ITemplateObjectCache TemplateObjectCache { get; }

        IMailer Mailer { get; }

        ISingletonId SingletonId { get; }

        IBarcodeGenerator BarcodeGenerator { get; }
    }
}
