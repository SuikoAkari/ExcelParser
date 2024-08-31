
using ExcelParser;

Console.WriteLine("Starting parsing...");

Parser parser = new Parser();

string[] files = Directory.GetFiles("Data");
foreach(string file in files)
{
/*if(file.Contains("AvatarTalentExcel"))*/parser.Parse(file);
}
