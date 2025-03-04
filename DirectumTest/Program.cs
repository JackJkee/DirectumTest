using DirectumTest.Models;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace DirectumTest
{
    internal class Program
    {
        const string folder_path = "C:\\Users\\JackJkee\\Desktop\\Directum Тестовое Задание";
        const string excelFilePath = $"{folder_path}\\DirectumTest.xlsx";

        static int[] LEVEL_SIFT = [9, 10, 17];

        

        static void Main(string[] args)
        {
            if (File.Exists(excelFilePath))
                File.Delete(excelFilePath);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var excelFile = new FileInfo(excelFilePath);
            using ExcelPackage package = new ExcelPackage(excelFile);

            List<AddressObjects.AObject> objects = new();
            List<ObjectLevels.ObjectLevel> levels = new();

            Console.WriteLine("______________ Загрузка объектов ______________");
            LoadObjects(ref objects);

            Console.WriteLine("______________ Загрузка уровней _______________");
            LoadLevels(ref levels);


            var sortedObjects = objects.OrderBy(o => o.UpdateDate).ToList();
            var listDates = sortedObjects.Select(o => o.UpdateDate).Distinct().ToList();
            

            foreach (var date in listDates)
            {
                Console.WriteLine($"Дата: {date}");

                string dateWithoutTime = date.Date.ToString("dd/MM/yyyy");
                int field = 1, row = 1;

                var ws = package.Workbook.Worksheets.Add(dateWithoutTime);
                

                var range = ws.Cells[1, 1].Value = $"Отчет по добавленным адресным объектам за {dateWithoutTime}";

                var listLevels = sortedObjects
                    .Where(o => o.UpdateDate == date)
                    .Select(o => o.Level)
                    .Distinct()
                    .ToList();

                foreach(var level in listLevels)
                {
                    var levelName = levels.Find(lvl => lvl.Level == level).Name;

                    var tmpObjects = sortedObjects
                        .Where(o => o.UpdateDate == date)
                        .Where(o => o.Level == level)
                        .Where(o => o.IsActive == true)
                        .OrderBy(o => o.Name)
                        .ToList();

                    if(tmpObjects.Count == 0) continue;

                    ws.Cells[++row, 1].Value = levelName;
                    ws.Cells[++row, 1].Value = "Тип объекта";
                    ws.Cells[row++, 2].Value = "Наименование";

                    foreach (var o in tmpObjects)
                    {
                        if (!o.IsActive || LEVEL_SIFT.Contains(o.Level))
                            continue;

                        ws.Cells[row, 1].Value = o.TypeName;
                        ws.Cells[row, 2].Value = o.Name;

                        row++;

                        Console.WriteLine($"{o.Name} | {o.UpdateDate} | Level: {levelName} ({level})");

                    }
                    Console.WriteLine();
                }
                ws.Column(1).AutoFit();
                ws.Column(2).AutoFit();
            }

            
            package.Save();

        }

        static void LoadLevels(ref List<ObjectLevels.ObjectLevel> levels)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObjectLevels));
            var files = Directory.GetFiles(folder_path, "*.xml");

            foreach (var file in files)
            {                
                string pattern = @"^AS_OBJECT_LEVELS_\d+_.*$";
                string filename = Path.GetFileName(file);

                if (Regex.IsMatch(filename, pattern))
                {
                    using Stream reader = new FileStream(file, FileMode.Open);
                    var obj = (ObjectLevels)serializer.Deserialize(reader);

                    if(obj == null)
                        continue;

                    foreach(var level in obj.objectLevels)
                    {
                        levels.Add(level);
                        //Console.WriteLine(level.Name);
                    }

                }
            }
        }

        static void LoadObjects(ref List<AddressObjects.AObject> listObjects)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Models.AddressObjects));
            var dirs = Directory.GetDirectories(folder_path);

            foreach (var dir in dirs)
            {
                var files = Directory.GetFiles(dir, "*.xml");

                foreach (var file in files)
                {
                    string pattern = @"^AS_ADDR_OBJ_\d+_.*$";
                    string filename = Path.GetFileName(file);

                    if (Regex.IsMatch(filename, pattern))
                    {
                        using Stream reader = new FileStream(file, FileMode.Open);
                        var obj = (AddressObjects)serializer.Deserialize(reader);

                        if (obj.aObjects == null)
                            continue;

                        foreach (var o in obj.aObjects)
                        {
                            listObjects.Add(o);
                        }
                        Console.WriteLine(file);
                    }
                }

            }
        }
    }
}
