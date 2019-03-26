#if LESSTHAN_NET40 && !PROFILE328

namespace System.Threading.Tasks
{
    internal interface IContinuationTask
    {
        Task Antecedent { get; }
    }
}

#endif