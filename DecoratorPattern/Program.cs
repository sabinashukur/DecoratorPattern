using System.IO.Compression;
using System.Text;

namespace MyApp;

interface IDataSource
{
    void WriteData(string data);
    string ReadData();
}

class FileDataSource : IDataSource
{

    public string _path;
    protected string _fileName { get; set; }

    public FileDataSource(string fileName)
    {
        _fileName = fileName;
        _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), _fileName + ".txt");
    }

    public void WriteData(string data)
    {
        File.WriteAllText(_path, data);

    }
    public string ReadData()
    {

        return File.ReadAllText(_path);

    }
    class DataSourceDecorator : IDataSource
    {
        protected IDataSource _source;

        protected DataSourceDecorator(IDataSource source)
        {
            _source = source;
        }
        public virtual void WriteData(string data)
        {
            _source.WriteData(data);
        }

        public virtual string ReadData()
        {
            return _source.ReadData();

        }
    }
    class EncryptionDecorator : DataSourceDecorator
    {
        public EncryptionDecorator(IDataSource source) : base(source) { }
        public override void WriteData(string data)
        {
            base.WriteData(data);
            var dataBytes = Encoding.Default.GetBytes(data);
            byte code = 4;
            for (int i = 0; i < dataBytes.Length; i++)
            {
                dataBytes[i] ^= code;
            }
            _source.WriteData(Encoding.Default.GetString(dataBytes));
        }
        public override string ReadData()
        {
            var data = base.ReadData();
            var dataBytes = Encoding.Default.GetBytes(data);
            byte code = 4;
            for (int i = 0; i < dataBytes.Length; i++)
            {
                dataBytes[i] ^= code;
            }
            return Encoding.Default.GetString(dataBytes);
        }
    }
    class CompressionDecorator : DataSourceDecorator
    {
        public CompressionDecorator(IDataSource source) : base(source) { }
        public string CompressString(string text)
        {
            FileInfo fileToCompress = new(@"C:\Users\Documents\Desktop\word.txt");
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                {
                    using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                        {
                            originalFileStream.CopyTo(compressionStream);
                            Console.WriteLine("Zip succesfully done");
                        }
                    }
                }
            }
            File.Delete(@"C:\Users\Documents\Desktop\word.txt");
            return "";
        }
        public static byte[] Decompress(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {

                using (var outputStream = new MemoryStream())
                {
                    using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(outputStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }
        public override void WriteData(string data)
        {
            CompressString(data);

        }

        public override string ReadData()
        {
            var data = base.ReadData();
            var dataBytes = Encoding.Default.GetBytes(data);
            var decompressedData = Decompress(dataBytes);
            return Encoding.Default.GetString(decompressedData);

        }
    }
    class Program
    {
        static void Main(string[] args)
        {

            var data = "Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!Salam,muellim!!!!!";
            IDataSource dataSource = new FileDataSource("word");
            dataSource.WriteData(data);
            dataSource = new EncryptionDecorator(dataSource);
            dataSource.WriteData(data);

            dataSource = new CompressionDecorator(dataSource);
            dataSource.WriteData(data);

            //Console.WriteLine(dataSource.ReadData());
        }
    }
}
