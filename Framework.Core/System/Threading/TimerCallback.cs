#if LESSTHAN_NETSTANDARD12 && !PROFILE328
namespace System.Threading
{
    [Runtime.InteropServices.ComVisible(true)]
    public delegate void TimerCallback(object state);
}

#endif