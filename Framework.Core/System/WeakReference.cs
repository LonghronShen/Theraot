#if LESSTHAN_NET45

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Theraot;

namespace System
{

#if PROFILE328
    [Serializable]
    public class WeakReference<T>
        : ISerializable where T : class
    {

        [NonSerialized]
        private readonly WeakReference inner;

        public bool IsAlive
        {
            get
            {
                return this.inner.IsAlive;
            }
        }

        public T Target
        {
            get
            {
                return (T)this.inner.Target;
            }
            set
            {
                this.inner.Target = value;
            }
        }

        public WeakReference(T target)
            : this(target, false)
        {
        }

        public WeakReference(T target, bool trackResurrection)
        {
            if (target == null) throw new ArgumentNullException("target");
            this.inner = new WeakReference(target, trackResurrection);
        }

        private WeakReference(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            No.Op(context);
            var target = (T)info.GetValue("TrackedObject", typeof(T));
            var trackResurrection = info.GetBoolean("TrackResurrection");

            this.inner = new WeakReference(target, trackResurrection);
        }

#if !PROFILE328
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
#endif
        public bool TryGetTarget(out T target)
        {
            target = default;
            //if (!_handle.IsAllocated)
            //{
            //    return false;
            //}

            try
            {
                var obj = this.inner.Target;
                if (obj == null)
                {
                    return false;
                }

                target = obj as T;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

#if !PROFILE328
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
#endif
        public void SetTarget(T value)
        {
            this.Target = value;
        }

#if !PROFILE328
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
#endif
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            TryGetTarget(out var value);
            info.AddValue("TrackedObject", value, typeof(T));
            info.AddValue("TrackResurrection", this.inner.TrackResurrection);
        }

    }
#else
        [Serializable]
    public sealed class WeakReference<T> : ISerializable
        where T : class
    {
        private readonly bool _trackResurrection;

        [NonSerialized]
        private GCHandle _handle;

        public WeakReference(T target)
            : this(target, false)
        {
            // Empty
        }

        public WeakReference(T target, bool trackResurrection)
        {
            _trackResurrection = trackResurrection;
            SetTarget(target);
        }

        private WeakReference(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            No.Op(context);
            var value = (T)info.GetValue("TrackedObject", typeof(T));
            _trackResurrection = info.GetBoolean("TrackResurrection");
            SetTarget(value);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            TryGetTarget(out var value);
            info.AddValue("TrackedObject", value, typeof(T));
            info.AddValue("TrackResurrection", _trackResurrection);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void SetTarget(T value)
        {
            var oldHandle = _handle;
            _handle = GetNewHandle(value, _trackResurrection);
            if (!oldHandle.IsAllocated)
            {
                return;
            }

            oldHandle.Free();
            try
            {
                oldHandle.Free();
            }
            catch (InvalidOperationException exception)
            {
                // The handle was freed or never initialized.
                // Nothing to do.
                No.Op(exception);
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public bool TryGetTarget(out T target)
        {
            target = default;
            if (!_handle.IsAllocated)
            {
                return false;
            }

            try
            {
                var obj = _handle.Target;
                if (obj == null)
                {
                    return false;
                }

                target = obj as T;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        private static GCHandle GetNewHandle(T value, bool trackResurrection)
        {
            return GCHandle.Alloc(value, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
        }
    }
#endif

}

#endif