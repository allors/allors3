// <copyright file="IDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using Excel;

    public interface IExcelServices
    {
        IErrorService ErrorService { get; }

        ILoggerService LoggerService { get; }

        IMessageService MessageService { get; }

        IUserIdService UserIdService { get; }

        IRibbonService RibbonService { get; }
    }
}
