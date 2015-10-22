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
        static void Main(string[] args)
        {
            //DDirectory dump = createContext("C:\\Users\\admin", 0);
            //serialize(dump);
            //Console.WriteLine("Serialization successfull");
            DDirectory dump = (DDirectory)deserialize();
            Console.WriteLine("Deserialization successfull");
            print(dump);
            Console.Write("Press any key to quit");
            Console.ReadKey();
        }

        static void check(string path) 
        {
            DDirectory dump = (DDirectory)deserialize();
            string[] splpath = path.Split('\\');

        }

        static DDirectory checkAvailabilty(string path, int iteration)
        {
            string[] splpath = path.Split('\\');
        }

        public static void print(DDirectory dir)
        {
            List<DFile> files = dir.files;
            List<DDirectory> subdirs = dir.subdirs;
            for (int i = 0; i < files.Count; i++)
                    Console.WriteLine(files[i].name);
            for (int i = 0; i < subdirs.Count; i++)
            {
                print(subdirs[i]);
                Console.WriteLine("Name: {0} Dumped:{1}",subdirs[i].name,subdirs[i].isDumped);
            }
        }

        public static DDirectory createContext(string path, int iteration)
        {
            string[] folders = path.Split('\\');
            string sub = "";
            DDirectory dir;
            if (iteration < folders.Length)
            {
                for (int i = 0; i < iteration; i++)
                    sub += folders[i] + '\\';
                dir = new DDirectory(sub, false);
                dir.subdirs.Add(createContext(path, ++iteration));
            }
            else
                dir = createDB(path);
            return dir;
        }

        public static DDirectory createDB(string path)
        {
            string[] subdirs, files;
            try
            {
                subdirs = Directory.GetDirectories(path);
                files = Directory.GetFiles(path);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Доступ запрещен к папке {0}",path);
                return new DDirectory(path,false);
            }
            List<DDirectory> dsd = new List<DDirectory>();
            List<DFile> df = new List<DFile>();
            for (int i = 0; i < subdirs.Length; i++)
                dsd.Add(createDB(subdirs[i]));
            for (int i = 0; i < files.Length; i++)
                df.Add(dump(files[i]));
            return new DDirectory(path, df, dsd, true);
        }

        public static DFile dump(string path)
        {
            FileInfo temp = new FileInfo(path);
            long size = temp.Length;
            byte[] hashBytes = computeFileHash(path);
            if (hashBytes == null)
            {
                
                return new DFile(size,path,null,false);
            }
            string hash = BitConverter.ToString(hashBytes);
            DFile result = new DFile(size, path, hash, true);
            return result;
        }

        private static byte[] computeFileHash(string filename)
        {
            FileStream fs;
            try
            {
                fs = new FileStream(filename, FileMode.Open);
            }
            catch (IOException e)
            {
                Console.WriteLine("Для файла \n\r{0}\n\r не может быть посчитана хеш-сумма: "+ 
                "Файл используется другим приложением", filename);
                return null;
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine("Для файла \n\r{0}\n\r не может быть посчитана хеш-сумма: " +
                    "Отказано в доступе", filename);
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
        bool isChecked;
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