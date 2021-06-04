// <copyright file="Security.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors;
    using Meta;

    public partial class Security
    {
        private static readonly Operations[] ReadWriteExecute = { Operations.Read, Operations.Write, Operations.Execute };

        private readonly Dictionary<Guid, Dictionary<IOperandType, Permission>> deniablePermissionByOperandTypeByObjectTypeId;
        private readonly Dictionary<Guid, Dictionary<IOperandType, Permission>> executePermissionsByObjectTypeId;
        private readonly Dictionary<ObjectType, IObjects> objectsByObjectType;
        private readonly Dictionary<Guid, Dictionary<IOperandType, Permission>> readPermissionsByObjectTypeId;
        private readonly Dictionary<Guid, Role> roleById;
        private readonly ITransaction transaction;
        private readonly Dictionary<Guid, Dictionary<IOperandType, Permission>> writePermissionsByObjectTypeId;

        public Security(ITransaction transaction)
        {
            this.transaction = transaction;

            this.objectsByObjectType = new Dictionary<ObjectType, IObjects>();
            foreach (ObjectType objectType in transaction.Database.MetaPopulation.DatabaseComposites)
            {
                this.objectsByObjectType[objectType] = objectType.GetObjects(transaction);
            }

            this.roleById = new Dictionary<Guid, Role>();
            foreach (Role role in transaction.Extent<Role>())
            {
                if (!role.ExistUniqueId)
                {
                    throw new Exception("Role " + role + " has no unique id");
                }

                this.roleById[role.UniqueId] = role;
            }

            this.readPermissionsByObjectTypeId = new Dictionary<Guid, Dictionary<IOperandType, Permission>>();
            this.writePermissionsByObjectTypeId = new Dictionary<Guid, Dictionary<IOperandType, Permission>>();
            this.executePermissionsByObjectTypeId = new Dictionary<Guid, Dictionary<IOperandType, Permission>>();

            this.deniablePermissionByOperandTypeByObjectTypeId = new Dictionary<Guid, Dictionary<IOperandType, Permission>>();

            foreach (Permission permission in transaction.Extent<Permission>())
            {
                if (!permission.ExistClassPointer || !permission.ExistOperation)
                {
                    throw new Exception("Permission " + permission + " has no concrete class, operand type and/or operation");
                }

                var objectId = permission.ClassPointer;

                if (permission.Operation != Operations.Read)
                {
                    var operandType = permission.OperandType;

                    if (!this.deniablePermissionByOperandTypeByObjectTypeId.TryGetValue(objectId, out var deniablePermissionByOperandTypeId))
                    {
                        deniablePermissionByOperandTypeId = new Dictionary<IOperandType, Permission>();
                        this.deniablePermissionByOperandTypeByObjectTypeId[objectId] = deniablePermissionByOperandTypeId;
                    }

                    deniablePermissionByOperandTypeId.Add(operandType, permission);
                }

                Dictionary<Guid, Dictionary<IOperandType, Permission>> permissionByOperandTypeByObjectTypeId;
                switch (permission.Operation)
                {
                    case Operations.Read:
                        permissionByOperandTypeByObjectTypeId = this.readPermissionsByObjectTypeId;
                        break;

                    case Operations.Write:
                        permissionByOperandTypeByObjectTypeId = this.writePermissionsByObjectTypeId;
                        break;

                    case Operations.Execute:
                        permissionByOperandTypeByObjectTypeId = this.executePermissionsByObjectTypeId;
                        break;

                    default:
                        throw new Exception("Unkown operation: " + permission.Operation);
                }

                if (!permissionByOperandTypeByObjectTypeId.TryGetValue(objectId, out var permissionByOperandType))
                {
                    permissionByOperandType = new Dictionary<IOperandType, Permission>();
                    permissionByOperandTypeByObjectTypeId[objectId] = permissionByOperandType;
                }

                if (permission.OperandType == null)
                {
                    permission.Delete();
                }
                else
                {
                    permissionByOperandType.Add(permission.OperandType, permission);
                }
            }
        }

        public void Apply()
        {
            foreach (Role role in this.transaction.Extent<Role>())
            {
                role.RemovePermissions();
                role.RemoveDeniedPermissions();
            }

            this.OnPreSetup();

            foreach (var objects in this.objectsByObjectType.Values)
            {
                objects.Secure(this);
            }

            this.OnPostSetup();

            this.transaction.Derive();
        }
        
        public void Grant(Guid roleId, ObjectType objectType, params Operations[] operations)
        {
            if (this.roleById.TryGetValue(roleId, out var role))
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
                            role.AddPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }

        public void Grant(Guid roleId, ObjectType objectType, IOperandType operandType, params Operations[] operations)
        {
            if (this.roleById.TryGetValue(roleId, out var role))
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
                        if (permissionByOperandType.TryGetValue(operandType, out var permission))
                        {
                            role.AddPermission(permission);
                        }
                    }
                }
            }
        }

        public void GrantAdministrator(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.AdministratorId, objectType, operations);

        public void GrantCreator(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.CreatorId, objectType, operations);

        public void GrantExcept(Guid roleId, ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations)
        {
            if (this.roleById.TryGetValue(roleId, out var role))
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
                            role.AddPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }

        public void GrantGuest(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.GuestId, objectType, operations);

        public void GrantGuestCreator(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.GuestCreatorId, objectType, operations);

        public void GrantOwner(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.OwnerId, objectType, operations);

        private void CoreOnPostSetup()
        {
        }

        private void CoreOnPreSetup()
        {
        }
    }
}
