
using ExcelParser;
using Newtonsoft.Json;
using Pastel;

Console.WriteLine("Starting parsing...");

Parser parser = new Parser();

string[] files = Directory.GetFiles("Data");
foreach(string file in files)
{
/*if(file.Contains("WeaponPromote"))*parser.Parse(file);
}


/*List<NewActivityExcelConfigData> newActivities = JsonConvert.DeserializeObject<List<NewActivityExcelConfigData>>(File.ReadAllText("Output/NewActivityExcelConfigData.json"));
List<ActivityConf> newConf = new List<ActivityConf>();

foreach(NewActivityExcelConfigData config in newActivities)
{
    newConf.Add(new ActivityConf()
    {
        activityId=config.activityId,
        activityType=0,
        scheduleId=0,
        meetCondList=new uint[0],
        beginTime= "2024-08-26T00:00:00+02:00",
        endTime= "2025-09-26T23:59:59+02:00"
    });
}
File.WriteAllText("ActivityConfig.json", JsonConvert.SerializeObject(newConf, Formatting.Indented));*/


/*string[] unknownFiles = Directory.GetFiles("D:\\Genshin5.1\\AssetMap\\Excels\\All\\MiHoYoBinData");
int tot = 0;
foreach(string file in unknownFiles)
{
    string txt = File.ReadAllText(file);
    Console.Write("\r{0}%".Pastel("#4287f5") + " completed   ", (float)tot / unknownFiles.Length * 100);
    if (txt.Contains("UI_AvatarIcon_Kate"))
    {
        Console.WriteLine("Found at: "+file);
        return;
    }
    tot++;
}*/