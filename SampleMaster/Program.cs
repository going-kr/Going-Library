using Going.Basis.Communications.Modbus.TCP;
using Going.Basis.Communications.TextComm.TCP;
using System.Diagnostics;
using System.Globalization;

await SampleMasterApp.RunAsync(args);

static class SampleMasterApp
{
    public static async Task RunAsync(string[] args)
    {
        var options = CliOptions.Parse(args, defaultPort: 1502);

        if (options.SelfTest)
        {
            if (options.ExternalSlave)
                await ExternalSlaveSelfTest.RunAsync(options);
            else if (options.Protocol.Equals("text", StringComparison.OrdinalIgnoreCase))
                await TextSelfTest.RunAsync(options.Port);
            else
                await ModbusSelfTest.RunAsync(options.Port);
            return;
        }

        if (options.Protocol.Equals("text", StringComparison.OrdinalIgnoreCase))
        {
            await new TextMasterConsole(options.Host, options.Port).RunAsync();
            return;
        }

        await new ModbusMasterConsole(options.Host, options.Port).RunAsync();
    }
}

sealed class ModbusMasterConsole
{
    private readonly ModbusTCPMaster master;
    private int messageId = 1;

    public ModbusMasterConsole(string host, int port)
    {
        master = new ModbusTCPMaster
        {
            RemoteIP = host,
            RemotePort = port,
            Timeout = 300,
            Interval = 50,
            AutoReconnect = true,
        };

        WireEvents(master);
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"SampleMaster modbus {master.RemoteIP}:{master.RemotePort}");
        PrintHelp();

        while (true)
        {
            Console.Write("master> ");
            var line = await Console.In.ReadLineAsync();
            if (line == null) break;

            var tokens = Parsing.Split(line);
            if (tokens.Length == 0) continue;

            try
            {
                switch (tokens[0].ToLowerInvariant())
                {
                    case "start":
                        master.Start();
                        Log("started");
                        break;
                    case "stop":
                        master.Stop();
                        Log("stopped");
                        break;
                    case "restart":
                        master.Stop();
                        master.Start();
                        Log("restarted");
                        break;
                    case "status":
                        Console.WriteLine($"IsStart={master.IsStart}, IsOpen={master.IsOpen}, AutoReconnect={master.AutoReconnect}, Remote={master.RemoteIP}:{master.RemotePort}");
                        break;
                    case "read-bit1":
                        master.ManualBitRead_FC1(NextId(), 1, Parsing.Int(tokens, 1), Parsing.Int(tokens, 2));
                        break;
                    case "read-bit2":
                        master.ManualBitRead_FC2(NextId(), 1, Parsing.Int(tokens, 1), Parsing.Int(tokens, 2));
                        break;
                    case "read-word3":
                        master.ManualWordRead_FC3(NextId(), 1, Parsing.Int(tokens, 1), Parsing.Int(tokens, 2));
                        break;
                    case "read-word4":
                        master.ManualWordRead_FC4(NextId(), 1, Parsing.Int(tokens, 1), Parsing.Int(tokens, 2));
                        break;
                    case "write-bit5":
                        master.ManualBitWrite_FC5(NextId(), 1, Parsing.Int(tokens, 1), Parsing.Bool(tokens, 2));
                        break;
                    case "write-word6":
                        master.ManualWordWrite_FC6(NextId(), 1, Parsing.Int(tokens, 1), Parsing.Int(tokens, 2));
                        break;
                    case "write-bits15":
                        master.ManualMultiBitWrite_FC15(NextId(), 1, Parsing.Int(tokens, 1), Parsing.BoolArray(tokens, 2));
                        break;
                    case "write-words16":
                        master.ManualMultiWordWrite_FC16(NextId(), 1, Parsing.Int(tokens, 1), Parsing.IntArray(tokens, 2));
                        break;
                    case "bitset26":
                        master.ManualWordBitSet_FC26(NextId(), 1, Parsing.Int(tokens, 1), (byte)Parsing.Int(tokens, 2), Parsing.Bool(tokens, 3));
                        break;
                    case "auto-word3":
                        master.AutoWordRead_FC3(Parsing.Int(tokens, 1), 1, Parsing.Int(tokens, 2), Parsing.Int(tokens, 3));
                        break;
                    case "auto-bit1":
                        master.AutoBitRead_FC1(Parsing.Int(tokens, 1), 1, Parsing.Int(tokens, 2), Parsing.Int(tokens, 3));
                        break;
                    case "clear-auto":
                        master.ClearAuto();
                        break;
                    case "clear-manual":
                        master.ClearManual();
                        break;
                    case "stress":
                        EnqueueStress(Parsing.Int(tokens, 1));
                        break;
                    case "help":
                        PrintHelp();
                        break;
                    case "quit":
                    case "exit":
                        master.Stop();
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

        master.Stop();
    }

    private int NextId() => messageId++;

    private void EnqueueStress(int count)
    {
        for (var i = 0; i < count; i++)
        {
            master.ManualWordWrite_FC6(NextId(), 1, i % 32, i);
            master.ManualWordRead_FC3(NextId(), 1, i % 32, 1);
        }
    }

    public static void WireEvents(ModbusTCPMaster master)
    {
        master.SocketConnected += (_, _) => Log("connected");
        master.SocketDisconnected += (_, _) => Log("disconnected");
        master.TimeoutReceived += (_, e) => Log($"timeout id={e.MessageID} fc={e.Function} addr={e.StartAddress}");
        master.BitReadReceived += (_, e) => Log($"bit-read id={e.MessageID} fc={e.Function} addr={e.StartAddress} values={string.Join(',', e.ReceiveData.Select(v => v ? 1 : 0))}");
        master.WordReadReceived += (_, e) => Log($"word-read id={e.MessageID} fc={e.Function} addr={e.StartAddress} values={string.Join(',', e.ReceiveData)}");
        master.BitWriteReceived += (_, e) => Log($"bit-write id={e.MessageID} addr={e.StartAddress} value={(e.WriteValue ? 1 : 0)}");
        master.WordWriteReceived += (_, e) => Log($"word-write id={e.MessageID} addr={e.StartAddress} value={e.WriteValue}");
        master.MultiBitWriteReceived += (_, e) => Log($"multi-bit-write id={e.MessageID} addr={e.StartAddress} len={e.Length}");
        master.MultiWordWriteReceived += (_, e) => Log($"multi-word-write id={e.MessageID} addr={e.StartAddress} len={e.Length}");
        master.WordBitSetReceived += (_, e) => Log($"word-bitset id={e.MessageID} addr={e.StartAddress} bit={e.BitIndex} value={(e.WriteValue ? 1 : 0)}");
    }

    private static void PrintHelp()
    {
        Console.WriteLine("commands: start, stop, restart, status, read-bit1 <addr> <len>, read-bit2 <addr> <len>, read-word3 <addr> <len>, read-word4 <addr> <len>");
        Console.WriteLine("          write-bit5 <addr> <0|1>, write-word6 <addr> <value>, write-bits15 <addr> <0,1,...>, write-words16 <addr> <v1,v2,...>, bitset26 <addr> <bit> <0|1>");
        Console.WriteLine("          auto-bit1 <id> <addr> <len>, auto-word3 <id> <addr> <len>, clear-auto, clear-manual, stress <count>, quit");
    }

    private static void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
}

sealed class TextMasterConsole
{
    private readonly TextCommTCPMaster master;
    private int messageId = 1;

    public TextMasterConsole(string host, int port)
    {
        master = new TextCommTCPMaster
        {
            RemoteIP = host,
            RemotePort = port,
            Timeout = 300,
            Interval = 50,
            AutoReconnect = true,
        };

        master.SocketConnected += (_, _) => Log("connected");
        master.SocketDisconnected += (_, _) => Log("disconnected");
        master.MessageReceived += (_, e) => Log($"message id={e.MessageID} slave={e.Slave} command={e.Command} response={e.Message}");
        master.TimeoutReceived += (_, e) => Log($"timeout id={e.MessageID} slave={e.Slave} command={e.Command}");
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"SampleMaster text {master.RemoteIP}:{master.RemotePort}");
        Console.WriteLine("commands: start, stop, restart, status, send <slave> <cmd> <message>, auto <id> <slave> <cmd> <message>, clear-auto, quit");

        while (true)
        {
            Console.Write("master-text> ");
            var line = await Console.In.ReadLineAsync();
            if (line == null) break;

            var tokens = Parsing.Split(line);
            if (tokens.Length == 0) continue;

            try
            {
                switch (tokens[0].ToLowerInvariant())
                {
                    case "start":
                        master.Start();
                        Log("started");
                        break;
                    case "stop":
                        master.Stop();
                        Log("stopped");
                        break;
                    case "restart":
                        master.Stop();
                        master.Start();
                        Log("restarted");
                        break;
                    case "status":
                        Console.WriteLine($"IsStart={master.IsStart}, IsOpen={master.IsOpen}, AutoReconnect={master.AutoReconnect}, Remote={master.RemoteIP}:{master.RemotePort}");
                        break;
                    case "send":
                        master.ManualSend(messageId++, (byte)Parsing.Int(tokens, 1), (byte)Parsing.Int(tokens, 2), string.Join(' ', tokens.Skip(3)));
                        break;
                    case "auto":
                        master.AutoSend(Parsing.Int(tokens, 1), (byte)Parsing.Int(tokens, 2), (byte)Parsing.Int(tokens, 3), string.Join(' ', tokens.Skip(4)));
                        break;
                    case "clear-auto":
                        master.ClearAuto();
                        break;
                    case "quit":
                    case "exit":
                        master.Stop();
                        return;
                    default:
                        Console.WriteLine("unknown command");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
            }
        }

        master.Stop();
    }

    private static void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
}

static class ModbusSelfTest
{
    public static async Task RunAsync(int port)
    {
        var bits = new bool[65536];
        var words = new int[65536];
        var slave = new ModbusTCPSlave { LocalPort = port };
        slave.BitReadRequest += (_, e) => ModbusTCPSlave.ProcessBitReads(e, 0, bits);
        slave.WordReadRequest += (_, e) => ModbusTCPSlave.ProcessWordReads(e, 0, words);
        slave.BitWriteRequest += (_, e) => ModbusTCPSlave.ProcessBitWrite(e, 0, bits);
        slave.WordWriteRequest += (_, e) => ModbusTCPSlave.ProcessWordWrite(e, 0, words);
        slave.MultiBitWriteRequest += (_, e) => ModbusTCPSlave.ProcessMultiBitWrite(e, 0, bits);
        slave.MultiWordWriteRequest += (_, e) => ModbusTCPSlave.ProcessMultiWordWrite(e, 0, words);
        slave.WordBitSetRequest += (_, e) => ModbusTCPSlave.ProcessWordBitSet(e, 0, words);

        var connected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var readBack = new TaskCompletionSource<int[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var master = new ModbusTCPMaster
        {
            RemoteIP = "127.0.0.1",
            RemotePort = port,
            Timeout = 300,
            Interval = 50,
            AutoReconnect = true,
        };
        ModbusMasterConsole.WireEvents(master);
        master.SocketConnected += (_, _) => connected.TrySetResult();
        master.WordReadReceived += (_, e) =>
        {
            if (e.MessageID == 3) readBack.TrySetResult(e.ReceiveData);
        };

        try
        {
            master.Start();
            await Task.Delay(300);
            slave.Start();
            await connected.Task.WaitAsync(TimeSpan.FromSeconds(3));

            master.ManualWordWrite_FC6(1, 1, 10, 1234);
            master.ManualBitWrite_FC5(2, 1, 20, true);
            master.ManualWordRead_FC3(3, 1, 10, 1);

            var values = await readBack.Task.WaitAsync(TimeSpan.FromSeconds(3));
            if (values.Length != 1 || values[0] != 1234)
                throw new InvalidOperationException("read-back mismatch");

            master.Stop();
            slave.Stop();
            master.Start();
            slave.Start();
            await Task.Delay(500);
            master.Stop();
            slave.Stop();

            Console.WriteLine("self-test passed");
        }
        finally
        {
            master.Stop();
            slave.Stop();
        }
    }
}

static class TextSelfTest
{
    public static async Task RunAsync(int port)
    {
        var slave = new TextCommTCPSlave { LocalPort = port };
        slave.MessageRequest += (_, e) => e.ResponseMessage = $"OK:{e.RequestMessage}";

        var connected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var master = new TextCommTCPMaster
        {
            RemoteIP = "127.0.0.1",
            RemotePort = port,
            Timeout = 300,
            Interval = 50,
            AutoReconnect = true,
        };
        master.SocketConnected += (_, _) =>
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] connected");
            connected.TrySetResult();
        };
        master.SocketDisconnected += (_, _) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] disconnected");
        master.MessageReceived += (_, e) =>
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] message id={e.MessageID} response={e.Message}");
            received.TrySetResult(e.Message);
        };

        try
        {
            master.Start();
            await Task.Delay(300);
            slave.Start();
            await connected.Task.WaitAsync(TimeSpan.FromSeconds(3));

            master.ManualSend(1, 1, 1, "ping");
            var response = await received.Task.WaitAsync(TimeSpan.FromSeconds(3));
            if (response != "OK:ping")
                throw new InvalidOperationException("text read-back mismatch");

            master.Stop();
            slave.Stop();
            master.Start();
            slave.Start();
            await Task.Delay(500);
            master.Stop();
            slave.Stop();

            Console.WriteLine("text self-test passed");
        }
        finally
        {
            master.Stop();
            slave.Stop();
        }
    }
}

static class ExternalSlaveSelfTest
{
    public static async Task RunAsync(CliOptions options)
    {
        var slaveProject = Path.GetFullPath(options.SlaveProject);
        var slaveAssembly = GetSlaveAssemblyPath(slaveProject);
        var protocolArgs = options.Protocol.Equals("text", StringComparison.OrdinalIgnoreCase)
            ? "--protocol text"
            : "--protocol modbus";

        using var slave = StartSlave(slaveAssembly, protocolArgs, options.Port);
        currentSlaveProcess = slave;
        _ = Task.Run(async () => await PumpOutputAsync(slave, "slave"));

        try
        {
            await SendSlaveCommandAsync(slave, "start");
            await Task.Delay(1500);

            if (options.Protocol.Equals("text", StringComparison.OrdinalIgnoreCase))
                await RunTextAgainstExternalSlaveAsync(options.Port);
            else
                await RunModbusAgainstExternalSlaveAsync(options.Port);

            await SendSlaveCommandAsync(slave, "quit");
            if (!slave.WaitForExit(5000))
                throw new InvalidOperationException("SampleSlave did not exit after quit command");

            Console.WriteLine("external-slave self-test passed");
        }
        finally
        {
            if (!slave.HasExited)
            {
                try { await SendSlaveCommandAsync(slave, "quit"); }
                catch { }

                if (!slave.WaitForExit(2000))
                    slave.Kill(entireProcessTree: true);
            }

            currentSlaveProcess = null;
        }
    }

    private static string GetSlaveAssemblyPath(string slaveProject)
    {
        var projectDirectory = Path.GetDirectoryName(slaveProject)
            ?? throw new InvalidOperationException("invalid SampleSlave project path");
        var assemblyPath = Path.Combine(projectDirectory, "bin", "Debug", "net9.0", "SampleSlave.dll");

        if (!File.Exists(assemblyPath))
            throw new FileNotFoundException("SampleSlave must be built before external self-test", assemblyPath);

        return assemblyPath;
    }

    private static Process StartSlave(string slaveAssembly, string protocolArgs, int port)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{slaveAssembly}\" {protocolArgs} --port {port}",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };

        if (!process.Start())
            throw new InvalidOperationException("failed to start SampleSlave");

        _ = Task.Run(async () => await PumpErrorAsync(process, "slave"));
        return process;
    }

    private static async Task RunModbusAgainstExternalSlaveAsync(int port)
    {
        var readMessageId = 0;
        TaskCompletionSource<int[]>? readBack = null;
        var bitMessageId = 0;
        TaskCompletionSource<bool[]>? bitBack = null;
        var master = new ModbusTCPMaster
        {
            RemoteIP = "127.0.0.1",
            RemotePort = port,
            Timeout = 300,
            Interval = 50,
            AutoReconnect = true,
        };
        ModbusMasterConsole.WireEvents(master);
        master.WordReadReceived += (_, e) =>
        {
            if (e.MessageID == readMessageId)
                readBack?.TrySetResult(e.ReceiveData);
        };
        master.BitReadReceived += (_, e) =>
        {
            if (e.MessageID == bitMessageId)
                bitBack?.TrySetResult(e.ReceiveData);
        };

        try
        {
            await SendSlaveCommandAsync(CurrentSlaveProcess(), "setbit 30 1");
            master.Start();
            await WaitUntilAsync(() => master.IsOpen, "master connect");
            await ExchangeModbusAsync(master, 100, 10, 4321, () =>
            {
                readMessageId = 103;
                readBack = new TaskCompletionSource<int[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                return readBack;
            });

            master.Stop();
            await Task.Delay(300);
            master.Start();
            await WaitUntilAsync(() => master.IsOpen, "master reconnect after master restart");
            await ExchangeModbusAsync(master, 200, 11, 4322, () =>
            {
                readMessageId = 203;
                readBack = new TaskCompletionSource<int[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                return readBack;
            });

            master.AutoWordRead_FC3(900, 1, 12, 1);
            await SendSlaveCommandAsync(CurrentSlaveProcess(), "stop");
            await WaitUntilAsync(() => !master.IsOpen, "master disconnect after slave stop");
            await SendSlaveCommandAsync(CurrentSlaveProcess(), "start");
            await WaitUntilAsync(() => master.IsOpen, "master reconnect after slave restart");
            master.ClearAuto();
            await SendSlaveCommandAsync(CurrentSlaveProcess(), "setbit 30 1");
            await ExchangeAutoBitAsync(master, 901, 30, () =>
            {
                bitMessageId = 901;
                bitBack = new TaskCompletionSource<bool[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                return bitBack;
            });
            await ExchangeModbusAsync(master, 300, 12, 4323, () =>
            {
                readMessageId = 303;
                readBack = new TaskCompletionSource<int[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                return readBack;
            });
        }
        finally
        {
            master.Stop();
        }
    }

    private static async Task RunTextAgainstExternalSlaveAsync(int port)
    {
        var receivedMessageId = 0;
        TaskCompletionSource<string>? received = null;
        var master = new TextCommTCPMaster
        {
            RemoteIP = "127.0.0.1",
            RemotePort = port,
            Timeout = 300,
            Interval = 50,
            AutoReconnect = true,
        };
        master.MessageReceived += (_, e) =>
        {
            if (e.MessageID == receivedMessageId)
                received?.TrySetResult(e.Message);
        };

        try
        {
            master.Start();
            await WaitUntilAsync(() => master.IsOpen, "text master connect");
            await ExchangeTextAsync(master, 101, "ping-1", () =>
            {
                receivedMessageId = 101;
                received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                return received;
            });

            master.Stop();
            await Task.Delay(300);
            master.Start();
            await WaitUntilAsync(() => master.IsOpen, "text master reconnect after master restart");
            await ExchangeTextAsync(master, 201, "ping-2", () =>
            {
                receivedMessageId = 201;
                received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                return received;
            });

            master.AutoSend(900, 1, 1, "poll");
            await SendSlaveCommandAsync(CurrentSlaveProcess(), "stop");
            await WaitUntilAsync(() => !master.IsOpen, "text master disconnect after slave stop");
            await SendSlaveCommandAsync(CurrentSlaveProcess(), "start");
            await WaitUntilAsync(() => master.IsOpen, "text master reconnect after slave restart");
            master.ClearAuto();
            await ExchangeTextAsync(master, 301, "ping-3", () =>
            {
                receivedMessageId = 301;
                received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                return received;
            });
        }
        finally
        {
            master.Stop();
        }
    }

    private static Process? currentSlaveProcess;

    private static Process CurrentSlaveProcess()
    {
        return currentSlaveProcess ?? throw new InvalidOperationException("SampleSlave process is not available");
    }

    private static async Task ExchangeModbusAsync(
        ModbusTCPMaster master,
        int idBase,
        int address,
        int value,
        Func<TaskCompletionSource<int[]>> createReadBack)
    {
        var readBack = createReadBack();
        master.ManualWordWrite_FC6(idBase + 1, 1, address, value);
        master.ManualBitWrite_FC5(idBase + 2, 1, address + 100, true);
        master.ManualWordRead_FC3(idBase + 3, 1, address, 1);

        var values = await readBack.Task.WaitAsync(TimeSpan.FromSeconds(5));
        if (values.Length != 1 || values[0] != value)
            throw new InvalidOperationException($"external modbus read-back mismatch. expected={value}, actual={string.Join(',', values)}");
    }

    private static async Task ExchangeTextAsync(
        TextCommTCPMaster master,
        int messageId,
        string message,
        Func<TaskCompletionSource<string>> createReceived)
    {
        var received = createReceived();
        master.ManualSend(messageId, 1, 1, message);

        var response = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        if (response != $"OK:{message}")
            throw new InvalidOperationException($"external text read-back mismatch. expected=OK:{message}, actual={response}");
    }

    private static async Task ExchangeAutoBitAsync(
        ModbusTCPMaster master,
        int messageId,
        int address,
        Func<TaskCompletionSource<bool[]>> createBitBack)
    {
        var bitBack = createBitBack();
        master.AutoBitRead_FC1(messageId, 1, address, 1);

        var values = await bitBack.Task.WaitAsync(TimeSpan.FromSeconds(5));
        if (values.Length != 1 || !values[0])
            throw new InvalidOperationException($"external modbus auto-bit read-back mismatch. actual={string.Join(',', values.Select(v => v ? 1 : 0))}");

        master.RemoveAuto(messageId);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, string description, int timeoutMilliseconds = 7000)
    {
        var startedAt = Environment.TickCount64;
        while (Environment.TickCount64 - startedAt < timeoutMilliseconds)
        {
            if (condition()) return;
            await Task.Delay(50);
        }

        throw new TimeoutException($"timed out waiting for {description}");
    }

    private static async Task SendSlaveCommandAsync(Process process, string command)
    {
        await process.StandardInput.WriteLineAsync(command);
        await process.StandardInput.FlushAsync();
    }

    private static async Task PumpOutputAsync(Process process, string prefix)
    {
        while (!process.StandardOutput.EndOfStream)
        {
            var line = await process.StandardOutput.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
                Console.WriteLine($"[{prefix}] {line}");
        }
    }

    private static async Task PumpErrorAsync(Process process, string prefix)
    {
        while (!process.StandardError.EndOfStream)
        {
            var line = await process.StandardError.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
                Console.WriteLine($"[{prefix}:err] {line}");
        }
    }
}

sealed record CliOptions(string Protocol, string Host, int Port, bool SelfTest, bool ExternalSlave, string SlaveProject)
{
    public static CliOptions Parse(string[] args, int defaultPort)
    {
        var protocol = "modbus";
        var host = "127.0.0.1";
        var port = defaultPort;
        var selfTest = false;
        var externalSlave = false;
        var slaveProject = Path.Combine("SampleSlave", "SampleSlave.csproj");

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--protocol" when i + 1 < args.Length:
                    protocol = args[++i];
                    break;
                case "--host" when i + 1 < args.Length:
                    host = args[++i];
                    break;
                case "--port" when i + 1 < args.Length:
                    port = Parsing.Number(args[++i]);
                    break;
                case "--self-test":
                    selfTest = true;
                    break;
                case "--external-slave":
                    externalSlave = true;
                    break;
                case "--slave-project" when i + 1 < args.Length:
                    slaveProject = args[++i];
                    break;
            }
        }

        return new CliOptions(protocol, host, port, selfTest, externalSlave, slaveProject);
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

    public static bool[] BoolArray(string[] tokens, int index)
    {
        if (index >= tokens.Length) throw new ArgumentException("missing argument");
        return tokens[index].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(v => v is "1" or "true" or "on").ToArray();
    }

    public static int[] IntArray(string[] tokens, int index)
    {
        if (index >= tokens.Length) throw new ArgumentException("missing argument");
        return tokens[index].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(Number).ToArray();
    }

    public static int Number(string value)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.Parse(value[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return int.Parse(value, CultureInfo.InvariantCulture);
    }
}
