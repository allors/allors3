// <copyright file="PropertyByObjectByPropertyType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ObjectBase type.</summary>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;
    using Meta;
    using Numbers;

    public class PropertyByObjectByPropertyType
    {
        private readonly INumbers numbers;
        private readonly IDictionary<IPropertyType, IDictionary<long, object>> propertyByObjectByPropertyType;
        private IDictionary<IPropertyType, IDictionary<long, object>> changedPropertyByObjectByPropertyType;

        public PropertyByObjectByPropertyType(INumbers numbers)
        {
            this.numbers = numbers;
            this.propertyByObjectByPropertyType = new Dictionary<IPropertyType, IDictionary<long, object>>();
            this.changedPropertyByObjectByPropertyType = new Dictionary<IPropertyType, IDictionary<long, object>>();
        }

        public object Get(long @object, IPropertyType propertyType)
        {
            if (this.changedPropertyByObjectByPropertyType.TryGetValue(propertyType, out var changedPropertyByObject) && changedPropertyByObject.TryGetValue(@object, out var changedValue))
            {
                return changedValue;
            }

            if (this.propertyByObjectByPropertyType.TryGetValue(propertyType, out var propertyByObject) && propertyByObject.TryGetValue(@object, out var value))
            {
                return value;
            }

            return null;
        }

        public void Set(long @object, IPropertyType propertyType, object newValue)
        {
            if (!(this.propertyByObjectByPropertyType.TryGetValue(propertyType, out var valueByPropertyType) && valueByPropertyType.TryGetValue(@object, out var originalValue)))
            {
                originalValue = null;
            }

            this.changedPropertyByObjectByPropertyType.TryGetValue(propertyType, out var changedValueByPropertyType);

            if (propertyType.IsOne ? Equals(newValue, originalValue) : this.numbers.AreEqual(newValue, originalValue))
            {
                changedValueByPropertyType?.Remove(@object);
            }
            else
            {
                if (changedValueByPropertyType == null)
                {
                    changedValueByPropertyType = new Dictionary<long, object>();
                    this.changedPropertyByObjectByPropertyType.Add(propertyType, changedValueByPropertyType);
                }

                changedValueByPropertyType[@object] = newValue;
            }
        }

        public IDictionary<IPropertyType, IDictionary<long, object>> Checkpoint()
        {
            try
            {
                var changesSet = this.changedPropertyByObjectByPropertyType;

                foreach (var kvp in changesSet)
                {
                    var propertyType = kvp.Key;
                    var changedPropertyByObject = kvp.Value;

                    this.propertyByObjectByPropertyType.TryGetValue(propertyType, out var propertyByObject);

                    foreach (var kvp2 in changedPropertyByObject)
                    {
                        var @object = kvp2.Key;
                        var changedProperty = kvp2.Value;

                        if (changedProperty == null)
                        {
                            propertyByObject?.Remove(@object);
                        }
                        else
                        {
                            if (propertyByObject == null)
                            {
                                propertyByObject = new Dictionary<long, object>();
                                this.propertyByObjectByPropertyType.Add(propertyType, propertyByObject);
                            }

                            propertyByObject[@object] = changedProperty;
                        }
                    }

                    if (propertyByObject?.Count == 0)
                    {
                        this.propertyByObjectByPropertyType.Remove(propertyType);
                    }
                }

                return changesSet;
            }
            finally
            {
                this.changedPropertyByObjectByPropertyType = new Dictionary<IPropertyType, IDictionary<long, object>>();
            }
        }
    }
}
