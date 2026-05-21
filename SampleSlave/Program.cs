using Going.Basis.Communications.Modbus.TCP;
using Going.Basis.Communications.TextComm.TCP;
using System.Globalization;

await SampleSlaveApp.RunAsync(args);

static class SampleSlaveApp
{
    public static async Task RunAsync(string[] args)
    {
        var options = CliOptions.Parse(args, defaultPort: 1502);

        if (options.Protocol.Equals("text", StringComparison.OrdinalIgnoreCase))
        {
            await new TextSlaveConsole(options.Port).RunAsync();
            return;
        }

        await new ModbusSlaveConsole(options.Port).RunAsync();
    }
}

sealed class ModbusSlaveConsole
{
    private const int BaseAddress = 0;
    private readonly bool[] bits = new bool[65536];
    private readonly int[] words = new int[65536];
    private readonly ModbusTCPSlave slave;

    public ModbusSlaveConsole(int port)
    {
        slave = new ModbusTCPSlave { LocalPort = port };
        slave.SocketConnected += (_, _) => Log("connected");
        slave.SocketDisconnected += (_, _) => Log("disconnected");
        slave.BitReadRequest += (_, e) => ModbusTCPSlave.ProcessBitReads(e, BaseAddress, bits);
        slave.WordReadRequest += (_, e) => ModbusTCPSlave.ProcessWordReads(e, BaseAddress, words);
        slave.BitWriteRequest += (_, e) => ModbusTCPSlave.ProcessBitWrite(e, BaseAddress, bits);
        slave.WordWriteRequest += (_, e) => ModbusTCPSlave.ProcessWordWrite(e, BaseAddress, words);
        slave.MultiBitWriteRequest += (_, e) => ModbusTCPSlave.ProcessMultiBitWrite(e, BaseAddress, bits);
        slave.MultiWordWriteRequest += (_, e) => ModbusTCPSlave.ProcessMultiWordWrite(e, BaseAddress, words);
        slave.WordBitSetRequest += (_, e) => ModbusTCPSlave.ProcessWordBitSet(e, BaseAddress, words);
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"SampleSlave modbus port={slave.LocalPort}");
        PrintHelp();

        while (true)
        {
            Console.Write("slave> ");
            var line = await Console.In.ReadLineAsync();
            if (line == null) break;

            var tokens = Parsing.Split(line);
            if (tokens.Length == 0) continue;

            try
            {
                switch (tokens[0].ToLowerInvariant())
                {
                    case "start":
                        slave.Start();
                        Log("started");
                        break;
                    case "stop":
                        slave.Stop();
                        Log("stopped");
                        break;
                    case "restart":
                        slave.Stop();
                        slave.Start();
                        Log("restarted");
                        break;
                    case "status":
                        Console.WriteLine($"IsStart={slave.IsStart}, Port={slave.LocalPort}");
                        break;
                    case "setbit":
                        bits[Parsing.Int(tokens, 1)] = Parsing.Bool(tokens, 2);
                        break;
                    case "setword":
                        words[Parsing.Int(tokens, 1)] = Parsing.Int(tokens, 2);
                        break;
                    case "getbit":
                        Console.WriteLine(bits[Parsing.Int(tokens, 1)] ? "1" : "0");
                        break;
                    case "getword":
                        Console.WriteLine(words[Parsing.Int(tokens, 1)]);
                        break;
                    case "dump":
                        Dump(Parsing.Int(tokens, 1), Parsing.Int(tokens, 2));
                        break;
                    case "help":
                        PrintHelp();
                        break;
                    case "quit":
                    case "exit":
                        slave.Stop();
                        return;
                    default:
                        Console.WriteLine("unknown command. type help.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
            }
        }

        slave.Stop();
    }

    private void Dump(int start, int length)
    {
        for (var i = 0; i < length; i++)
        {
            var address = start + i;
            Console.WriteLine($"{address}: bit={(bits[address] ? 1 : 0)}, word={words[address]}");
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("commands: start, stop, restart, status, setbit <addr> <0|1>, setword <addr> <value>, getbit <addr>, getword <addr>, dump <addr> <len>, quit");
    }

    private static void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
}

sealed class TextSlaveConsole
{
    private readonly TextCommTCPSlave slave;

    public TextSlaveConsole(int port)
    {
        slave = new TextCommTCPSlave { LocalPort = port };
        slave.SocketConnected += (_, _) => Log("connected");
        slave.SocketDisconnected += (_, _) => Log("disconnected");
        slave.MessageRequest += (_, e) =>
        {
            Console.WriteLine($"request slave={e.Slave} command={e.Command} message={e.RequestMessage}");
            e.ResponseMessage = $"OK:{e.RequestMessage}";
        };
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"SampleSlave text port={slave.LocalPort}");
        Console.WriteLine("commands: start, stop, restart, status, quit");

        while (true)
        {
            Console.Write("slave-text> ");
            var line = await Console.In.ReadLineAsync();
            if (line == null) break;

            switch (line.Trim().ToLowerInvariant())
            {
                case "start":
                    slave.Start();
                    Log("started");
                    break;
                case "stop":
                    slave.Stop();
                    Log("stopped");
                    break;
                case "restart":
                    slave.Stop();
                    slave.Start();
                    Log("restarted");
                    break;
                case "status":
                    Console.WriteLine($"IsStart={slave.IsStart}, Port={slave.LocalPort}");
                    break;
                case "quit":
                case "exit":
                    slave.Stop();
                    return;
                default:
                    Console.WriteLine("commands: start, stop, restart, status, quit");
                    break;
            }
        }

        slave.Stop();
    }

    private static void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
}

sealed record CliOptions(string Protocol, int Port)
{
    public static CliOptions Parse(string[] args, int defaultPort)
    {
        var protocol = "modbus";
        var port = defaultPort;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--protocol" when i + 1 < args.Length:
                    protocol = args[++i];
                    break;
                case "--port" when i + 1 < args.Length:
                    port = Parsing.Number(args[++i]);
                    break;
            }
        }

        return new CliOptions(protocol, port);
    }
}

static class Parsing
{
    public static string[] Split(string line) => line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static int Int(string[] tokens, int index)
    {
        if (index >= tokens.Length) throw new ArgumentException("missing argument");
        return Number(tokens[index]);
    }

    public static bool Bool(string[] tokens, int index)
    {
        if (index >= tokens.Length) throw new ArgumentException("missing argument");
        return tokens[index] is "1" or "true" or "on";
    }

    public static int Number(string value)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.Parse(value[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return int.Parse(value, CultureInfo.InvariantCulture);
    }
}
