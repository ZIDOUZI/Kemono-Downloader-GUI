using System.Text;

namespace Kemono.Core.Helpers;

public class ConsoleHelper : IDisposable
{
    private readonly TextWriter _doubleWriter;
    private readonly FileStream _fileStream;
    private readonly StreamWriter _outputWriter;
    private readonly TextWriter _systemOut;

    private ConsoleHelper(string folderPath, string fileName)
    {
        _systemOut = Console.Out;

        _fileStream = PathHelper.OpenStream(folderPath, fileName, FileAccess.ReadWrite, FileShare.Read);

        _outputWriter = new StreamWriter(_fileStream)
        {
            AutoFlush = true
        };

        _doubleWriter = new DoubleWriter(_outputWriter, _systemOut);

        Console.SetOut(_doubleWriter);
    }

    public void Dispose()
    {
        Console.SetOut(_systemOut);

        _outputWriter.Flush();
        _outputWriter.Close();
        _fileStream.Close();
        _doubleWriter.Close();
        GC.SuppressFinalize(this);
    }

    public static ConsoleHelper CreateInstance(string folderPath, string fileName)
    {
        try
        {
            return new ConsoleHelper(folderPath, fileName);
        }
        catch (Exception re)
        {
            Console.WriteLine(re);
            return null;
        }
    }

    public async Task Copy(string folderPath, string fileName) =>
        await _fileStream.CopyToAsync(PathHelper.OpenStream(folderPath, fileName));

    public async Task<bool> Export(string folder)
    {
        if (folder == null)
        {
            return false;
        }

        try
        {
            await Copy(folder, "latest-log.log");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private class DoubleWriter : TextWriter
    {
        private readonly TextWriter _writer1;
        private readonly TextWriter _writer2;

        public DoubleWriter(TextWriter one, TextWriter two)
        {
            _writer1 = one;
            _writer2 = two;
        }

        public override Encoding Encoding => _writer1.Encoding;

        public override void Flush()
        {
            _writer1.Flush();
            _writer2.Flush();
        }

        public override void Write(char value)
        {
            _writer1.Write(value);
            _writer2.Write(value);
        }
    }
}