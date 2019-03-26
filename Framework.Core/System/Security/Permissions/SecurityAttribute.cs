#if (LESSTHAN_NETCOREAPP20 || LESSTHAN_NETSTANDARD20) && !PROFILE328

namespace System.Security.Permissions
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    [Runtime.InteropServices.ComVisible(true)]
    public abstract class SecurityAttribute : Attribute
    {
        protected SecurityAttribute(SecurityAction action)
        {
            Action = action;
        }

        public SecurityAction Action { get; set; }

        public bool Unrestricted { get; set; }

        public abstract IPermission CreatePermission();
    }
}

#endif