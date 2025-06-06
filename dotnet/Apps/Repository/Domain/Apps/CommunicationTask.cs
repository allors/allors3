// <copyright file="CommunicationTask.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("14D5613A-412F-41A7-9F94-4E10DF9FFEF0")]
    #endregion
    public partial class CommunicationTask : Task, UniquelyIdentifiable
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public WorkItem WorkItem { get; set; }

        public string Title { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateDue { get; set; }

        public DateTime DateClosed { get; set; }

        public User[] Participants { get; set; }

        public User Performer { get; set; }

        #endregion

        #region Allors
        [Id("D6495C2B-2396-4C90-A04D-E689E0BC20E7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public CommunicationEvent CommunicationEvent { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
