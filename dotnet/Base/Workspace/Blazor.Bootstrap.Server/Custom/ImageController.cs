// <copyright file="MediaController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using Allors.Services;

    public class ImageController : BaseImageController
    {
        public ImageController(ITransactionService sessionService)
            : base(sessionService)
        {
        }
    }
}
