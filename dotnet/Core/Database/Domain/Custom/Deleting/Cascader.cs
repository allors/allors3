// <copyright file="Build.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    /// <summary>
    /// Shared.
    /// </summary>
    public partial class Cascader
    {
        public void CustomDelete(DeletableDelete method)
        {
            if (!this.IsDeleting())
            {
                throw new Exception("I should be deleting!");
            }

            var cascaded = this.Cascaded;

            if (cascaded.IsDeleting())
            {
                throw new Exception("My Cascade should not be deleting!");
            }

            this.Cascaded?.Delete();

            if (cascaded.IsDeleting())
            {
                throw new Exception("My Cascade should not be deleting!");
            }
        }
    }
}
