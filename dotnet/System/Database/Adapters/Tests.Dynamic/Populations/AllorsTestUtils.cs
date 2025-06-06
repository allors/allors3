// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AllorsTestUtils.cs" company="Allors bv">
//   Copyright 2002-2012 Allors bv.
//
// Dual Licensed under
//   a) the Lesser General Public Licence v3 (LGPL)
//   b) the Allors License
//
// The LGPL License is included in the file lgpl.txt.
// The Allors License is an addendum to your contract.
//
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
