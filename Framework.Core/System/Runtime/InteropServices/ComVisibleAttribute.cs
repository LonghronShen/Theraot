﻿#if PROFILE328

namespace System.Runtime.InteropServices
{

    /// <summary>
    /// Controls accessibility of an individual managed type or member, or of all types
    /// within an assembly, to COM.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    public sealed class ComVisibleAttribute 
        : Attribute
    {

        /// <summary>
        /// Gets a value that indicates whether the COM type is visible.
        /// </summary>
        /// <returns>
        /// true if the type is visible; otherwise, false. The default value is true.
        /// </returns>
        public bool Value { get; }

        /// <summary>
        /// Initializes a new instance of the ComVisibleAttribute class.
        /// </summary>
        /// <param name="visibility">
        /// true to indicate that the type is visible to COM; otherwise, false. The default is true.
        /// </param>
        public ComVisibleAttribute(bool visibility)
        {
            this.Value = visibility;
        }

    }

}

#endif