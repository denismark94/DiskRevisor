using System;
using System.IO;
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
            //DFile a = new DFile(512, "a", "a");
            //DFile b = new DFile(1024, "b", "b");
            //DDirectory dir = new DDirectory();
            //dir.files.Add("a", a);
            //dir.files.Add("b", b);
            string hash = BitConverter.ToString(computeFileHash("C:\\\\vm\\olympXP2015.vdi"));
            //string hash = BitConverter.ToString(computeFileHash("C:\\\\vm\\olympXP2015.vbox"));
            Console.WriteLine(hash);
            Console.ReadKey();
        }

        public static DDirectory createDB(string path)
        {   DDirectory curDir = new DDirectory();
            string[] subdirs = Directory.GetDirectories(path);
            for (int i = 0; i < subdirs.Length; i++)
                 curDir.subdirs.Add(subdirs[i],createDB(subdirs[i]));
            string[] files = Directory.GetFiles(path);
            DFile curFile;
            for (int i = 0; i < files.Length; i++)
			{   
                curFile = dump(files[i]);
                curDir.files.Add(files[i], curFile);
			}
            return curDir;
        }

        public static DFile dump(string path)
        {
            FileInfo temp = new FileInfo(path);
            long size = temp.Length;
            byte[] hash = computeFileHash(path);
            string[] name = path.Split('/');
            DFile result = new DFile(size,path,name[name.Length-1]);
            return result;
        }

        private static byte[] computeFileHash(string filename)
        {
            MD5 md5 = MD5.Create();
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                byte[] hash = md5.ComputeHash(fs);
                return hash;
            }
        }
        static void serialize() 
        {
            DFile test = new DFile(512, "a", "b");
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("test.bin", 
                FileMode.Create,
                FileAccess.Write, 
                FileShare.None);
            formatter.Serialize(stream, test);
            stream.Close();
        }

        static void deserialize()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("test.bin",
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            DFile test = (DFile)formatter.Deserialize(stream);
            stream.Close();
            Console.WriteLine("size: {0},\n\rname {1},\n\rhash {2}",
                test.size, test.name, test.hash);
        }
    }

    public class DDirectory
    {
        public Dictionary<string, DDirectory> subdirs;
        public Dictionary<string, DFile> files;

        public DDirectory()
        {
            this.files = new Dictionary<string,DFile>();
            this.subdirs = new Dictionary<string,DDirectory>();
        }

    }
        
    [Serializable()]
    public class DFile : ISerializable
    {
        public long size;
        public string name;
        public string hash;

        public DFile(long size, string name, string hash)
        {
            this.size = size;
            this.name = name;
            this.hash = hash;
        }

        protected DFile(SerializationInfo info, StreamingContext ctxt)
        {
            size = info.GetInt64("size");
            name = info.GetString("name");
            hash = info.GetString("hash");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("size", size);
            info.AddValue("name", name);
            info.AddValue("hash", hash);
        }
    }
}
