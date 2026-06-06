namespace SampleBinding;

/// <summary>
/// 화면 정의(gudx). 평소엔 .gudx 파일로 두지만 데모라 인라인 상수로 둔다.
/// 세 기능이 한 장에 들어있다: ① 선언적 바인딩 ② 컴포넌트 ③ GoItemList.
///
/// 주의: Bounds 형식은 Left,Top,Right,Bottom (SKRect 원시값) 이다.
/// </summary>
internal static class Markup
{
    public const string Gudx = @"
<GoDesign>

  <!-- ② 재사용 컴포넌트 정의: motor(MotorVM) 하나를 받아 내부에서 바인딩 -->
  <Components>
    <GoComponent name='MotorCard'>
      <Param name='motor' type='MotorVM'/>
      <GoBoxPanel Bounds='0,0,340,64' BoxColor='Base2' BorderColor='Base3'><Childrens>
        <GoLabel Text='{motor.Name}'   Bounds='12,8,210,36'  FontSize='15'/>
        <GoLabel Text='{motor.Rpm:F0}' Bounds='210,8,330,56' FontSize='26' TextColor='Point'/>
      </Childrens></GoBoxPanel>
    </GoComponent>
  </Components>

  <Pages>
    <GoPage Name='Main' BackColor='Back'><Childrens>

      <!-- ① 선언적 바인딩: 허브 멤버에 직접 (L,T,R,B) -->
      <GoLabel Text='{PlantStatus}' Bounds='12,10,348,44' FontSize='20' TextColor='Point'/>

      <!-- ② 컴포넌트 인스턴스: 각각 다른 모터를 바인딩 -->
      <MotorCard motor='{PumpA}' Bounds='10,52,350,116'/>
      <MotorCard motor='{PumpB}' Bounds='10,124,350,188'/>

      <!-- 로그 헤더 -->
      <GoLabel Text='최근 로그' Bounds='12,200,348,222' FontSize='13' TextColor='Fore'/>

      <!-- ③ GoItemList: 컬렉션을 ItemTemplate으로 반복 -->
      <GoItemList Items='{Logs}' ItemHeight='26' Bounds='10,226,350,458'>
        <ItemTemplate>
          <GoBoxPanel Bounds='0,0,340,26' BoxColor='Base1' BorderColor='Base2'><Childrens>
            <GoLabel Text='{Message}' Bounds='10,3,330,24' FontSize='12'/>
          </Childrens></GoBoxPanel>
        </ItemTemplate>
      </GoItemList>

    </Childrens></GoPage>
  </Pages>

</GoDesign>";
}
