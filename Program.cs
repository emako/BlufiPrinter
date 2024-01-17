using BlufiPrinter;
using Dumpify;
using System.IO.Ports;
using System.Reflection;
using System.Text.Json;

BlufiPrinterJson? json = null;
ConsolePrinter.PrintInternal($"Blufi Printer v{Assembly.GetExecutingAssembly().GetName().Version!.Major}");
ConsolePrinter.PrintInternal("Program Url: https://github.com/emako/BlufiPrinter");
NativeMethod.DisableQuickEditMode();

string jsonFile = Path.Combine(Path.GetTempPath(), "BlufiPrinter.json");
string? comName = null;

var tabs = SerialPortProvider.GetPortNames()
    .Select(tab => $"{tab.ComName} : {tab.ActualName}")
    .ToList();

READ_COMNAME:
ConsolePrinter.PrintInternal("Serial Port list:");
ConsolePrinter.PrintInternal(tabs.DumpText());

if (File.Exists(jsonFile))
{
    try
    {
        string jsonString = File.ReadAllText(jsonFile);
        json = JsonSerializer.Deserialize<BlufiPrinterJson>(jsonString);

        if (json != null)
        {
            ConsolePrinter.PrintInternal($"Try to connect the latest device {json.ComName}.");
            ConsolePrinter.PrintInternal("Please input another device COM number in 2 seconds ...");
            Task<string?> readTask = Task.Run(Console.ReadLine);
            Task completedTask = await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromSeconds(2)));

            if (completedTask == readTask)
            {
                comName = await readTask;
            }
            else
            {
                comName = json.ComName.ToString();
            }
        }
    }
    catch
    {
    }
}

if (string.IsNullOrWhiteSpace(comName))
{
    ConsolePrinter.PrintInternal("Please input device COM number ...");
    comName = Console.ReadLine();
}

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

    json ??= new();
    json.ComName = comName;
    File.WriteAllText(jsonFile, JsonSerializer.Serialize(json));
    ConsolePrinter.PrintInternal($"Connected to {serialPort.PortName} (BaudRate={serialPort.BaudRate}, StopBits={(int)serialPort.StopBits}, DataBits={serialPort.DataBits}, Parity={serialPort.Parity}).");
}
catch (Exception e)
{
    ConsolePrinter.PrintInternal(e.Message);
    serialPort.Dispose();
    goto READ_COMNAME;
}

while (true)
{
    try
    {
        if (!serialPort.IsOpen)
        {
            Thread.Sleep(1000);
            serialPort.Open();
        }

        ConsolePrinter.Print(serialPort.ReadLine());
    }
    catch (Exception e)
    {
        ConsolePrinter.PrintInternal(e.Message);
    }
}
