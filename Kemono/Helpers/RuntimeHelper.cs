using System.Runtime.InteropServices;
using System.Text;
using Windows.ApplicationModel;

namespace Kemono.Helpers;

public class RuntimeHelper
{
    public static bool IsMSIX
    {
        get
        {
            var length = 0;

            return GetCurrentPackageFullName(ref length, null) != 15700L;
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);

    public static string Version
    {
        get
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}