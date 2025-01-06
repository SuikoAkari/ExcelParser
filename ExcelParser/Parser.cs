using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Pastel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
    public class BinOutBitMask : BitMask
    {
      

        public BinOutBitMask(DeReader reader, bool small)
        {
            // Call the base class constructor (if any)
            // base();

            _mask = (ulong)(small ? new BigInteger(reader.ReadU8()) : reader.ReadVarUInt());
            _len = ((uint)new BigInteger(8));
        }
    }
    public class Parser
    {
        public string ParseClass(DeReader reader, ExcelConfig excel, bool needToBeBin = false)
        {
          //  uint classId = (uint)reader.ReadVarUInt();

            return ParseClassInt(reader, excel, needToBeBin);
        }
        public string ParseClassInt(DeReader reader, ExcelConfig excel, bool needToBeBin=false)
        {
            List<string> strings = new List<string>();
            bool isBin = excel.type == "Bin";
            if (needToBeBin) isBin = true;
            BitMask mask = isBin ? new BinOutBitMask(reader, excel.Properties.Count <= 8) : new ExcelBitMask(reader);
            int typeIndex = 0;

            if(excel.TypeIndexes.Count > 0)
            {
                typeIndex = (int)reader.ReadVarUInt();
                excel = excel.GetExcelFromTypeIndex(typeIndex);
                strings.Add($"\"$type\": \"{excel.Name}\"");
                // Console.WriteLine("$type".PastelBg(ConsoleColor.Green) + ": \"" + excel.Name+"\"");

            }
            int j = 0;
            foreach (KeyValuePair<string, string> keyValuePair in excel.Properties)
            {
                try
                {
                    if (mask.TestBit(j))
                    {


                        string val = ParseType(keyValuePair.Value.Trim(), reader, isBin);
                        //  Console.WriteLine(keyValuePair.Key.PastelBg(ConsoleColor.Green)+": "+val);
                        if (val != null)
                            strings.Add($"\"{keyValuePair.Key}\": {val}");
                    }
                    else
                    {
                        //Console.WriteLine(keyValuePair.Key.PastelBg(ConsoleColor.Red));

                    }
                }
                catch(Exception e)
                {
                    //Console.WriteLine(keyValuePair.Key.PastelBg(ConsoleColor.Red));
                }



                j++;
            }

            return "{"+ string.Join(",", strings.ToArray())+"}";
            
        }
        public string ParseDictionary(string fieldType, DeReader reader, string excelName)
        {
            List<string> strings = new List<string>();

            string field = ParseType(fieldType.Trim(), reader);
            // Console.WriteLine("Map key field: " + field);
            return $"{field}: " + ParseType(excelName,reader);
        }
        private string ParseType(string Value, DeReader reader, bool isBin=false)
        {
            if (Value.StartsWith("dictionary<"))
            {
                Value = Value.Replace("dictionary<", "").Replace(">", "");

                string fieldType = Value.Split(",")[0];
               
                uint length = (uint)reader.ReadVarUInt();
                Console.WriteLine("Dictionary Size: " + length);
                List<string> strings = new List<string>();
                ExcelConfig config = new ExcelConfig("Configs/" + Value.Split(",")[1] + ".txt");
                for (int i = 0; i < length; i++)
                {
                    strings.Add(ParseDictionary(fieldType, reader, Value.Split(",")[1]));
                }
                return "{"+$"{string.Join(",", strings.ToArray())}"+"}";
            }
            else if (Value.EndsWith("[]"))
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
                    return ParseClass(reader, config,isBin);
                }else if(config.type == "Enum")
                {
                    return ParseEnum(reader, config);
                }
                else if (config.type == "Bin")
                {
                    return ParseClass(reader, config, isBin);
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
            List<string> strings = new List<string>();
            try
            {
                DeReader deReader = new DeReader(input);
           

                string ExcelName = fileName.Replace("Data", "");
                if (ExcelName.Contains("ConfigTalent_"))
                {
                    ExcelName = "ConfigTalent";
                }else if (ExcelName.Contains("scene3"))
                {
                    ExcelName = "ScenePointList";
                }else if (ExcelName.Contains("AbilityGroup_Other_PlayerElementAbility"))
                {
                    ExcelName = "AbilityGroup";
                }
                ExcelConfig config = new ExcelConfig("Configs/" + ExcelName + ".txt");
                int arraySize = 0;
                if (config.type == "DicBin")
                {
                  
                    Console.WriteLine("DicSize " + arraySize);
                    string dic = ParseType(config.Properties["start"], deReader, true);
                    // Console.WriteLine(dic);
                    dynamic parsedJson = JsonConvert.DeserializeObject(dic);
                    string allText = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                    
                    File.WriteAllText($"Output/{fileName}.json", allText);
                    return;
                }
                else if (config.type != "Bin")
                {
                    arraySize = (int)deReader.ReadVarInt();

                }
                //  Console.WriteLine("arraySize " + arraySize);
                
                string singleClassString = "";
                if(arraySize > 0)
                {
                    for (int i = 0; i < arraySize; i++)
                    {
                        Console.Write("\r{0}%".Pastel("#4287f5") + " completed   ", (float)i / arraySize * 100);

                        strings.Add(ParseClassInt(deReader, config));
                    }
                    Console.WriteLine("\rParsing of " + fileName.Pastel("#4287f5") + ": " + "SUCCESS".Pastel("#51f542") + " OUTPUT: " + $"Output/{fileName}.json".Pastel("#ffdf78"));
                    // File.WriteAllText($"Output/{fileName}.json.noIdent", "[" + string.Join(",", strings.ToArray()) + "]");
                    dynamic parsedJson = JsonConvert.DeserializeObject("[" + string.Join(",", strings.ToArray()) + "]");
                    string allText = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                    File.WriteAllText($"Output/{fileName}.json", allText);
                }
                else
                {
                    singleClassString = ParseClassInt(deReader, config);
                    Console.WriteLine("\rParsing of " + fileName.Pastel("#4287f5") + ": " + "SUCCESS".Pastel("#51f542") + " OUTPUT: " + $"Output/{fileName}.json".Pastel("#ffdf78"));
                    // File.WriteAllText($"Output/{fileName}.json.noIdent", "[" + string.Join(",", strings.ToArray()) + "]");
                    dynamic parsedJson = JsonConvert.DeserializeObject(singleClassString);
                    string allText = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                    File.WriteAllText($"Output/{fileName}.json", allText);
                }
               
               
              
                
            }
            catch(Exception e)
            {
                //File.WriteAllLines($"Output/{fileName}.json", strings);
                Console.WriteLine("\rParsing of " + fileName.Pastel("#4287f5") + ": "+"ERROR OCCURRED".Pastel("#f54242"));
                Console.WriteLine("Error: "+e.Message.Pastel("#f54242"));
            }
           

        } 
    }
}
