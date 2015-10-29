using System;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace DiskRevisor
{
    class Program
    {
        public static bool changed = false;

        static void Main(string[] args)
        {
            if (args.Length < 2)
                Console.WriteLine("DiskRevisor v 0.1\nИспользование\ndiskrevisor.exe -t:<dump|check> -p:<Путь>");
            else
            {
                string operation = args[0].Replace("-t:", "").ToLower();
                string path = args[1].Replace("-p:", "");
                DDirectory dump;
                if (operation.Equals("dump"))
                {
                    File.Delete("test.bin");
                    Console.WriteLine("Сбор дампа...");
                    dump = createContext(path, 0);
                    Console.WriteLine("Дамп успешно собран, сериализация...");
                    serialize(dump);
                    Console.WriteLine("Сериализация успешна");
                }
                else if (operation.Equals("check"))
                {
                    Console.WriteLine("Десериализация...");
                    dump = (DDirectory)deserialize();
                    Console.WriteLine("Дамп успешно восстановлен");
                    check(path, dump);
                    if (changed)
                        Console.WriteLine("Проверка завершена. В ходе проверки выявлены изменения");
                    else
                        Console.WriteLine("Проверка завершена. В ходе проверки изменения не выявлены");
                }
                else
                    Console.WriteLine("Неверно задан тип операции");
            }

        }

        static bool check(string path, DDirectory dir)
        {
            string[] splpath = path.Split('\\');
            string tmppth = "";
            for (int i = 1; i < splpath.Length; i++)
            {
                tmppth = splpath[0];
                for (int j = 1; j <= i; j++)
                    tmppth += '\\' + splpath[j];
                dir = checkAvailabilty(tmppth, dir);
                if (dir == null)
                {
                    Console.WriteLine("Папка {0} не была проверена либо была добавлена после дампа", tmppth);
                    return false;
                }
            }
            Console.WriteLine("Проверка...");
            compare(dir, path);

            return true;

        }

        public static void compare(DDirectory dump, string path)
        {
            string[] subdirs;
            string[] files;

            try
            {
                subdirs = Directory.GetDirectories(path);
                files = Directory.GetFiles(path);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Доступ запрещен к папке {0}", path);
                return;
            }
            if (!dump.isDumped)
                Console.WriteLine("Внимание! Не полностью проверена папка\n{0}\n" +
                    "Некоторые объекты могли быть проигнорированы при сборе дампа\n", dump.name);
            FileInfo fi;
            bool match;
            for (int i = 0; i < files.Length; i++)
            {
                match = false;
                for (int j = 0; j < dump.files.Count; j++)
                    if (files[i].Equals(dump.files[j].name))
                    {

                        fi = new FileInfo(files[i]);
                        if (fi.Length != dump.files[j].size)
                        {
                            Console.WriteLine("Размер следующего файла был изменен:\n{0}\n", files[i]);
                            changed = true;
                        }
                        byte[] hashBytes = computeFileHash(files[i]);
                        if (hashBytes == null)
                        {
                            match = true;
                            dump.files[j].isChecked = true;
                            break;
                        }
                        if (!BitConverter.ToString(hashBytes).Equals(dump.files[j].hash))
                        {
                            Console.WriteLine("Хеш-сумма следующего файла изменилась:\n{0}\n", files[i]);
                            changed = true;
                        }
                        dump.files[j].isChecked = true;
                        match = true;
                        break;
                    }
                if (!match)
                {
                    if (dump.isDumped)
                        Console.WriteLine("Следующий файл был добавлен:\n{0}\n", files[i]);
                    changed = true;
                }
            }

            for (int i = 0; i < dump.files.Count; i++)
                if (!dump.files[i].isChecked)
                {
                    changed = true;
                    Console.WriteLine("Следующий файл был удален:\n{0}\n", dump.files[i].name);
                }

            for (int i = 0; i < subdirs.Length; i++)
            {
                match = false;
                for (int j = 0; j < dump.subdirs.Count; j++)
                    if (subdirs[i].Equals(dump.subdirs[j].name))
                    {
                        compare(dump.subdirs[j], subdirs[i]);
                        dump.subdirs[j].isChecked = true;
                        match = true;
                        break;
                    }
                if (!match)
                {
                    changed = true;
                    if (dump.isDumped)
                        Console.WriteLine("Следующая папка была добавлена:\n{0}\n", subdirs[i]);
                    else
                        Console.WriteLine("Следующая папка была добавлена либо не была внесена в дамп:\n{0}\n", subdirs[i]);
                }
            }

            for (int i = 0; i < dump.subdirs.Count; i++)
                if (!dump.subdirs[i].isChecked)
                {
                    changed = true;
                    Console.WriteLine("Следующая папка была удалена:\n{0}\n", dump.subdirs[i].name);
                }
        }

        static DDirectory checkAvailabilty(string path, DDirectory dir)
        {
            for (int i = 0; i < dir.subdirs.Count; i++)
                if (dir.subdirs[i].name.Equals(path))
                    return dir.subdirs[i];
            return null;
        }

        public static void print(DDirectory dir)
        {
            Console.WriteLine("DirName: {0} Dumped:{1}", dir.name, dir.isDumped);
            List<DFile> files = dir.files;
            List<DDirectory> subdirs = dir.subdirs;
            for (int i = 0; i < files.Count; i++)
                Console.WriteLine("FileName: {0} Dumped: {1}", files[i].name, files[i].isDumped);
            for (int i = 0; i < subdirs.Count; i++)
                print(subdirs[i]);
        }

        public static DDirectory createContext(string path, int iteration)
        {
            string[] folders = path.Split('\\');
            string sub = "";
            DDirectory dir;
            if (iteration < folders.Length - 1)
            {
                for (int i = 0; i < iteration; i++)
                    sub += folders[i] + '\\';
                sub += folders[iteration];
                dir = new DDirectory(sub, false);
                dir.subdirs.Add(createContext(path, ++iteration));
            }
            else
            {
                dir = createDB(path);
                dir.isDumped = true;
            }
            return dir;
        }

        public static DDirectory createDB(string path)
        {
            IEnumerable<string> subdirs, files;
            try
            {
                subdirs = Directory.EnumerateDirectories(path);
                files = Directory.EnumerateFiles(path);

            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                return new DDirectory(path, false);
            }
            List<DDirectory> dsd = new List<DDirectory>();
            List<DFile> df = new List<DFile>();
            foreach (string subdir in subdirs)
                dsd.Add(createDB(subdir));
            foreach (string file in files)
                df.Add(dump(file));
            return new DDirectory(path, df, dsd, true);
        }

        public static DFile dump(string path)
        {
            FileInfo temp = new FileInfo(path);
            long size = temp.Length;
            byte[] hashBytes = computeFileHash(path);
            if (hashBytes == null)
                return new DFile(size, path, null, false);
            string hash = BitConverter.ToString(hashBytes);
            DFile result = new DFile(size, path, hash, true);
            return result;
        }

        private static byte[] computeFileHash(string filename)
        {
            FileStream fs;
            try
            {
                fs = new FileStream(filename, FileMode.Open, System.Security.AccessControl.FileSystemRights.ReadData, FileShare.Read, 4096, FileOptions.None);
            }
            catch (IOException io)
            {
                Console.WriteLine(io.Message);
                //Console.WriteLine("Для файла \n{0}\nНе может быть посчитана хеш-сумма: " +
                //"Файл используется другим приложением\n", filename);
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Для файла \n\r{0}\n\rНе может быть посчитана хеш-сумма: " +
                    "Отказано в доступе\n", filename);
                return null;
            }
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(fs);
            fs.Flush();
            return hash;
        }

        public static void serialize(Object obj)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("test.bin",
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);
            formatter.Serialize(stream, obj);
            stream.Close();
        }

        public static Object deserialize()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("test.bin",
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            Object obj = formatter.Deserialize(stream);
            stream.Close();
            return obj;
        }
    }

    [Serializable()]
    public class DDirectory : ISerializable
    {
        public string name;
        public List<DDirectory> subdirs = new List<DDirectory>();
        public List<DFile> files = new List<DFile>();
        public bool isChecked = false;
        public bool isDumped = false;

        public DDirectory(string name, bool isDumped)
        {
            this.name = name;
            this.isDumped = isDumped;
        }

        public DDirectory(string name, List<DFile> files, List<DDirectory> subdirs, bool isDumped)
        {
            this.name = name;
            this.files = files;
            this.subdirs = subdirs;
            this.isDumped = isDumped;
        }

        public DDirectory(SerializationInfo info, StreamingContext ctxt)
        {
            name = info.GetString("name");
            files = (List<DFile>)info.GetValue("files", files.GetType());
            subdirs = (List<DDirectory>)info.GetValue("subdirs", subdirs.GetType());
            isDumped = info.GetBoolean("isDumped");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("name", name);
            info.AddValue("files", files);
            info.AddValue("subdirs", subdirs);
            info.AddValue("isDumped", isDumped);
        }
    }

    [Serializable()]
    public class DFile : ISerializable
    {
        public long size;
        public string name;
        public string hash;
        public bool isDumped = false;
        public bool isChecked = false;

        public DFile(long size, string name, string hash, bool isDumped)
        {
            this.size = size;
            this.name = name;
            this.hash = hash;
            this.isDumped = isDumped;
        }

        protected DFile(SerializationInfo info, StreamingContext ctxt)
        {
            size = info.GetInt64("size");
            name = info.GetString("name");
            hash = info.GetString("hash");
            isDumped = info.GetBoolean("isDumped");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("size", size);
            info.AddValue("name", name);
            info.AddValue("hash", hash);
            info.AddValue("isDumped", isDumped);
        }
    }
}