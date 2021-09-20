// <copyright file="ApproveTask.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    /// <summary>
    /// A <see cref="Task"/> that can be approved or rejected.
    /// </summary>
    #region Allors
    [Id("b86d8407-c411-49e4-aae3-64192457c701")]
    #endregion
    public partial interface ApproveTask : Task
    {
        /// <summary>
        /// A text the user can enter when approving or rejecting a task.
        /// </summary>
        #region Allors
        [Id("a280bf60-2eb7-488a-abf7-f03c9d9197b5")]
        [Size(-1)]
        #endregion
        [Workspace(Default)]
        string Comment { get; set; }

        /// <summary>
        /// The <see cref="Notification"/> that is created when this task is rejected.
        /// </summary>
        #region Allors
        [Id("a7c646a2-7aaa-44ae-9240-77b3b6f2e8fa")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        [Derived]
        Notification RejectionNotification { get; set; }

        /// <summary>
        /// The <see cref="Notification"/> that is created when this task is approved.
        /// </summary>
        #region Allors
        [Id("4AF7D84E-393F-402F-8E76-044A75F77543")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        [Derived]
        Notification ApprovalNotification { get; set; }

        /// <summary>
        /// Approve this task.
        /// </summary>
        #region Allors
        [Id("0158D8F3-3E9F-48B3-AD25-51BD7EABC27C")]
        #endregion
        [Workspace(Default)]
        void Approve();

        /// <summary>
        /// Reject this task.
        /// </summary>
        #region Allors
        [Id("F68B3D21-0108-40EC-9455-98764EB74874")]
        #endregion
        [Workspace(Default)]
        void Reject();
    }
}
