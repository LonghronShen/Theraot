#if LESSTHAN_NET40 && !PROFILE328

namespace System.Collections
{
    public interface IStructuralComparable
    {
        int CompareTo(object other, IComparer comparer);
    }
}

#endif