﻿using System;
using System.Collections.Generic;
using System.Linq;
using Iced.Intel;
using MBBSEmu.Disassembler.Artifacts;
using MBBSEmu.Logging;
using NLog;

namespace MBBSEmu.Memory
{
    /// <summary>
    ///     Handles Memory Operations for the CPU Emulator
    ///
    ///     Memory is split into two regions:
    ///     1. Module Memory Space, which is used to host the module code/data segments
    ///     2. Host Memory Space, which is used to host memory/pointers generated by the MajorBBS/WG host software.
    ///
    ///     Functions within MajorBBS such as alczer(), alcblok(), etc. live within the host process memory regions and are accessed
    ///     by the imported functions. Pointers to these regions are passed back the module and saved in the module data segments.
    ///
    ///     Information of x86 Memory Segmentation: https://en.wikipedia.org/wiki/X86_memory_segmentation
    /// </summary>
    public class MemoryCore : IMemoryCore
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger(typeof(CustomLogger));
        public readonly Dictionary<ushort, InstructionList> _decodedSegments;
        public readonly Dictionary<ushort, byte[]> _memorySegments;
        public readonly Dictionary<ushort, Segment> _segments;

        private ushort _hostMemoryOffset = 0x0;

        public MemoryCore()
        {
            _decodedSegments = new Dictionary<ushort, InstructionList>();
            _memorySegments = new Dictionary<ushort, byte[]>();
            _segments = new Dictionary<ushort, Segment>();
        }

        /// <summary>
        ///     Declares a new 16-bit Segment and allocates it to the defined Segment Number
        /// </summary>
        /// <param name="segmentNumber"></param>
        public void AddSegment(ushort segmentNumber)
        {
            if(_memorySegments.ContainsKey(segmentNumber))
                throw new Exception($"Segment with number {segmentNumber} already defined");

            _memorySegments[segmentNumber] = new byte[0x10000];
        }

        public void AddSegment(Segment segment)
        {
            //Get Address for this Segment
            var segmentMemory = new byte[0x10000];
            
            //Add the data to memory and record the segment offset in memory
            Array.Copy(segment.Data, 0, segmentMemory, 0, segment.Data.Length);
            _memorySegments.Add(segment.Ordinal, segmentMemory);

            if (segment.Flags.Contains(EnumSegmentFlags.Code))
            {
                //Decode the Segment
                var instructionList = new InstructionList();
                var codeReader = new ByteArrayCodeReader(segment.Data);
                var decoder = Decoder.Create(16, codeReader);
                decoder.IP = 0x0;

                while (decoder.IP < (ulong)segment.Data.Length)
                {
                    decoder.Decode(out instructionList.AllocUninitializedElement());
                }

                _decodedSegments[segment.Ordinal] = instructionList;
            }

            _segments[segment.Ordinal] = segment;
        }

        public Segment GetSegment(ushort segmentNumber) => _segments[segmentNumber];

        public bool HasSegment(ushort segmentNumber) => _memorySegments.ContainsKey(segmentNumber);

        public Instruction GetInstruction(ushort segment, int instructionPointer)
        {
            return _decodedSegments[segment].First(x => x.IP16 == instructionPointer);
        }

        public byte GetByte(ushort segment, ushort offset)
        {
            return _memorySegments[segment][offset];
        }

        public ushort GetWord(ushort segment, ushort offset)
        {
            return BitConverter.ToUInt16(_memorySegments[segment], offset);
        }

        public byte[] GetArray(ushort segment, ushort offset, ushort count) => GetSpan(segment, offset, count).ToArray();

        public ReadOnlySpan<byte> GetSpan(ushort segment, ushort offset, ushort count)
        {
            Span<byte> segmentSpan = _memorySegments[segment];
            return segmentSpan.Slice(offset, count);
        }


        /// <summary>
        ///     Reads an array of bytes from the specified segment:offset, stopping
        ///     at the first null character denoting the end of the string.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public byte[] GetString(ushort segment, ushort offset)
        {
            Span<byte> segmentSpan = _memorySegments[segment];
            ushort byteCount = 0;
            for (ushort i = 0; i < ushort.MaxValue; i++)
            {
                byteCount = i;

                if (segmentSpan[offset + i] == 0)
                    break;
            }
            return segmentSpan.Slice(offset, byteCount).ToArray();
        }

        public void SetByte(ushort segment, ushort offset, byte value)
        {
            _memorySegments[segment][offset] = value;
        }

        public void SetWord(ushort segment, ushort offset, ushort value)
        {
            SetArray(segment, offset, BitConverter.GetBytes(value));
        }

        public void SetArray(ushort segment, ushort offset, byte[] array)
        {
            Array.Copy(array, 0, _memorySegments[segment], offset, array.Length);
        }

        public void SetArray(ushort segment, ushort offset, ReadOnlySpan<byte> array)
        {
            SetArray(segment, offset, array.ToArray());
        }

        public ushort AllocateHostMemory(ushort size)
        {
            var offset = _hostMemoryOffset;
            _hostMemoryOffset += size;

#if DEBUG
            _logger.Debug($"Allocated {size} bytes of memory in Host Memory Segment");
#endif
            return offset;
        }

        /// <summary>
        ///     Allocates a new segment at the first available segment in the 0xA000-0xAFFF address space
        /// </summary>
        /// <returns></returns>
        public ushort AllocateRoutineMemorySegment()
        {
            for (ushort i = 0x1000; i < 0x9FFF; i++)
            {
                if (_memorySegments.ContainsKey(i)) continue;

                _memorySegments[i] = new byte[0x10000];
                return i;
            }

            throw new OutOfMemoryException("Unable to Allocate Routine Memory - No available slots");
        }

        /// <summary>
        ///     Releases specified segment from Routine Memory segment space
        /// </summary>
        /// <param name="segment"></param>
        public void FreeRoutineMemorySegment(ushort segment)
        {
            if(segment < 0x1000 || segment > 0x9FFF)
                throw new Exception($"Attempt to free Routine Memory Segment outside of Routine Memory Segment Space: {segment:X4}");

            if (!_memorySegments.ContainsKey(segment))
                throw new Exception($"Attempt to free unknown Routine Memory Segment: {segment:X4}");

            _memorySegments.Remove(segment);
        }
    }
}
