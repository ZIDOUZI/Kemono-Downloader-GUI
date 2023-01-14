using System.Diagnostics;

namespace Kemono.Core.Helpers;

public static class PathHelper
{
    public static readonly string AppDataPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "kemono");

    public static string AppPath(string sub) => Path.Combine(AppDataPath, sub);

    public static FileStream OpenStream(string folderPath, string fileName, FileAccess access = FileAccess.Write,
        FileShare share = FileShare.None) =>
        Directory.Exists(folderPath) || Directory.CreateDirectory(folderPath!).Exists
            ? File.Open(Path.Combine(folderPath, fileName), FileMode.OpenOrCreate, access, share)
            : throw new IOException();

    public static void SaveText(string fileName, string text)
    {
        if (Directory.Exists(AppDataPath) || Directory.CreateDirectory(AppDataPath!).Exists)
            File.WriteAllText(Path.Combine(AppDataPath, fileName), text);
        else
            throw new IOException();
    }

    public static void OpenFolder(string path, bool create = true)
    {
        if (!Directory.Exists(path) && create)
        {
            Directory.CreateDirectory(path!);
        }

        Process.Start("Explorer", path!);
    }

    public static bool IsSingleDirectory(this string path) =>
        !Directory.GetFiles(path).Any() && Directory.GetDirectories(path).Length == 1;

    public static DirectoryInfo GetFirstNotSingleDirectory(this string path)
    {
        while (true)
        {
            if (!path.IsSingleDirectory())
            {
                return new DirectoryInfo(path);
            }

            path = Directory.GetDirectories(path).First();
        }
    }
}