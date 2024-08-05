using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelParser
{
    public class Reader
    {

        public static ulong ReadVarUInt(BinaryReader reader)
        {
            ulong result = 0;
            int shift = 0;

            while (true)
            {
                byte byteValue = reader.ReadByte();
                result |= (ulong)(byteValue & 0x7F) << shift;
                if ((byteValue & 0x80) == 0)
                    break;
                shift += 7;
            }

            return result;
        }

        public static long ReadVarInt(BinaryReader reader)
        {
            ulong encoded = ReadVarUInt(reader);
            long sign = (long)(encoded & 1);
            long absValue = (long)(encoded >> 1);
            return sign == 0 ? absValue : ~(absValue - 1);
        }
    }
}
