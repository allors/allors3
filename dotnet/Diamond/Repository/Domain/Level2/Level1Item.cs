// <copyright file="Level1Item.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    // Level2 extends the model of Level1: a role on Level1's class, declared in Level2's own folder.
    public partial class Level1Item
    {
        #region Allors
        [Id("40063883-5fc1-45e1-abd9-73d483af165c")]
        #endregion
        [Size(256)]
        public string Level2Note { get; set; }
    }
}
