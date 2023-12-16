using BlufiPrinter;
using Dumpify;
using System.IO.Ports;

ConsolePrinter.PrintInternal("Blufi Printer");
NativeMethod.DisableQuickEditMode();

var tabs = SerialPortProvider.GetPortNames()
    .Select(tab => $"{tab.ComName} : {tab.ActualName}")
    .ToList();

READ_COMNAME:
ConsolePrinter.PrintInternal("Serial Port list:");
ConsolePrinter.PrintInternal(tabs.DumpText());
ConsolePrinter.PrintInternal($"Please input device COM number ...");
string? comName = Console.ReadLine();

if (string.IsNullOrWhiteSpace(comName))
{
    goto READ_COMNAME;
}
if (!comName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
{
    comName = $"COM{comName}";
}
if (!SerialPort.GetPortNames().Contains(comName))
{
    ConsolePrinter.PrintInternal($"Not avaliabled COM name of `{comName}`.");
    goto READ_COMNAME;
}

SerialPort serialPort = new(comName)
{
    BaudRate = 115200,
    StopBits = StopBits.One,
    DataBits = 8,
    Parity = Parity.None,
};

try
{
    serialPort.Open();
    ConsolePrinter.PrintInternal($"Connected to {serialPort.PortName}.");
}
catch (Exception e)
{
    ConsolePrinter.PrintInternal(e.Message);
    goto READ_COMNAME;
}

while (true)
{
    ConsolePrinter.Print(serialPort.ReadLine());
}
