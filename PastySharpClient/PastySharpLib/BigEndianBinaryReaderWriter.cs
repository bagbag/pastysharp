using System;
using System.IO;
using System.Text;

namespace PastySharp
{
    public class BigEndianReader : BinaryReader
    {
        private byte[] _a16 = new byte[2];
        private byte[] _a32 = new byte[4];
        private byte[] _a64 = new byte[8];

        public BigEndianReader(Stream stream) : base(stream)
        {
        }

        public BigEndianReader(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public BigEndianReader(Stream stream, Encoding encoding, bool leaveOpen) : base(stream, encoding, leaveOpen)
        {
        }

        public override Int16 ReadInt16()
        {
            _a16 = base.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_a16);
            return BitConverter.ToInt16(_a16, 0);
        }

        public override int ReadInt32()
        {
            _a32 = base.ReadBytes(4);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a32);
            return BitConverter.ToInt32(_a32, 0);
        }

        public override Int64 ReadInt64()
        {
            _a64 = base.ReadBytes(8);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a64);
            return BitConverter.ToInt64(_a64, 0);
        }

        public override UInt16 ReadUInt16()
        {
            _a16 = base.ReadBytes(2);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a16);
            return BitConverter.ToUInt16(_a16, 0);
        }

        public override UInt32 ReadUInt32()
        {
            _a32 = base.ReadBytes(4);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a32);
            return BitConverter.ToUInt32(_a32, 0);
        }

        public override Single ReadSingle()
        {
            _a32 = base.ReadBytes(4);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a32);
            return BitConverter.ToSingle(_a32, 0);
        }

        public override UInt64 ReadUInt64()
        {
            _a64 = base.ReadBytes(8);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a64);
            return BitConverter.ToUInt64(_a64, 0);
        }

        public override Double ReadDouble()
        {
            _a64 = base.ReadBytes(8);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a64);
            return BitConverter.ToUInt64(_a64, 0);
        }

        public string ReadStringToNull()
        {
            string result = "";
            char c;
            for (int i = 0; i < base.BaseStream.Length; i++)
            {
                if ((c = (char) base.ReadByte()) == 0)
                {
                    break;
                }
                result += c.ToString();
            }
            return result;
        }
    }

    public class BigEndianWriter : BinaryWriter
    {
        private byte[] _a16 = new byte[2];
        private byte[] _a32 = new byte[4];
        private byte[] _a64 = new byte[8];

        public BigEndianWriter(Stream stream) : base(stream)
        {
        }

        public BigEndianWriter(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public BigEndianWriter(Stream stream, Encoding encoding, bool leaveOpen) : base(stream, encoding, leaveOpen)
        {
        }

        public override void Write(Int16 value)
        {
            _a16 = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a16);
            base.Write(_a16);
        }

        public override void Write(Int32 value)
        {
            _a32 = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a32);
            base.Write(_a32);
        }

        public override void Write(Int64 value)
        {
            _a64 = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a64);
            base.Write(_a64);
        }

        public override void Write(UInt16 value)
        {
            _a16 = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a16);
            base.Write(_a16);
        }

        public override void Write(UInt32 value)
        {
            _a32 = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a32);
            base.Write(_a32);
        }

        public override void Write(Single value)
        {
            _a32 = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a32);
            base.Write(_a32);
        }

        public override void Write(UInt64 value)
        {
            _a64 = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a64);
            base.Write(_a64);
        }

        public override void Write(Double value)
        {
            _a64 = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(_a64);
            base.Write(_a64);
        }
    }
}
