#if (LESSTHAN_NETCOREAPP20 || LESSTHAN_NETSTANDARD20) && !PROFILE328

namespace System.Security.Permissions
{
    public sealed class SecurityPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        public SecurityPermission(PermissionState state)
        {
            Theraot.No.Op(state);
        }

        public SecurityPermission(SecurityPermissionFlag flag)
        {
            Theraot.No.Op(flag);
        }

        public SecurityPermissionFlag Flags { get; set; }
        public override IPermission Copy() { return this; }
        public override void FromXml(SecurityElement securityElement) { }
        public override IPermission Intersect(IPermission target) { return default; }
        public override bool IsSubsetOf(IPermission target) { return false; }
        public bool IsUnrestricted() { return false; }
        public override SecurityElement ToXml() { return default; }
        public override IPermission Union(IPermission target) { return default; }
    }
}

#endif