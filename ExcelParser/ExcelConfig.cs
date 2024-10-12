using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelParser
{
    public class ExcelConfig
    {
        public string Name;
        public bool exist;
        public string type = "Class";
        public Dictionary<string, int> Enums = new Dictionary<string, int>();
        public Dictionary<string, string> Properties = new Dictionary<string, string>()
        {

        };
        public Dictionary<string, string> TypeIndexes = new Dictionary<string, string>()
        {

        };

        public ExcelConfig(string input)
        {
            try
            {
                if(File.Exists(input+ ".TypeIndex"))
                {
                   
                    string[] lines_ = File.ReadAllLines(input + ".TypeIndex");
                    int ii = 0;
                    foreach (string line in lines_)
                    {
                        if (line.Contains("//"))
                        {
                            continue;
                        }
                        string[] keyValue = line.Split(':');
                        string val = keyValue[1].Trim();
                       
                        TypeIndexes.Add(keyValue[0].Trim(), val);
                        
                        ii++;
                    }
                }
                Name = input.Replace("Configs/", "").Replace(".txt", "");
                string[] lines = File.ReadAllLines(input);
                exist = true;
                Properties.Clear();
                Enums.Clear();
                int i = 0;
                foreach (string line in lines)
                {
                    if (line.Contains("//")){
                        continue;
                    }
                    string[] keyValue = line.Split(':');
                    string val = keyValue[1].Trim();
                    if (keyValue[0].Trim() == "type" && i==0)
                    {
                        type = val;
                        i++;
                        continue;
                    }
                    else
                    {

                    }
                    if (type == "Class" || type=="Bin")
                    {
                        Properties.Add(keyValue[0].Trim(),val);
                    }
                    if (type == "Enum")
                    {
                        Enums.Add(keyValue[0].Trim(), int.Parse(val));
                    }
                    i++;
                }
            }
            catch (Exception e)
            {
                exist = false;
                Properties.Clear();
                Enums.Clear();
            }
           
        }

        public string GetEnumValue(uint val)
        {
           foreach(KeyValuePair<string, int> pair in Enums)
           {
                if (pair.Value == (int)val)
                {
                    return pair.Key;
                }
           }
            return "UNKNOWN_" + val;
        }

        internal ExcelConfig GetExcelFromTypeIndex(int typeIndex)
        {
            string key = "" + typeIndex;
            Console.WriteLine(key);
            if (TypeIndexes.ContainsKey(key))
            {
                return new ExcelConfig("Configs/" + TypeIndexes[key] + ".txt");
            }
            else
            {
                return this;
            }
        }
    }
}
