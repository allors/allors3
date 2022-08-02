// <copyright file="AllorsTestUtils.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    public class AllorsTestUtils
    {
        public static void ForceRoleCaching(IObject allorsObject)
        {
            foreach (var role in allorsObject.Strategy.Class.DatabaseRoleTypes)
            {
                allorsObject.Strategy.GetRole(role);
            }
        }

        public static void ForceAssociationCaching(IObject allorsObject)
        {
            foreach (var association in allorsObject.Strategy.Class.DatabaseAssociationTypes)
            {
                allorsObject.Strategy.GetAssociation(association);
            }
        }
    }
}
