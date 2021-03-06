﻿#if FAT
using System;

namespace Theraot.Core
{
    public static class AggregateExceptionHelper
    {
        public static void AddException(ref AggregateException target, Exception source)
        {
            target = target == null ? new AggregateException(source) : new AggregateException(source, target).Flatten();
        }

        public static void AddException(ref Exception target, Exception source)
        {
            target = target == null ? new AggregateException(source) : new AggregateException(source, target).Flatten();
        }
    }
}

#endif