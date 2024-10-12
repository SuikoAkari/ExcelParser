
using ExcelParser;
using Pastel;

Console.WriteLine("Starting parsing...");

Parser parser = new Parser();

string[] files = Directory.GetFiles("Data");
foreach(string file in files)
{
/*if(file.Contains("scene3"))*/parser.Parse(file);
}

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