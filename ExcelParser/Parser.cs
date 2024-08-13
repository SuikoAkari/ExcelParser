using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Pastel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;


namespace ExcelParser
{
    public class BitMask
    {
        protected uint _len;
        protected ulong _mask;
        protected uint[] _excelMask;

        public bool TestBit(int index)
        {
            if (index >= (int)_len * 8) return false;

            if (_excelMask != null)
            {
                return (_excelMask[index >> 5] & (1 << (index & 0x1F))) != 0;
            }
            else
            {
                return (_mask & (1UL << index)) != 0;
            }
        }
    }

    public class ExcelBitMask : BitMask
    {
        public ExcelBitMask(DeReader reader)
        {
            _len = (uint)reader.ReadVarUInt();
            int lenInBytes = (int)_len / 4 + 1;
            _excelMask = new uint[lenInBytes];

            for (int i = 0; i < _len / 4; i++)
            {
                _excelMask[i] = reader.ReadU32();
            }

            // Read leftovers
            for (int i = 0; i < _len % 4; i++)
            {
                if (_excelMask[_len / 4] == 0)
                {
                    _excelMask[_len / 4] = 0;
                }
                _excelMask[_len / 4] |= (uint)reader.ReadU8() << (i * 8);
            }
        }
    }
    public class Parser
    {
        public string ParseClass(DeReader reader, ExcelConfig excel)
        {
          //  uint classId = (uint)reader.ReadVarUInt();

            return ParseClassInt(reader, excel);
        }
        public string ParseClassInt(DeReader reader, ExcelConfig excel)
        {
            List<string> strings = new List<string>();

            ExcelBitMask mask = new ExcelBitMask(reader);

            int j = 0;
            foreach (KeyValuePair<string, string> keyValuePair in excel.Properties)
            {
                if (mask.TestBit(j))
                {

                  
                    string val = ParseType(keyValuePair.Value.Trim(), reader);
                    Console.WriteLine("Adding " + keyValuePair.Key+": "+val);
                    if (val != null)
                    strings.Add($"\"{keyValuePair.Key}\": {(val.Length < 500 ? val : 0) }"); 
                }
                else
                {
                   Console.WriteLine("Skip "+keyValuePair.Key);
                }

               
                j++;
            }

            return "{"+ string.Join(",", strings.ToArray())+"}";
            
        }

        private string ParseType(string Value, DeReader reader)
        {
            if (Value.EndsWith("[]"))
            {
                string fieldType = Value.Substring(0, Value.Length - 2);
                uint length = (uint)reader.ReadVarUInt();
                List<string> strings = new List<string>();
                for(int i=0; i < length; i++)
                {
                    strings.Add(ParseType(fieldType, reader));
                }
                return $"[{string.Join(",", strings.ToArray())}]";
            }
            else if (Value == "uint")
            {
               ulong val = reader.ReadVarUInt();
                return ""+val;
            }
            else if (Value == "float")
            {
                float val = reader.ReadF32();
                return $"{val}".Replace(",",".");
            }
            else if (Value == "double")
            {
                double val = reader.ReadF64();
                return val.ToString().Replace(",", ".");
            }
            else if (Value == "byte")
            {
                byte val = reader.ReadU8();
                return val.ToString();
            }
            else if (Value == "int")
            {
                int val = (int)reader.ReadVarInt();
                return val.ToString();
            }
            else if (Value == "bool")
            {
                bool val = reader.ReadBool();
                return val.ToString().ToLower();
            }
            else if (Value == "string")
            {
                string val = reader.ReadString();
                return $"\"{(val.Length < 500 ? val : 0)}\"";
            }
            else
            {
                ExcelConfig config = new ExcelConfig("Configs/"+Value+".txt");
                if (!config.exist) return null;
                if (config.type == "Class")
                {
                    return ParseClass(reader, config);
                }else if(config.type == "Enum")
                {
                    return ParseEnum(reader, config);
                }
                else
                {
                    return null;
                }
                
            }
        }

        private string ParseEnum(DeReader reader, ExcelConfig config)
        {
           uint val =(uint) reader.ReadVarInt() ;
           string sVal=config.GetEnumValue(val);

           return $"\"{sVal}\"";
        }

        public void Parse(string input) {
            string fileName = Path.GetFileName(input);
            Console.WriteLine("Parsing " + fileName.Pastel("#4287f5"));
            try
            {
                DeReader deReader = new DeReader(input);
           

                string ExcelName = fileName.Replace("Data", "");

                int arraySize = (int)deReader.ReadVarInt();

                // Console.WriteLine("arraySize " + arraySize);
                List<string> strings = new List<string>();
                for (int i = 0; i < arraySize; i++)
                {
                    Console.Write("\r{0}%".Pastel("#4287f5")+" completed   ", (float)i/arraySize*100);
                    
                    strings.Add(ParseClassInt(deReader, new ExcelConfig("Configs/" + ExcelName + ".txt")));
                }
                Console.WriteLine("\rParsing of " + fileName.Pastel("#4287f5") + ": "+"SUCCESS".Pastel("#51f542")+" OUTPUT: "+ $"Output/{fileName}.json".Pastel("#ffdf78"));
               // File.WriteAllText($"Output/{fileName}.json.noIdent", "[" + string.Join(",", strings.ToArray()) + "]");
                dynamic parsedJson = JsonConvert.DeserializeObject("[" + string.Join(",", strings.ToArray()) + "]");
                string allText = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                File.WriteAllText($"Output/{fileName}.json", allText);
              
                
            }
            catch(Exception e)
            {
                
                Console.WriteLine("\rParsing of " + fileName.Pastel("#4287f5") + ": "+"ERROR OCCURRED".Pastel("#f54242"));
                Console.WriteLine("Error: "+e.Message.Pastel("#f54242"));
            }
           

        } 
    }
}
