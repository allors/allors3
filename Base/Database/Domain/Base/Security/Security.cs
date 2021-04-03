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
        public void Deny(MetaClass objectType, ObjectState objectState, params Operations[] operations) => this.Deny(objectType.ObjectType, objectState, operations);

        public void Deny(MetaClass objectType, ObjectState objectState, params IOperandType[] operandTypes) => this.Deny(objectType.ObjectType, objectState, (IEnumerable<OperandType>)operandTypes);

        public void Deny(MetaClass objectType, ObjectState objectState, IEnumerable<IOperandType> operandTypes) => this.Deny(objectType.ObjectType, objectState, operandTypes);

        public void DenyExcept(MetaClass objectType, ObjectState objectState, IEnumerable<IOperandType> excepts, params Operations[] operations) => this.DenyExcept(objectType.ObjectType, objectState, excepts, operations);
        
        public void Deny(IObjectType objectType, ObjectState objectState, params Operations[] operations)
        {
            var actualOperations = operations ?? ReadWriteExecute;
            foreach (var operation in actualOperations)
            {
                Dictionary<IOperandType, Permission> permissionByOperandType;
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

        public void Deny(IObjectType objectType, ObjectState objectState, params IOperandType[] operandTypes) => this.Deny(objectType, objectState, (IEnumerable<IOperandType>)operandTypes);

        public void Deny(IObjectType objectType, ObjectState objectState, IEnumerable<IOperandType> operandTypes)
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

        public void DenyExcept(IObjectType objectType, ObjectState objectState, IEnumerable<IOperandType> excepts, params Operations[] operations)
        {
            var actualOperations = operations ?? ReadWriteExecute;
            foreach (var operation in actualOperations)
            {
                Dictionary<IOperandType, Permission> permissionByOperandType;
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
