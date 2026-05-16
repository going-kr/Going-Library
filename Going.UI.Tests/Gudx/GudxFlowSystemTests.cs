using Going.UI.Enums;
using Going.UI.FlowSystem;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Gudx;

public class GudxFlowSystemTests
{
    [Fact]
    public void FsFlowSystemPanel_roundTripsChildren()
    {
        var panel = new FsFlowSystemPanel { Name = "fs" };
        panel.Childrens.Add(new FsPump { Name = "pump1", OnOff = true, Rotate = ObjectRotate.Deg90 });
        panel.Childrens.Add(new FsValve { Name = "valve1", OnOff = false });
        panel.Childrens.Add(new FsCylinderTank { Name = "tank1", Value = 75, UseInlet = true });

        var xml = GoGudxConverter.SerializeControl(panel);
        var restored = (FsFlowSystemPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Equal(3, restored.Childrens.Count);
        Assert.IsType<FsPump>(restored.Childrens[0]);
        Assert.Equal("pump1", restored.Childrens[0].Name);
        Assert.True(((FsPump)restored.Childrens[0]).OnOff);
        Assert.Equal(ObjectRotate.Deg90, ((FsPump)restored.Childrens[0]).Rotate);
        Assert.IsType<FsValve>(restored.Childrens[1]);
        Assert.Equal("valve1", restored.Childrens[1].Name);
        Assert.IsType<FsCylinderTank>(restored.Childrens[2]);
        Assert.Equal(75d, ((FsCylinderTank)restored.Childrens[2]).Value);
        Assert.True(((FsCylinderTank)restored.Childrens[2]).UseInlet);
    }

    [Fact]
    public void FsFlowSystemPanel_roundTripsConnectionsWithNodes()
    {
        var panel = new FsFlowSystemPanel { Name = "fs" };
        var tank = new FsCylinderTank { Name = "tank", UseOutlet = true };
        var pump = new FsPump { Name = "pump", UseInlet = true };
        panel.Childrens.Add(tank);
        panel.Childrens.Add(pump);

        var conn = new FlowConnection
        {
            StartControlId = tank.Id,
            StartPortName = "Outlet",
            EndControlId = pump.Id,
            EndPortName = "Inlet"
        };
        conn.Nodes.Add(new PipeNode { Direction = PortDirection.R, Position = 400 });
        conn.Nodes.Add(new PipeNode { Direction = PortDirection.B, Position = 200 });
        var originalConnId = conn.Id;
        panel.Connections.Add(conn);

        var xml = GoGudxConverter.SerializeControl(panel);
        var restored = (FsFlowSystemPanel)GoGudxConverter.DeserializeControl(xml);

        Assert.Single(restored.Connections);
        var r = restored.Connections[0];
        Assert.Equal(originalConnId, r.Id);
        Assert.Equal(tank.Id, r.StartControlId);
        Assert.Equal("Outlet", r.StartPortName);
        Assert.Equal(pump.Id, r.EndControlId);
        Assert.Equal("Inlet", r.EndPortName);
        Assert.Equal(2, r.Nodes.Count);
        Assert.Equal(PortDirection.R, r.Nodes[0].Direction);
        Assert.Equal(400f, r.Nodes[0].Position);
        Assert.Equal(PortDirection.B, r.Nodes[1].Direction);
        Assert.Equal(200f, r.Nodes[1].Position);
    }
}
