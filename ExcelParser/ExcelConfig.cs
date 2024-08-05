using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelParser
{
    public class ExcelConfig
    {
        public bool exist;
        public string type = "Class";
        public Dictionary<string, int> Enums = new Dictionary<string, int>();
        public Dictionary<string, string> Properties = new Dictionary<string, string>()
        {

        };
       

        public ExcelConfig(string input)
        {
            try
            {
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
                    if (type == "Class")
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
    }
}
