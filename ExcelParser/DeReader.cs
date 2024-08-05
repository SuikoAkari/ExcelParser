using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelParser
{
    public class DeReader
    {
        public BinaryReader reader;
        private long position;

        public DeReader(string filename)
        {
            var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(fileStream);
            position = 0;
        }

        public long LenToEof()
        {
            return reader.BaseStream.Length - position;
        }

        public bool ReadBool()
        {
            var b = reader.ReadByte();
            position += 1;

            switch (b)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    throw new Exception($"Invalid boolean value: {b} pos: {position - 1}");
            }
        }

        public byte ReadU8()
        {
            var value = reader.ReadByte();
            position += 1;
            return value;
        }

        public sbyte ReadS8()
        {
            var value = reader.ReadSByte();
            position += 1;
            return value;
        }

        public ushort ReadU16()
        {
            var value = reader.ReadUInt16();
            position += 2;
            return value;
        }

        public short ReadS16()
        {
            var value = reader.ReadInt16();
            position += 2;
            return value;
        }

        public ushort ReadU16Be()
        {
            var a = reader.ReadByte();
            var b = reader.ReadByte();
            position += 2;
            return (ushort)((a << 8) | b);
        }

        public int ReadS32()
        {
            var value = reader.ReadInt32();
            position += 4;
            return value;
        }

        public uint ReadU32()
        {
            var value = reader.ReadUInt32();
            position += 4;
            return value;
        }

        public ulong ReadU64()
        {
            var value = reader.ReadUInt64();
            position += 8;
            return value;
        }

        public float ReadF32()
        {
            float value = reader.ReadSingle();
            position += 4;
            return value;
        }

        public double ReadF64()
        {
            var value = reader.ReadDouble();
            position += 8;
            return value;
        }

        public int ReadVarInt()
        {
            // From ZigZag-encoded data
            var encoded = ReadVarUInt();
            var sign = encoded & 1;
            var absValue = encoded >> 1;
            return sign == 0 ? (int)absValue : (int)~(absValue - 1);
        }

        public ulong ReadVarUInt()
        {
            ulong result = 0;
            int shift = 0;

            for (int i = 0; i < 20; i++)
            {
                byte byteValue = reader.ReadByte();
                position += 1;

                result |= (ulong)(byteValue & 0x7F) << shift;

                if ((byteValue & 0x80) != 0x80)
                {
                    return result;
                }

                shift += 7;
            }

            throw new Exception("Invalid VarUInt encoding.");
        }

        public string ReadString()
        {
            var len = (int)ReadVarUInt();
            var bytes = reader.ReadBytes(len);
            position += len;
            var str = Encoding.UTF8.GetString(bytes);
            return str.Replace(@"\\", @"\").Replace(@"\""", @"""");
        }

        public byte PeekByte()
        {
            var byteValue = reader.ReadByte();
            reader.BaseStream.Seek(-1, SeekOrigin.Current); // Move the position back
            return byteValue;
        }
    }
}
