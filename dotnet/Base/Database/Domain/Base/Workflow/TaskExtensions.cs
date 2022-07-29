// <copyright file="TaskExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class TaskExtensions
    {
        public static void BaseOnBuild(this Task @this, ObjectOnBuild method)
        {
            if (!@this.ExistDateCreated)
            {
                @this.DateCreated = @this.Strategy.Transaction.Now();
            }
        }
        
        public static void BaseDelete(this Task @this, DeletableDelete _)
        {
            foreach (var taskAssignment in @this.TaskAssignmentsWhereTask)
            {
                taskAssignment.Delete();
            }
        }

        public static void AssignPerformer(this Task @this) => @this.Performer = @this.Strategy.Transaction.Services.Get<IUserService>().User as Person;

        public static void AssignParticipants(this Task @this, IEnumerable<User> participants)
        {
            var transaction = @this.Strategy.Transaction;

            var participantSet = new HashSet<User>(participants.Where(v => v != null).Distinct());

            @this.Participants = participantSet.ToArray();

            // Manage Security
            var defaultSecurityToken = new SecurityTokens(transaction).DefaultSecurityToken;
            var securityTokens = new HashSet<SecurityToken> { defaultSecurityToken };
            var ownerSecurityTokens = participantSet.Where(v => v.ExistOwnerSecurityToken).Select(v => v.OwnerSecurityToken);
            securityTokens.UnionWith(ownerSecurityTokens);
            @this.SecurityTokens = securityTokens.ToArray();

            // Manage TaskAssignments
            foreach (var currentTaskAssignement in @this.TaskAssignmentsWhereTask.ToArray())
            {
                var user = currentTaskAssignement.User;
                if (!participantSet.Contains(user))
                {
                    currentTaskAssignement.Delete();
                }
                else
                {
                    participantSet.Remove(user);
                }
            }

            foreach (var user in participantSet)
            {
                new TaskAssignmentBuilder(transaction)
                    .WithTask(@this)
                    .WithUser(user)
                    .Build();
            }
        }
    }
}
