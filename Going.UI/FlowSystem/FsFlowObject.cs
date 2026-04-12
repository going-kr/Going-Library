using Going.UI.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.FlowSystem
{
    #region enum : PortDirection
    /// <summary>
    /// 포트의 방향을 나타내는 열거형
    /// </summary>
    public enum PortDirection
    {
        /// <summary>왼쪽</summary>
        L,
        /// <summary>오른쪽</summary>
        R,
        /// <summary>위쪽</summary>
        T,
        /// <summary>아래쪽</summary>
        B
    }
    #endregion

    #region enum : FlowState
    /// <summary>
    /// 흐름 상태를 나타내는 열거형
    /// </summary>
    public enum FlowState
    {
        /// <summary>정지</summary>
        Stop,
        /// <summary>흐름</summary>
        Flow
    }
    #endregion

    #region enum : PortType
    /// <summary>
    /// 포트의 타입을 나타내는 열거형
    /// </summary>
    public enum PortType
    {
        /// <summary>입력</summary>
        Input,
        /// <summary>출력</summary>
        Output,
        /// <summary>양방향</summary>
        Bidirectional
    }
    #endregion

    #region enum : PortFlow
    /// <summary>
    /// 포트의 흐름 방향을 나타내는 열거형
    /// </summary>
    public enum PortFlow
    {
        /// <summary>없음</summary>
        None,
        /// <summary>입력</summary>
        Input,
        /// <summary>출력</summary>
        Output
    }
    #endregion

    #region enum : ObjectRotate
    /// <summary>
    /// 객체의 회전 각도를 나타내는 열거형
    /// </summary>
    public enum ObjectRotate
    {
        /// <summary>0도</summary>
        Deg0,
        /// <summary>90도</summary>
        Deg90,
        /// <summary>180도</summary>
        Deg180,
        /// <summary>270도</summary>
        Deg270
    }
    #endregion

    #region interface : IRotatable
    /// <summary>
    /// 회전 가능한 객체를 나타내는 인터페이스
    /// </summary>
    public interface IRotatable
    {
        /// <summary>
        /// 객체의 회전 각도를 가져오거나 설정합니다.
        /// </summary>
        ObjectRotate Rotate { get; set; }
    }
    #endregion

    #region class : ConnectPort
    /// <summary>
    /// 플로우 객체의 연결 포트를 나타내는 클래스
    /// </summary>
    public class ConnectPort
    {
        /// <summary>
        /// 포트의 이름을 가져오거나 설정합니다.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 포트의 위치를 가져오거나 설정합니다.
        /// </summary>
        public SKPoint Position { get; set; }

        /// <summary>
        /// 포트의 타입을 가져오거나 설정합니다.
        /// </summary>
        public PortType Type { get; set; }

        /// <summary>
        /// 포트의 방향을 가져오거나 설정합니다.
        /// </summary>
        public PortDirection Direction { get; set; }

        /// <summary>
        /// 이 포트가 속한 플로우 객체를 가져오거나 설정합니다.
        /// </summary>
        [JsonIgnore]
        internal FsFlowObject? Control { get; set; }

        /// <summary>
        /// 포트의 흐름 상태를 가져오거나 설정합니다.
        /// </summary>
        [JsonIgnore]
        internal PortFlow State { get; set; } = PortFlow.None;
    }
    #endregion

    #region class : PipeNode
    /// <summary>
    /// 파이프 노드를 나타내는 클래스
    /// </summary>
    public class PipeNode
    {
        /// <summary>
        /// 파이프 노드의 방향을 가져오거나 설정합니다.
        /// </summary>
        public PortDirection Direction { get; set; }

        /// <summary>
        /// 파이프 노드의 위치를 가져오거나 설정합니다.
        /// </summary>
        public float Position { get; set; }
    }
    #endregion

    #region class : FsFlowObject
    /// <summary>
    /// 흐름 시스템의 모든 컴포넌트(펌프, 밸브, 배관, 탱크 등)의 추상 기반 클래스.
    /// <para>FsFlowSystemPanel의 자식으로 배치되며, ConnectPort를 통해 다른 컴포넌트와 배관으로 연결된다.</para>
    /// <para>각 파생 클래스는 IoPorts()로 입출력 포트를 정의하고, IsFlow로 현재 흐름 상태를 반환해야 한다.</para>
    ///
    /// <para><b>주요 사용 시나리오:</b></para>
    /// <list type="bullet">
    ///   <item>배관 네트워크에서 펌프, 밸브, 탱크, 파이프 등의 컴포넌트를 구현할 때 이 클래스를 상속한다.</item>
    ///   <item>FsFlowSystemPanel.Childrens에 추가하여 배관 레이아웃을 구성한다.</item>
    ///   <item>FlowConnection을 통해 포트 간 배관 연결을 정의한다.</item>
    /// </list>
    ///
    /// <para><b>파생 클래스 구현 가이드:</b></para>
    /// <list type="number">
    ///   <item><see cref="IoPorts"/>: 컴포넌트의 모든 입출력 ConnectPort를 열거하여 반환한다.</item>
    ///   <item><see cref="GetPort"/>: 이름으로 특정 포트를 검색하여 반환한다.</item>
    ///   <item><see cref="IsFlow"/>: 현재 흐름이 통과 가능한 상태인지 반환한다 (예: 펌프 ON, 밸브 열림).</item>
    /// </list>
    ///
    /// <para><b>포트 구성 설명:</b></para>
    /// <para>각 ConnectPort는 Name(고유 이름), Position(좌표), Type(Input/Output/Bidirectional), Direction(L/R/T/B)을 가진다.
    /// FlowConnection에서 StartPortName/EndPortName으로 포트를 참조하여 컴포넌트 간 연결을 정의한다.</para>
    ///
    /// <para><b>회전 지원:</b></para>
    /// <para>IRotatable 인터페이스를 구현하는 파생 클래스(FsTeePipe, FsCrossPipe 등)는 Rotate 프로퍼티로 0/90/180/270도 회전이 가능하며,
    /// 회전 시 포트의 Direction과 Position이 자동으로 재배치된다.</para>
    /// </summary>
    /// <remarks>
    /// 사용 예:
    /// <code>
    /// var panel = new FsFlowSystemPanel();
    /// var pump = new FsPump { OnOff = true };
    /// var tank = new FsCylinderTank { UseOutlet = true };
    /// panel.Childrens.Add(pump);
    /// panel.Childrens.Add(tank);
    ///
    /// // 탱크 Outlet → 펌프 Inlet 연결
    /// panel.Connections.Add(new FlowConnection
    /// {
    ///     StartControlId = tank.Id,
    ///     StartPortName = "Outlet",
    ///     EndControlId = pump.Id,
    ///     EndPortName = "Inlet"
    /// });
    /// panel.RefreshCache();
    /// </code>
    /// </remarks>
    public abstract class FsFlowObject : GoControl
    {
        /// <summary>
        /// 이 객체의 모든 입출력 포트를 반환합니다.
        /// </summary>
        /// <returns>연결 포트의 열거 가능한 컬렉션</returns>
        public abstract IEnumerable<ConnectPort> IoPorts();

        /// <summary>
        /// 지정된 이름의 포트를 반환합니다.
        /// </summary>
        /// <param name="name">포트 이름</param>
        /// <returns>해당 이름의 포트, 없으면 null</returns>
        public abstract ConnectPort? GetPort(string? name);

        /// <summary>
        /// 이 객체가 흐름 객체인지 여부를 가져옵니다.
        /// </summary>
        public abstract bool IsFlow { get; }
    }
    #endregion
}
