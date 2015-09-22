using System;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.DataContractAttribute;
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
            //string hash = BitConverter.ToString(computeFileHash("C:\\\\vm\\olympXP2015.vdi"));
            //string hash = BitConverter.ToString(computeFileHash("C:\\\\vm\\olympXP2015.vbox"));
            //Console.WriteLine(hash);
            /*DDirectory test = createDB("C:\\\\Temp");
            string hash = "123";
            DDirectory temp;
            test.subdirs.TryGetValue("C:\\\\Temp\\123", out temp);
            DFile result;
            temp.files.TryGetValue("C:\\\\Temp\\123\\1.bat", out result);
            Console.WriteLine(result.hash);
            Console.WriteLine(BitConverter.ToString(computeFileHash("C:\\\\temp\\123\\1.bat")));
            Console.ReadKey();*/
            var serializer = new DataContractSerializer(typeof(FileMap));
            string xmlString;
            using (var sw = new StringWriter())
            {
                using (var writer = new XmlTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented; // indent the Xml so it's human readable
                    serializer.WriteObject(writer, marketplace);
                    writer.Flush();
                    xmlString = sw.ToString();
                }
            }
        }

        //public static DDirectory createDB(string path)
        //{   DDirectory curDir = new DDirectory();
        //    string[] subdirs = Directory.GetDirectories(path);
        //    for (int i = 0; i < subdirs.Length; i++)
        //         curDir.subdirs.Add(subdirs[i],createDB(subdirs[i]));
        //    string[] files = Directory.GetFiles(path);
        //    DFile curFile;
        //    for (int i = 0; i < files.Length; i++)
        //    {   
        //        curFile = dump(files[i]);
        //        curDir.files.Add(files[i], curFile);
        //    }
        //    return curDir;
        //}

        //public static DFile dump(string path)
        //{
        //    FileInfo temp = new FileInfo(path);
        //    long size = temp.Length;

        //    byte[] hashBytes = computeFileHash(path);
        //    string hash = BitConverter.ToString(computeFileHash(path));
        //    string[] name = path.Split('/');
        //    DFile result = new DFile(size,name[name.Length-1],hash);
        //    return result;
        //}

        private static byte[] computeFileHash(string filename)
        {
            MD5 md5 = MD5.Create();
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                byte[] hash = md5.ComputeHash(fs);
                return hash;
            }
        }
        //static void serialize() 
        //{
        //    DFile test = new DFile(512, "a", "b");
        //    IFormatter formatter = new BinaryFormatter();
        //    Stream stream = new FileStream("test.bin", 
        //        FileMode.Create,
        //        FileAccess.Write, 
        //        FileShare.None);
        //    formatter.Serialize(stream, test);
        //    stream.Close();
        //}

        //static void deserialize()
        //{
        //    IFormatter formatter = new BinaryFormatter();
        //    Stream stream = new FileStream("test.bin",
        //        FileMode.Open,
        //        FileAccess.Read,
        //        FileShare.Read);

        //    DFile test = (DFile)formatter.Deserialize(stream);
        //    stream.Close();
        //    Console.WriteLine("size: {0},\n\rname {1},\n\rhash {2}",
        //        test.size, test.name, test.hash);
        //}
    //}

    //[Serializable()]
    //public class DDirectory : ISerializable
    //{
    //    public Dictionary<string, DDirectory> subdirs;
    //    public FileMap files;

    //    public DDirectory()
    //    {
    //        this.files = new FileMap();
    //        this.subdirs = new Dictionary<string,DDirectory>();
    //    }

    //    public DDirectory(SerializationInfo info, StreamingContext ctxt)
    //    {
    //        files = info.GetValue("files",);
    //    }

    //}
        
    //[Serializable()]
    //public class DFile : ISerializable
    //{
    //    public long size;
    //    public string name;
    //    public string hash;

    //    public DFile(long size, string name, string hash)
    //    {
    //        this.size = size;
    //        this.name = name;
    //        this.hash = hash;
    //    }

    //    protected DFile(SerializationInfo info, StreamingContext ctxt)
    //    {
    //        size = info.GetInt64("size");
    //        name = info.GetString("name");
    //        hash = info.GetString("hash");
    //    }

    //    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    //    {
    //        info.AddValue("size", size);
    //        info.AddValue("name", name);
    //        info.AddValue("hash", hash);
    //    }
    //}

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
