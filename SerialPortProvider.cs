using Microsoft.Win32;
using System.IO.Ports;
using System.Management;

namespace BlufiPrinter;

internal static partial class SerialPortProvider
{
    public static SerialPortTab[] GetPortNames()
    {
        List<SerialPortTab> tabs = [];
        string[] portNames = SerialPort.GetPortNames();

        try
        {
            using ManagementObjectSearcher searcher = new($"SELECT * FROM {HardwareEnum.Win32_PnPEntity} WHERE (PNPClass = 'Ports' OR ClassGuid = '{{4D36E978-E325-11CE-BFC1-08002BE10318}}')");
            using ManagementObjectCollection ports = searcher.Get();

            foreach (ManagementObject port in ports.Cast<ManagementObject>())
            {
                try
                {
                    string? name = port.GetPropertyValue("Name")?.ToString();
                    string? caption = port.GetPropertyValue("Caption")?.ToString();
                    string? deviceId = port.GetPropertyValue("PnpDeviceID")?.ToString();
                    string? regPath = @$"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Enum\{deviceId}\Device Parameters";
                    string? portName = Registry.GetValue(regPath, "PortName", string.Empty)?.ToString();
                    bool isIgnored = !portNames?.Contains(portName) ?? false;

                    if (!isIgnored)
                    {
                        tabs.Add(new SerialPortTab()
                        {
                            ComName = portName!,
                            ActualName = name!,
                        });
                    }
                }
                catch (Exception e)
                {
                    ConsolePrinter.Print(e.Message);
                }
            }
        }
        catch (Exception e)
        {
            ConsolePrinter.Print(e.Message);
        }

        portNames?.Except(tabs.Select(tab => tab.ComName))
            .ForEach(miss => tabs.Add(new SerialPortTab() { ComName = miss }));
        return SerialPortTab.Sort(tabs).ToArray();
    }
}

internal class SerialPortTab
{
    public string Name => !string.IsNullOrEmpty(ActualName) ? ActualName : ComName;
    public string ActualName { get; set; } = null!;
    public string ComName { get; set; } = null!;

    public static IEnumerable<SerialPortTab> Sort(IEnumerable<SerialPortTab> tabs)
    {
        SerialPortTab[]? array = tabs?.ToArray();

        if (array != null)
        {
            Array.Sort(array, new SerialPortTabComparer());
        }
        return array!;
    }

    protected class SerialPortTabComparer : IComparer<SerialPortTab>
    {
        int IComparer<SerialPortTab>.Compare(SerialPortTab? x, SerialPortTab? y)
        {
            int xNumber = GetNumberFromPortName(x);
            int yNumber = GetNumberFromPortName(y);

            return xNumber.CompareTo(yNumber);
        }

        private static int GetNumberFromPortName(SerialPortTab? tab)
        {
            string? numberString = tab?.ComName[3..];
            if (int.TryParse(numberString, out int number))
            {
                return number;
            }
            return 0;
        }
    }
}

file enum HardwareEnum
{
    Win32_Processor,
    Win32_PhysicalMemory,
    Win32_Keyboard,
    Win32_PointingDevice,
    Win32_FloppyDrive,
    Win32_DiskDrive,
    Win32_CDROMDrive,
    Win32_BaseBoard,
    Win32_BIOS,
    Win32_ParallelPort,
    Win32_SerialPort,
    Win32_SerialPortConfiguration,
    Win32_SoundDevice,
    Win32_SystemSlot,
    Win32_USBController,
    Win32_NetworkAdapter,
    Win32_NetworkAdapterConfiguration,
    Win32_Printer,
    Win32_PrinterConfiguration,
    Win32_PrintJob,
    Win32_TCPIPPrinterPort,
    Win32_POTSModem,
    Win32_POTSModemToSerialPort,
    Win32_DesktopMonitor,
    Win32_DisplayConfiguration,
    Win32_DisplayControllerConfiguration,
    Win32_VideoController,
    Win32_VideoSettings,
    Win32_TimeZone,
    Win32_SystemDriver,
    Win32_DiskPartition,
    Win32_LogicalDisk,
    Win32_LogicalDiskToPartition,
    Win32_LogicalMemoryConfiguration,
    Win32_PageFile,
    Win32_PageFileSetting,
    Win32_BootConfiguration,
    Win32_ComputerSystem,
    Win32_OperatingSystem,
    Win32_StartupCommand,
    Win32_Service,
    Win32_Group,
    Win32_GroupUser,
    Win32_UserAccount,
    Win32_Process,
    Win32_Thread,
    Win32_Share,
    Win32_NetworkClient,
    Win32_NetworkProtocol,
    Win32_PnPEntity,
}

file static class LinqExtension
{
    public static void ForEach<TSource>(this IEnumerable<TSource> ts, Action<TSource> action)
    {
        foreach (TSource t in ts)
        {
            action(t);
        }
    }
}
