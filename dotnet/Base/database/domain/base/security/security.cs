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
        public void Deny(IObjectType objectType, ObjectState objectState, params Operations[] operations)
        {
            if (objectState != null)
            {
                foreach (var operation in operations ?? ReadWriteExecute)
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
                            objectState.ObjectRevocation.AddDeniedPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }

        public void Deny(IObjectType objectType, ObjectState objectState, params IOperandType[] operandTypes) => this.Deny(objectType, objectState, (IEnumerable<IOperandType>)operandTypes);

        public void Deny(IObjectType objectType, ObjectState objectState, IEnumerable<IOperandType> operandTypes)
        {
            if (objectState != null)
            {
                if (this.deniablePermissionByOperandTypeByObjectTypeId.TryGetValue(objectType.Id, out var deniablePermissionByOperandTypeId))
                {
                    foreach (var operandType in operandTypes)
                    {
                        if (deniablePermissionByOperandTypeId.TryGetValue(operandType, out var permission))
                        {
                            objectState.ObjectRevocation.AddDeniedPermission(permission);
                        }
                    }
                }
            }
        }

        public void DenyExcept(IObjectType objectType, ObjectState objectState, IEnumerable<IOperandType> excepts, params Operations[] operations)
        {
            if (objectState != null)
            {

                foreach (var operation in operations ?? ReadWriteExecute)
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
                            objectState.ObjectRevocation.AddDeniedPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }

        public void Grant(IObjectType objectType, ObjectState objectState, IEnumerable<IOperandType> grants, params Operations[] operations)
        {
            if (objectState != null)
            {
                foreach (var operation in operations ?? ReadWriteExecute)
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
                        foreach (var dictionaryEntry in permissionByOperandType.Where(v => grants.Contains(v.Key)))
                        {
                            objectState.ObjectRevocation.RemoveDeniedPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }

        private void BaseOnPreSetup()
        {
            foreach (var revocation in this.transaction.Extent<Revocation>().Where(v => v.ExistObjectStatesWhereObjectRevocation))
            {
                revocation.RemoveDeniedPermissions();
            }
        }

        private void BaseOnPostSetup()
        {
        }
    }
}
