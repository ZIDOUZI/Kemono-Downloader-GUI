using System.IO.Compression;
using Kemono.Core.Models;

namespace Kemono.Core.Helpers;

public class FileExtract
{
    private static readonly string Temp = PathHelper.AppPath("temp");

    public FileExtract(string zipFile, string target)
    {
        ZipFile.ExtractToDirectory(zipFile, Temp);
        var sourceMD5 = Directory.GetFiles(target).Select(file => file.GetMD5HashFromFile());
        var extractMD5 = Directory.GetFiles(Temp);
    }

    public static void ExtractByMD5(string zipFile, string target)
    {
        ZipFile.ExtractToDirectory(zipFile, target);
        var sourceMD5 = Directory.GetFiles(target).Select(s => s.GetMD5HashFromFile());
        var extractMD5 = Directory.GetFiles(Temp).Select(s => s.GetMD5HashFromFile());
    }
}