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
    protected string _fileName { get; set; }

    public FileDataSource(string fileName)
    {
        _fileName = fileName;
    }

    public void WriteData(string data)
    {
        File.WriteAllText(_fileName, data);

    }
    public string ReadData()
    {

        return File.ReadAllText(_fileName);

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
        public static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();

                }
                return result.ToArray();
            }
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
            base.WriteData(data);
            var dataBytes = Encoding.Default.GetBytes(data);
            var compressedData = Compress(dataBytes);
            _source.WriteData(Encoding.Default.GetString(compressedData));
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
            var data = "Salam,muellim!";
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "word.txt");
            IDataSource dataSource = new FileDataSource(path);
            dataSource = new EncryptionDecorator(dataSource);
            //dataSource = new CompressionDecorator(dataSource);
            dataSource.WriteData(data);
            Console.WriteLine(dataSource.ReadData());
        }
    }
}
