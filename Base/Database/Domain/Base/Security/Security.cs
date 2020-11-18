// <copyright file="Security.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public partial class Security
    {
        public void Deny(ObjectType objectType, ObjectState objectState, params Operations[] operations)
        {
            var actualOperations = operations ?? ReadWriteExecute;
            foreach (var operation in actualOperations)
            {
                Dictionary<OperandType, Permission> permissionByOperandType;
                switch (operation)
                {
                    case Operations.Read:
                        this.readPermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Write:
                        this.writePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Execute:
                        this.executePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    default:
                        throw new Exception("Unkown operation: " + operations);
                }

                if (permissionByOperandType != null)
                {
                    foreach (var dictionaryEntry in permissionByOperandType)
                    {
                        objectState.AddDeniedPermission(dictionaryEntry.Value);
                    }
                }
            }
        }

        public void Deny(ObjectType objectType, ObjectState objectState, params OperandType[] operandTypes) => this.Deny(objectType, objectState, (IEnumerable<OperandType>)operandTypes);

        public void Deny(ObjectType objectType, ObjectState objectState, IEnumerable<OperandType> operandTypes)
        {
            if (this.deniablePermissionByOperandTypeByObjectTypeId.TryGetValue(objectType.Id, out var deniablePermissionByOperandTypeId))
            {
                foreach (var operandType in operandTypes)
                {
                    if (deniablePermissionByOperandTypeId.TryGetValue(operandType, out var permission))
                    {
                        objectState.AddDeniedPermission(permission);
                    }
                }
            }
        }

        public void DenyExcept(ObjectType objectType, ObjectState objectState, IEnumerable<OperandType> excepts, params Operations[] operations)
        {
            var actualOperations = operations ?? ReadWriteExecute;
            foreach (var operation in actualOperations)
            {
                Dictionary<OperandType, Permission> permissionByOperandType;
                switch (operation)
                {
                    case Operations.Read:
                        this.readPermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Write:
                        this.writePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Execute:
                        this.executePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    default:
                        throw new Exception("Unkown operation: " + operations);
                }

                if (permissionByOperandType != null)
                {
                    foreach (var dictionaryEntry in permissionByOperandType.Where(v => !excepts.Contains(v.Key)))
                    {
                        objectState.AddDeniedPermission(dictionaryEntry.Value);
                    }
                }
            }
        }


        private void BaseOnPostSetup()
        {
        }

        private void BaseOnPreSetup()
        {
        }
    }
}
