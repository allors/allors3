// <copyright file="Project.v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Newtonsoft.Json.Linq;

namespace Autotest.Angular
{
    public partial class Project
    {
        public void Load(JObject jsonProject)
        {
            this.BaseLoad(jsonProject);

            this.CustomLoad(jsonProject);
        }
    }
}
