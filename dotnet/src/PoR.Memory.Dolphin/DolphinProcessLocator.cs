using System.Runtime.InteropServices;

namespace PoR.Memory.Dolphin;

internal sealed record DolphinProcessInfo(uint ProcessId, string ExecutableName);

internal static class DolphinProcessLocator
{
    private static readonly string[] DefaultProcessNames =
    [
        "Dolphin.exe",
        "DolphinQt2.exe",
        "DolphinWx.exe",
    ];

    public static DolphinProcessInfo? Find()
    {
        var names = ResolveProcessNames();
        var snapshot = NativeMethods.CreateToolhelp32Snapshot(
            NativeMethods.TH32CS_SNAPPROCESS,
            0);

        if (snapshot == NativeMethods.InvalidHandleValue)
        {
            return null;
        }

        try
        {
            var entry = new NativeMethods.PROCESSENTRY32W
            {
                dwSize = (uint)Marshal.SizeOf<NativeMethods.PROCESSENTRY32W>(),
            };

            if (!NativeMethods.Process32FirstW(snapshot, ref entry))
            {
                return null;
            }

            do
            {
                var exeName = entry.szExeFile;
                if (string.IsNullOrEmpty(exeName))
                {
                    continue;
                }

                foreach (var name in names)
                {
                    if (string.Equals(exeName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return new DolphinProcessInfo(entry.th32ProcessID, exeName);
                    }
                }
            }
            while (NativeMethods.Process32NextW(snapshot, ref entry));

            return null;
        }
        finally
        {
            NativeMethods.CloseHandle(snapshot);
        }
    }

    private static string[] ResolveProcessNames()
    {
        var customName = Environment.GetEnvironmentVariable("DME_DOLPHIN_PROCESS_NAME");
        if (string.IsNullOrWhiteSpace(customName))
        {
            return DefaultProcessNames;
        }

        customName = customName.Trim();
        if (customName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return [customName, Path.GetFileNameWithoutExtension(customName)];
        }

        return [customName, customName + ".exe"];
    }
}
