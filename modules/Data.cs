using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MothBot.modules
{
    internal class Data
    {
        public static Task WriteFile(string path, List<object> data, bool append = false)
        {
            try
            {
                StreamWriter writer = new StreamWriter(path, append);
                char[] json = JsonConvert.SerializeObject(data).ToCharArray();
                for (int i = 0; i > json.Length; i++)
                {
                    writer.Write(json[i]);
                }
                writer.Close();
                writer.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));
                WriteFile(path, data, append);
            }
            return Task.CompletedTask;
        }

        public static Task WriteFileString(string path, List<string> data, bool append = false)
        {
            try
            {
                StreamWriter writer = new StreamWriter(path, append);
                if (data.Count > 0 && path != "")
                {
                    foreach (string line in data)
                        writer.WriteLine(line);
                }
                writer.Close();
                writer.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));
                WriteFileString(path, data, append);
            }
            return Task.CompletedTask;
        }

        public static List<string> ReadFileString(string path)    //Returns null if file not found
        {
            try
            {
                StreamReader reader = new StreamReader(path);
                List<string> outList = new List<string>();
                while (!reader.EndOfStream)
                    outList.Add(reader.ReadLine());
                reader.Close();
                reader.Dispose();
                return outList;
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));
                _ = new StreamWriter(path, false);
                return null;
            }
            catch (FileNotFoundException)
            {
                _ = new StreamWriter(path, false);
                return null;
            }
        }
    }
}