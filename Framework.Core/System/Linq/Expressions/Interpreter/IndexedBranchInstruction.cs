﻿#if LESSTHAN_NET35

#pragma warning disable CC0031 // Check for null before calling a delegate

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Linq.Expressions.Interpreter
{
    internal abstract class IndexedBranchInstruction : Instruction
    {
        protected const int CacheSize = 32;
        internal readonly int LabelIndex;

        protected IndexedBranchInstruction(int labelIndex)
        {
            LabelIndex = labelIndex;
        }

        public RuntimeLabel GetLabel(InterpretedFrame frame)
        {
            Debug.Assert(LabelIndex != UnknownInstrIndex);
            return frame.Interpreter.Labels[LabelIndex];
        }

        public override string ToDebugString(int instructionIndex, object cookie, Func<int, int> labelIndexer, IList<object> objects)
        {
            Debug.Assert(LabelIndex != UnknownInstrIndex);
            var targetIndex = labelIndexer(LabelIndex);
            return ToString() + (targetIndex != BranchLabel.UnknownIndex ? " -> " + targetIndex : "");
        }

        public override string ToString()
        {
            Debug.Assert(LabelIndex != UnknownInstrIndex);
            return InstructionName + "[" + LabelIndex + "]";
        }
    }
}

#endif