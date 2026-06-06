using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Going.UI.Collections;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Xunit;

namespace Going.UI.Tests.Samples;

// ─────────────────────────────────────────────────────────────────────────────
// 이번 작업 3종(선언적 바인딩 · 컴포넌트 · GoItemList)을 한 디자인에 합친 샘플.
// 이 테스트는 "동작하는 사용 예제"이자 회귀 가드다.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>모터 1대 ViewModel — 속성만 갱신되고 객체 참조는 안정적(HMI 전형).</summary>
public sealed class MotorVM
{
    public string Name { get; set; } = "";
    public double Rpm { get; set; }
}

/// <summary>로그 한 줄.</summary>
public sealed class LogVM
{
    public string Message { get; set; } = "";
}

/// <summary>앱 허브 — WireBindings(root)로 주입하는 단일 컨텍스트 객체.</summary>
public sealed class AppHub
{
    public string PlantStatus { get; set; } = "RUN";
    public MotorVM PumpA { get; } = new() { Name = "펌프 A", Rpm = 1450 };
    public MotorVM PumpB { get; } = new() { Name = "펌프 B", Rpm = 980 };
    public ObservableList<LogVM> Logs { get; } = new();
}

[Collection("GudxComponentRegistry")]   // 컴포넌트 레지스트리 공유 — 병렬 race 방지
public class BindingShowcaseSample
{
    // 마크업 한 장에 3종이 다 들어있다.
    private const string Gudx = @"
<GoDesign>
  <Components>
    <!-- 재사용 컴포넌트: 파라미터 motor(MotorVM)를 받아 내부에서 바인딩 -->
    <GoComponent name='MotorCard'>
      <Param name='motor' type='MotorVM'/>
      <GoBoxPanel Bounds='0,0,200,60'><Childrens>
        <GoLabel Text='{motor.Name}'    Bounds='4,4,192,24'/>
        <GoLabel Text='{motor.Rpm:F0}'  Bounds='4,30,192,24'/>
      </Childrens></GoBoxPanel>
    </GoComponent>
  </Components>

  <Pages>
    <GoPage Name='Main'><Childrens>
      <!-- 1) 선언적 바인딩: 허브 멤버에 직접 -->
      <GoLabel Text='{PlantStatus}' Bounds='0,0,200,30'/>

      <!-- 2) 컴포넌트: 인스턴스마다 다른 모터 -->
      <MotorCard motor='{PumpA}' Bounds='0,40,200,60'/>
      <MotorCard motor='{PumpB}' Bounds='0,110,200,60'/>

      <!-- 3) GoItemList: 컬렉션을 ItemTemplate으로 반복 -->
      <GoItemList Items='{Logs}' ItemHeight='24' Bounds='0,180,300,200'>
        <ItemTemplate>
          <GoLabel Text='{Message}'/>
        </ItemTemplate>
      </GoItemList>
    </Childrens></GoPage>
  </Pages>
</GoDesign>";

    [Fact]
    public void Showcase_AllThreeFeatures_FlowFromHub()
    {
        GoComponentTemplate.Registry.Clear();

        // ── 로드 + 주입 (앱 부트스트랩과 동일한 3줄) ──
        var hub = new AppHub();
        var design = GoGudxConverter.ReadGoDesign(XElement.Parse(Gudx))!;
        design.Init();
        design.WireBindings(hub);

        var page = design.Pages["Main"];
        hub.Logs.Add(new LogVM { Message = "부팅 완료" });
        hub.Logs.Add(new LogVM { Message = "펌프 A 기동" });

        // 두 번 펌프: 1틱에 바인딩/ItemsSource 전달 + 행 생성, 2틱에 새 행까지 값 반영
        page.FireUpdate();
        page.FireUpdate();

        // ── 1) 선언적 바인딩: PlantStatus ──
        var status = (GoLabel)page.Childrens[0];
        Assert.Equal("RUN", status.Text);

        // ── 2) 컴포넌트: 두 카드가 각자의 모터를 표시 ──
        var cardA = (GoComponentInstance)page.Childrens[1];
        var cardB = (GoComponentInstance)page.Childrens[2];
        var (nameA, rpmA) = ReadCard(cardA);
        var (nameB, rpmB) = ReadCard(cardB);
        Assert.Equal("펌프 A", nameA);
        Assert.Equal("1450", rpmA);
        Assert.Equal("펌프 B", nameB);
        Assert.Equal("980", rpmB);

        // ── 3) GoItemList: 로그 2줄이 행 2개로 ──
        var list = (GoItemList)page.Childrens[3];
        Assert.Equal(2, list.Childrens.Count);
        Assert.Equal("부팅 완료", ((GoLabel)list.Childrens[0]).Text);
        Assert.Equal("펌프 A 기동", ((GoLabel)list.Childrens[1]).Text);

        // ── live: 소스만 바꿔도 다음 틱에 반영 ──
        hub.PumpA.Rpm = 1600;
        hub.Logs.Add(new LogVM { Message = "경고: 과속" });
        page.FireUpdate();
        page.FireUpdate();

        Assert.Equal("1600", ReadCard(cardA).rpm);     // 컴포넌트 내부 바인딩 live
        Assert.Equal(3, list.Childrens.Count);          // ItemList 행 추가
        Assert.Equal("경고: 과속", ((GoLabel)list.Childrens[2]).Text);
    }

    private static (string name, string rpm) ReadCard(GoComponentInstance card)
    {
        var box = (GoBoxPanel)card.Childrens[0];
        var labels = box.Childrens.OfType<GoLabel>().ToList();
        return (labels[0].Text, labels[1].Text);
    }
}
