using DirectumTest.Models;
using OfficeOpenXml;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Xml.Serialization;

namespace DirectumTest
{
    internal class Program
    {
        static readonly string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        const string filesFolderName = "Directum Files";
        static readonly string filesPath = $"{currentDir}\\{filesFolderName}";
        const string GetLastDownloadFileInfoURL = "http://fias.nalog.ru/WebServices/Public/GetLastDownloadFileInfo";
        static readonly string excelFilePath = $"{currentDir}\\DirectumTest.xlsx";

        static List<AddressObjects.AObject> objects = new();
        static List<ObjectLevels.ObjectLevel> levels = new();

        static int[] LEVEL_SIFT = [9, 10, 17];

        static async Task Main(string[] args)
        {
            #region Инициализация
            Initialize();
            ClearDirectory(filesPath);

            DownloadFileInfo? dwi = await GetDownloadFileInfoAsync(GetLastDownloadFileInfoURL);

            if (dwi == null)
            {
                Console.WriteLine("Ошибка при загрузке информации DownloadFileInfo");
                return;
            }
            #endregion

            #region скачивание архива

            Console.WriteLine("Скачивание архива...");

            Uri uri = new Uri(dwi.GarXMLDeltaUrl);
            string zipName = Path.GetFileName(uri.LocalPath);

            if (! await DownloadFile(dwi.GarXMLDeltaUrl, $"{filesPath}\\{zipName}"))
                return;

            Console.WriteLine("Скачивание архива завершено.");
            #endregion

            #region Распаковка архива
            Console.WriteLine("Расспаковка архива...");
            System.IO.Compression.ZipFile.ExtractToDirectory($"{filesPath}\\{zipName}", filesPath);
            Console.WriteLine("Расспаковка архива завершена.");
            #endregion

            #region Загрузка объектов и уровней
            Console.WriteLine("Загрузка объектов...");
            LoadObjects(ref objects);

            Console.WriteLine("Загрузка уровней...");
            LoadLevels(ref levels);
            #endregion

            #region Генерация отчета
            Console.WriteLine("Генерация отчета...");
            GenerateReport(ref objects, ref levels, dwi);
            Console.WriteLine("Генерация отчета завершена.");
            #endregion
        }

        static void GenerateReport(ref List<AddressObjects.AObject> objects, ref List<ObjectLevels.ObjectLevel> levels, DownloadFileInfo dwi)
        {
            if (File.Exists(excelFilePath))
                File.Delete(excelFilePath);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var excelFile = new FileInfo(excelFilePath);
            using ExcelPackage package = new ExcelPackage(excelFile);

            int row = 1;

            var ws = package.Workbook.Worksheets.Add(dwi.Date);
            ws.Cells[1, 1].Value = $"Отчет по добавленным адресным объектам за {dwi.Date}";

            var listLevels = objects
                .Select(o => o.Level)
                .Distinct()
                .ToList();

            foreach (var level in listLevels)
            {
                var levelName = levels.Find(lvl => lvl.Level == level).Name;

                var tmpObjects = objects
                    .Where(o => o.Level == level)
                    .Where(o => o.IsActive == true)
                    .OrderBy(o => o.Name)
                    .ToList();

                if (tmpObjects.Count == 0) continue;

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

                }
            }
            ws.Column(1).AutoFit();
            ws.Column(2).AutoFit();
            package.Save();
        }

        static void LoadLevels(ref List<ObjectLevels.ObjectLevel> levels)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObjectLevels));
            var files = Directory.GetFiles(filesPath, "*.xml");

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
                    }

                }
            }
        }

        static void LoadObjects(ref List<AddressObjects.AObject> listObjects)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Models.AddressObjects));
            var dirs = Directory.GetDirectories(filesPath);

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

        public static void ClearDirectory(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            Array.ForEach(Directory.GetFiles(path), File.Delete);

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    file.Delete();
                }
                dir.Delete(true);
            }
        }

        public static void Initialize()
        {
            if (!Directory.Exists(filesPath))
                Directory.CreateDirectory(filesPath);
        }

        public static async Task<string?> GetJsonFromUrlAsync(string url)
        {
            try
            {
                using HttpClient client = new HttpClient();
                var response = await client.GetAsync(url);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception($"Ошибка соединения с сервисом. Status Code: {response.StatusCode}");

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static async Task<DownloadFileInfo?> GetDownloadFileInfoAsync(string url)
        {
            string? json = await GetJsonFromUrlAsync(url);

            if (json == null) return null;

            return JsonSerializer.Deserialize<DownloadFileInfo>(json);
        }

        public static async Task<bool> DownloadFile(string url, string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);

                using HttpClient client = new();
                using Stream stream = await client.GetStreamAsync(url);
                using FileStream fs = new(path, FileMode.Create);
                await stream.CopyToAsync(fs);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
