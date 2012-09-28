﻿using System;
//using System.Dynamic;

//Not using it here, but @marcgravell's stuff is too good not to include
#if !SILVERLIGHT && !MONOTOUCH && !XBOX
namespace FastMember
{
    /// <summary>
    /// Represents an individual object, allowing access to members by-name
    /// </summary>
    public abstract class ObjectAccessor
    {
        /// <summary>
        /// Get or Set the value of a named member for the underlying object
        /// </summary>
        public abstract object this[string name] { get; set; }
        /// <summary>
        /// The object represented by this instance
        /// </summary>
        public abstract object Target { get; }
        /// <summary>
        /// Use the target types definition of equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return Target.Equals(obj);
        }
        /// <summary>
        /// Obtain the hash of the target object
        /// </summary>
        public override int GetHashCode()
        {
            return Target.GetHashCode();
        }
        /// <summary>
        /// Use the target's definition of a string representation
        /// </summary>
        public override string ToString()
        {
            return Target.ToString();
        }

        /// <summary>
        /// Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target)
        {
            if (target == null) throw new ArgumentNullException("target");
            //IDynamicMetaObjectProvider dlr = target as IDynamicMetaObjectProvider;
            //if (dlr != null) return new DynamicWrapper(dlr); // use the DLR
            return new TypeAccessorWrapper(target, TypeAccessor.Create(target.GetType()));
        }

        sealed class TypeAccessorWrapper : ObjectAccessor
        {
            private readonly object target;
            private readonly TypeAccessor accessor;
            public TypeAccessorWrapper(object target, TypeAccessor accessor)
            {
                this.target = target;
                this.accessor = accessor;
            }
            public override object this[string name]
            {
                get { return accessor[target, name.ToUpperInvariant()]; }
				set { accessor[target, name.ToUpperInvariant()] = value; }
            }
            public override object Target
            {
                get { return target; }
            }
        }

		//sealed class DynamicWrapper : ObjectAccessor
		//{
		//    private readonly IDynamicMetaObjectProvider target;
		//    public override object Target
		//    {
		//        get { return target; }
		//    }
		//    public DynamicWrapper(IDynamicMetaObjectProvider target)
		//    {
		//        this.target = target;
		//    }
		//    public override object this[string name]
		//    {
		//        get { return CallSiteCache.GetValue(name, target); }
		//        set { CallSiteCache.SetValue(name, target, value); }
		//    }
        //}
    }

}

#endif