// <copyright file="Build.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    /// <summary>
    /// Shared.
    /// </summary>
    public partial class Cascaded
    {
        public void CustomDelete(DeletableDelete method)
        {
            if (!this.IsDeleting())
            {
                throw new Exception("I should be deleting!");
            }

            var cascader = this.CascaderWhereCascaded;

            if (!cascader.IsDeleting())
            {
                throw new Exception("My Cascader should be deleting too!");
            }
        }
    }
}
