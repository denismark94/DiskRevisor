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
            //serialize();

            
            //DDirectory dump = createDB("D:\\\\Downloads\\WinSetupFromUSB-1-6-beta2");
            //serialize(dump);
            //Console.WriteLine("Serialization successfull");
            DDirectory dump = (DDirectory)deserialize();
            Console.WriteLine("Deserialization successfull");
            print(dump);
            Console.Write("Press any key to quit");
            Console.ReadKey();
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
                    Console.WriteLine(subdirs[i].name);
                }
        }

        public static DDirectory createDB(string path)
        {
            string[] subdirs = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            List<DDirectory> dsd = new List<DDirectory>();
            List<DFile> df = new List<DFile>();
            for (int i = 0; i < subdirs.Length; i++)
                dsd.Add(createDB(subdirs[i]));
            for (int i = 0; i < files.Length; i++)
                df.Add(dump(files[i]));
            return new DDirectory(path, df,dsd, true);
        }

        public static DFile dump(string path)
        {
            FileInfo temp = new FileInfo(path);
            long size = temp.Length;
            byte[] hashBytes = computeFileHash(path);
            string hash = BitConverter.ToString(computeFileHash(path));
            DFile result = new DFile(size, path, hash, true);
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


        static void serialize(Object obj) 
        {             
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("test.bin", 
                FileMode.Create,
                FileAccess.Write, 
                FileShare.None);
            formatter.Serialize(stream, obj);
            stream.Close();
        }

        static Object deserialize()
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
            bool isDumped = false;

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
                isDumped =  info.GetBoolean("isDumped");
            }

            public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
            {
                info.AddValue("size", size);
                info.AddValue("name", name);
                info.AddValue("hash", hash);
                info.AddValue("isDumped", isDumped);
            }
        }

        [DataContract]
        public class FileMap
        {
            // need a parameterless constructor for serialization
            public FileMap()
            {
                files = new Dictionary<string, DFile>();
            }
            [DataMember]
            public Dictionary<string, DFile> files { get; set; }
        }
    }