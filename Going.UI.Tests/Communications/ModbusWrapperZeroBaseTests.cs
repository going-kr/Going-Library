using System.Reflection;
using Going.Basis.Communications.Modbus.RTU;
using Going.Basis.Memories;
using TcpMaster = Going.Basis.Communications.Modbus.TCP.MasterTCP;
using TcpSlave = Going.Basis.Communications.Modbus.TCP.SlaveTCP;
using TcpLowMaster = Going.Basis.Communications.Modbus.TCP.ModbusTCPMaster;
using TcpLowSlave = Going.Basis.Communications.Modbus.TCP.ModbusTCPSlave;
using Xunit;

namespace Going.UI.Tests.Communications;

public class ModbusWrapperZeroBaseTests
{
    [Fact]
    public void MasterRTU_StoresReadValuesByFunctionCodeAndZeroBasedAddress()
    {
        var master = new MasterRTU();

        InvokePrivate(master, "Modbus_BitReadReceived",
            new ModbusRTUMaster.BitReadEventArgs(new ModbusRTUMaster.Work(0, RtuWork(1, 1, 0, 2), 0), [true, false]));
        InvokePrivate(master, "Modbus_BitReadReceived",
            new ModbusRTUMaster.BitReadEventArgs(new ModbusRTUMaster.Work(0, RtuWork(1, 2, 0, 2), 0), [false, true]));
        InvokePrivate(master, "Modbus_WordReadReceived",
            new ModbusRTUMaster.WordReadEventArgs(new ModbusRTUMaster.Work(0, RtuWork(1, 3, 0, 2), 0), [123, 456]));
        InvokePrivate(master, "Modbus_WordReadReceived",
            new ModbusRTUMaster.WordReadEventArgs(new ModbusRTUMaster.Work(0, RtuWork(1, 4, 0, 2), 0), [789, 321]));

        Assert.True(master.GetCoil(1, 0));
        Assert.False(master.GetCoil(1, 1));
        Assert.False(master.GetContact(1, 0));
        Assert.True(master.GetContact(1, 1));
        Assert.Equal(123, master.GetHoldingRegister(1, 0));
        Assert.Equal(456, master.GetHoldingRegister(1, 1));
        Assert.Equal(789, master.GetInputRegister(1, 0));
        Assert.Equal(321, master.GetInputRegister(1, 1));
    }

    [Fact]
    public void MasterTCP_StoresReadValuesByFunctionCodeAndZeroBasedAddress()
    {
        var master = new TcpMaster();

        InvokePrivate(master, "Modbus_BitReadReceived",
            new TcpLowMaster.BitReadEventArgs(new TcpLowMaster.Work(0, TcpWork(1, 1, 0, 2), 0), [true, false]));
        InvokePrivate(master, "Modbus_BitReadReceived",
            new TcpLowMaster.BitReadEventArgs(new TcpLowMaster.Work(0, TcpWork(1, 2, 0, 2), 0), [false, true]));
        InvokePrivate(master, "Modbus_WordReadReceived",
            new TcpLowMaster.WordReadEventArgs(new TcpLowMaster.Work(0, TcpWork(1, 3, 0, 2), 0), [123, 456]));
        InvokePrivate(master, "Modbus_WordReadReceived",
            new TcpLowMaster.WordReadEventArgs(new TcpLowMaster.Work(0, TcpWork(1, 4, 0, 2), 0), [789, 321]));

        Assert.True(master.GetCoil(1, 0));
        Assert.False(master.GetCoil(1, 1));
        Assert.False(master.GetContact(1, 0));
        Assert.True(master.GetContact(1, 1));
        Assert.Equal(123, master.GetHoldingRegister(1, 0));
        Assert.Equal(456, master.GetHoldingRegister(1, 1));
        Assert.Equal(789, master.GetInputRegister(1, 0));
        Assert.Equal(321, master.GetInputRegister(1, 1));
    }

    [Fact]
    public void MasterRTU_UpdatesWritableCacheWhenWriteResponsesAreReceived()
    {
        var master = new MasterRTU();

        master.WriteCoil_FC5(1, 0, true);
        master.WriteCoils_FC15(1, 1, [false, true]);
        master.WriteHoldingRegister_FC6(1, 0, 123);
        master.WriteHoldingRegisters_FC16(1, 1, [456, 789]);

        Assert.Null(master.GetCoil(1, 0));
        Assert.Null(master.GetHoldingRegister(1, 0));

        InvokePrivate(master, "Modbus_BitWriteReceived",
            new ModbusRTUMaster.BitWriteEventArgs(new ModbusRTUMaster.Work(0, RtuWrite(1, 5, 0, 0xFF00), 0)));
        InvokePrivate(master, "Modbus_MultiBitWriteReceived",
            new ModbusRTUMaster.MultiBitWriteEventArgs(new ModbusRTUMaster.Work(0, RtuMultiBitWrite(1, 1, [false, true]), 0)));
        InvokePrivate(master, "Modbus_WordWriteReceived",
            new ModbusRTUMaster.WordWriteEventArgs(new ModbusRTUMaster.Work(0, RtuWrite(1, 6, 0, 123), 0)));
        InvokePrivate(master, "Modbus_MultiWordWriteReceived",
            new ModbusRTUMaster.MultiWordWriteEventArgs(new ModbusRTUMaster.Work(0, RtuMultiWordWrite(1, 1, [456, 789]), 0)));

        Assert.True(master.GetCoil(1, 0));
        Assert.False(master.GetCoil(1, 1));
        Assert.True(master.GetCoil(1, 2));
        Assert.Equal(123, master.GetHoldingRegister(1, 0));
        Assert.Equal(456, master.GetHoldingRegister(1, 1));
        Assert.Equal(789, master.GetHoldingRegister(1, 2));
    }

    [Fact]
    public void MasterTCP_UpdatesWritableCacheWhenWriteResponsesAreReceived()
    {
        var master = new TcpMaster();

        master.WriteCoil_FC5(1, 0, true);
        master.WriteCoils_FC15(1, 1, [false, true]);
        master.WriteHoldingRegister_FC6(1, 0, 123);
        master.WriteHoldingRegisters_FC16(1, 1, [456, 789]);

        Assert.Null(master.GetCoil(1, 0));
        Assert.Null(master.GetHoldingRegister(1, 0));

        InvokePrivate(master, "Modbus_BitWriteReceived",
            new TcpLowMaster.BitWriteEventArgs(new TcpLowMaster.Work(0, TcpWrite(1, 5, 0, 0xFF00), 0)));
        InvokePrivate(master, "Modbus_MultiBitWriteReceived",
            new TcpLowMaster.MultiBitWriteEventArgs(new TcpLowMaster.Work(0, TcpMultiBitWrite(1, 1, [false, true]), 0)));
        InvokePrivate(master, "Modbus_WordWriteReceived",
            new TcpLowMaster.WordWriteEventArgs(new TcpLowMaster.Work(0, TcpWrite(1, 6, 0, 123), 0)));
        InvokePrivate(master, "Modbus_MultiWordWriteReceived",
            new TcpLowMaster.MultiWordWriteEventArgs(new TcpLowMaster.Work(0, TcpMultiWordWrite(1, 1, [456, 789]), 0)));

        Assert.True(master.GetCoil(1, 0));
        Assert.False(master.GetCoil(1, 1));
        Assert.True(master.GetCoil(1, 2));
        Assert.Equal(123, master.GetHoldingRegister(1, 0));
        Assert.Equal(456, master.GetHoldingRegister(1, 1));
        Assert.Equal(789, master.GetHoldingRegister(1, 2));
    }

    [Fact]
    public void SlaveRTU_UsesDirectZeroBasedMemories()
    {
        var slave = new SlaveRTU { Slave = 1, Coils = new BitMemory(2), HoldingRegister = new WordMemory(2) };
        slave.Coils[0] = true;
        slave.HoldingRegister[0].W = 123;

        var bitRead = new ModbusRTUSlave.BitReadRequestArgs(RtuWork(1, 1, 0, 1));
        var wordRead = new ModbusRTUSlave.WordReadRequestArgs(RtuWork(1, 3, 0, 1));
        var bitWrite = new ModbusRTUSlave.BitWriteRequestArgs(RtuWrite(1, 5, 1, 0xFF00));

        InvokePrivate(slave, "Modbus_BitReadRequest", bitRead);
        InvokePrivate(slave, "Modbus_WordReadRequest", wordRead);
        InvokePrivate(slave, "Modbus_BitWriteRequest", bitWrite);

        Assert.True(bitRead.Success);
        Assert.Equal([true], bitRead.ResponseData);
        Assert.True(wordRead.Success);
        Assert.Equal([123], wordRead.ResponseData);
        Assert.True(bitWrite.Success);
        Assert.True(slave.Coils[1]);
    }

    [Fact]
    public void SlaveTCP_UsesDirectZeroBasedMemories()
    {
        var slave = new TcpSlave { Slave = 1, Coils = new BitMemory(2), HoldingRegister = new WordMemory(2) };
        slave.Coils[0] = true;
        slave.HoldingRegister[0].W = 123;

        var bitRead = new TcpLowSlave.BitReadRequestArgs(TcpWork(1, 1, 0, 1));
        var wordRead = new TcpLowSlave.WordReadRequestArgs(TcpWork(1, 3, 0, 1));
        var bitWrite = new TcpLowSlave.BitWriteRequestArgs(TcpWrite(1, 5, 1, 0xFF00));

        InvokePrivate(slave, "Modbus_BitReadRequest", bitRead);
        InvokePrivate(slave, "Modbus_WordReadRequest", wordRead);
        InvokePrivate(slave, "Modbus_BitWriteRequest", bitWrite);

        Assert.True(bitRead.Success);
        Assert.Equal([true], bitRead.ResponseData);
        Assert.True(wordRead.Success);
        Assert.Equal([123], wordRead.ResponseData);
        Assert.True(bitWrite.Success);
        Assert.True(slave.Coils[1]);
    }

    private static void InvokePrivate(object target, string methodName, EventArgs args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(target, [null, args]);
    }

    private static byte[] RtuWork(int slave, int fn, int address, int length) =>
    [
        (byte)slave, (byte)fn, Hi(address), Lo(address), Hi(length), Lo(length), 0, 0
    ];

    private static byte[] RtuWrite(int slave, int fn, int address, int value) =>
    [
        (byte)slave, (byte)fn, Hi(address), Lo(address), Hi(value), Lo(value), 0, 0
    ];

    private static byte[] RtuMultiBitWrite(int slave, int address, bool[] values) =>
    [
        (byte)slave, 15, Hi(address), Lo(address), Hi(values.Length), Lo(values.Length), 1, PackBits(values), 0, 0
    ];

    private static byte[] RtuMultiWordWrite(int slave, int address, int[] values) =>
    [
        (byte)slave, 16, Hi(address), Lo(address), Hi(values.Length), Lo(values.Length), (byte)(values.Length * 2),
        Hi(values[0]), Lo(values[0]), Hi(values[1]), Lo(values[1]), 0, 0
    ];

    private static byte[] TcpWork(int slave, int fn, int address, int length) =>
    [
        0, 0, 0, 0, 0, 6, (byte)slave, (byte)fn, Hi(address), Lo(address), Hi(length), Lo(length)
    ];

    private static byte[] TcpWrite(int slave, int fn, int address, int value) =>
    [
        0, 0, 0, 0, 0, 6, (byte)slave, (byte)fn, Hi(address), Lo(address), Hi(value), Lo(value)
    ];

    private static byte[] TcpMultiBitWrite(int slave, int address, bool[] values) =>
    [
        0, 0, 0, 0, 0, 8, (byte)slave, 15, Hi(address), Lo(address), Hi(values.Length), Lo(values.Length), 1, PackBits(values)
    ];

    private static byte[] TcpMultiWordWrite(int slave, int address, int[] values) =>
    [
        0, 0, 0, 0, 0, 11, (byte)slave, 16, Hi(address), Lo(address), Hi(values.Length), Lo(values.Length), (byte)(values.Length * 2),
        Hi(values[0]), Lo(values[0]), Hi(values[1]), Lo(values[1])
    ];

    private static byte Hi(int value) => (byte)((value & 0xFF00) >> 8);
    private static byte Lo(int value) => (byte)(value & 0x00FF);

    private static byte PackBits(bool[] values)
    {
        byte ret = 0;
        for (int i = 0; i < values.Length; i++)
            if (values[i]) ret |= (byte)(1 << i);
        return ret;
    }
}
