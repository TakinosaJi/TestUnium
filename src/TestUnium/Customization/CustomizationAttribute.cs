﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TestUnium.Instantiation.Prioritizing;

namespace TestUnium.Customization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class CustomizationAttribute : Attribute, IComparable<CustomizationAttribute>, ICancellable, ICustomizationAttribute
    {
        private readonly Type _targetType;
        public UInt16 Priority { get; set; }
        public Boolean Visible { get; set; }
        public IEnumerable<Type> CancellationList { get; set; }

        protected CustomizationAttribute(Type targetType, UInt16 priority = 0) : this(targetType, new List<Type>(), priority) { }

        protected CustomizationAttribute(Type targetType, IEnumerable<Type> cancellationCollection, UInt16 priority = 0)
        {
            _targetType = targetType;
            Visible = true;        
            CancellationList = cancellationCollection;
            var attr = GetType().GetCustomAttribute(typeof(PriorityAttribute)) as PriorityAttribute;
            if (attr != null)
            {
                Priority = attr.Value;
                return;
            }
            Priority = priority;
        }

        public Boolean CheckCancellationClause(IEnumerable<Type> invocationList)
        {
            var result = false;
            var types = CancellationList as List<Type>;
            types?.ForEach(cancelItem =>
            {
                if (invocationList.Any(invItem => cancelItem.Name.Equals(invItem.Name)))
                {
                    result = false;
                }
            });
            return result;
        }

        /// <summary>
        /// Customiation attributes with priority == 0 are being processed at last turn inside each family.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(CustomizationAttribute other)
        {
            var mineTargetType = GetCustomizationTargetType();
            var othersTargetType = other.GetCustomizationTargetType();
            if (!typeof(ICustomizationAttributeDrivenTest).IsAssignableFrom(mineTargetType))
                throw new IncorrectCustomizationTargetTypeException(mineTargetType.Name);
            if (!typeof(ICustomizationAttributeDrivenTest).IsAssignableFrom(othersTargetType))
                throw new IncorrectCustomizationTargetTypeException(othersTargetType.Name);
            if (mineTargetType.IsSubclassOf(othersTargetType)) return 1;
            if (othersTargetType.IsSubclassOf(mineTargetType)) return -1;
            return Priority == 0 ? 1 : other.Priority == 0 ? - 1 : Priority - other.Priority;
        }

        public Type GetCustomizationTargetType() => _targetType;
    }
}
