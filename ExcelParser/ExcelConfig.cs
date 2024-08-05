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
            {"interactionTitleTextMapHash","uint"},
            {"materialType","MaterialType" },
            {"cdTime","uint" },
            {"cdGroup","uint" },
            {"stackLimit","uint" },
            {"maxUseCount","uint" },
            {"useOnGain","bool" },
            {"noFirstGetHint","bool" },
            {"playGainEffect","bool" },
            {"useTarget","bool" },
            {"itemUse","ItemUseConfig[]" },
            {"rankLevel","uint" },
            {"foodQuality","FoodQualityType" },
            {"effectDescTextMapHash","uint" },
            {"specialDescTextMapHash","uint" },
            {"typeDescTextMapHash","uint" },
            {"effectIcon","string" },
        };
        private string v;

        public ExcelConfig(string input)
        {
            try
            {
                string[] lines = File.ReadAllLines(input);
                exist = true;
                Properties.Clear();
                Enums.Clear();
                foreach (string line in lines)
                {
                    string[] keyValue = line.Split(':');
                    string val = keyValue[1].Trim();
                    if (keyValue[0].Trim() == "type")
                    {
                        type = val;
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
