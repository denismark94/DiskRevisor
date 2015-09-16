using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace DiskRevisor
{
    class Program
    {
        static void Main(string[] args)
        {
            DFile a = new DFile(512, "a", "a");
            DFile b = new DFile(1024, "b", "b");
            DDirectory dir = new DDirectory();
            dir.files.Add("a",a);
            dir.files.Add("b",b);
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
            int size = File.
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

    [Serializable()]
    public class DDirectory : ISerializable
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
        public int size;
        public string name;
        public string hash;

        public DFile(int size, string name, string hash)
        {
            this.size = size;
            this.name = name;
            this.hash = hash;
        }

        protected DFile(SerializationInfo info, StreamingContext ctxt)
        {
            size = info.GetInt32("size");
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
