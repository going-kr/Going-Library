# GsShape — 벡터 셰이프 패밀리

**작성일**: 2026-06-06
**브랜치**: `hjh-gudx-binding`
**대상**: Going.UI (Controls + Utils/GUI)

---

## 목표

HMI 디자인 퀄리티를 올리는 벡터 셰이프 컨트롤 패밀리. 장식·커넥터·커스텀 게이지·배경·다이어그램에 사용.

```
GsRect · GsCircle · GsLine · GsArc · GsPolygon · GsBezier   (전부 GsShape 상속)
```

각 셰이프는 **기하(geometry)만** 정의하고, 공통 효과(Fill/Stroke/Shadow/Glow/Rotate/Clip)는 **GsShape 베이스**가 처리한다.

## 비목표 (1차)

- 애니메이션/모션, 패스 변형(morph)
- 텍스트-온-패스, 이미지 fill(패턴)
- 다중 그라데이션 stop(2색만), conic 그라데이션(Arc 제외)
- 히트테스트 정밀화(셰이프 외곽 클릭 판정은 bounds 기준 유지)

---

## 결정 사항

### D1. 점 좌표 = 정규화 0~1, `List<string>`("x,y")로 직렬화
Line/Polygon/Bezier의 점은 bounds에 대한 **정규화 좌표**(0~1). 저장은 기존 gudx가 지원하는 `List<string>`(각 항목 `"x,y"`)로 — **새 직렬화 인프라 불필요**. 그리기 시 `pt × bounds`로 환산. 크기 바뀌면 자동 스케일.

### D2. 6종 전부 1차 포함
베이스가 효과를 다 처리하므로 각 셰이프는 작다. 점 기반(Polygon/Bezier)도 D1로 직렬화 해결.

### D3. Clip = GoControl 가상 프로퍼티 (GUI 일반성 유지)
GUI.cs가 GsShape를 직접 알지 않게, `GoControl`에 가상 프로퍼티를 두고 GsShape가 구동:

```csharp
// GoControl — IsBindingSuppressed와 같은 패턴
protected internal virtual bool ClipToBounds => true;
// GsShape
protected internal override bool ClipToBounds => Clip;
```
```csharp
// GUI.cs Draw (현재 line 88: canvas.ClipRect(c.Bounds);)
if ((c as GoControl)?.ClipToBounds ?? true) canvas.ClipRect(c.Bounds);
canvas.Translate(c.Left, c.Top);
c.FireDraw(canvas, thm);
```
Clip=false면 shadow/glow/회전/굵은 stroke가 bounds 밖으로 나가도 안 잘린다. (트레이드오프: 형제 위로 그려질 수 있음 — 작성자 책임.)

---

## GsShape 베이스

```csharp
public abstract class GsShape : GoControl
{
    // ── Fill ──
    string? Fill;                 // 테마 색 키, null=안 채움
    GsFillType FillType;          // Solid/Linear/Radial (신규 enum)
    string? FillColor2;           // 그라데이션 끝색, null=Fill
    float GradientAngle = 90;     // Linear 방향(도)

    // ── Stroke ──
    string? StrokeColor;          // null=없음
    float StrokeWidth = 1;

    // ── Shadow ──
    string? ShadowColor; float ShadowX, ShadowY=2, ShadowBlur=4;

    // ── Glow ──
    string? GlowColor;  float GlowBlur=8;   // 오프셋 0 외곽 블러

    // ── Transform / Clip ──
    float Rotation = 0;           // 도, bounds 중심 기준
    bool  Clip = true;            // false → GUI가 ClipToBounds로 clip 생략

    protected internal override bool ClipToBounds => Clip;

    /// 각 셰이프가 구현하는 유일한 것: bounds 안의 경로.
    protected abstract SKPath GetPath(SKRect bounds);
    /// 닫힌 도형이면 fill 가능. 기본 true. Line/열린 Polyline은 false 오버라이드.
    protected virtual bool Fillable => true;

    protected override void OnDraw(SKCanvas canvas, GoTheme thm)
    {
        var rt = Areas()["Content"];
        canvas.Save();
        if (Rotation != 0) canvas.RotateDegrees(Rotation, rt.MidX, rt.MidY);
        using var path = GetPath(rt);

        // 1) Glow (오프셋0 DropShadowOnly)  2) Shadow  — 실루엣 뒤에 깔림
        DrawEffect(canvas, path, GlowColor,  0, 0, GlowBlur, thm);
        DrawEffect(canvas, path, ShadowColor, ShadowX, ShadowY, ShadowBlur, thm);
        // 3) Fill (Fillable && Fill!=null) — Solid/Linear/Radial shader
        // 4) Stroke (StrokeColor!=null)
        canvas.Restore();
        base.OnDraw(canvas, thm);
    }
}
```

- **효과 적용 대상**: 닫힌 도형은 fill 실루엣, Line/열린 도형은 stroke 실루엣에 shadow/glow.
- **그라데이션/shader**: Solid=색, Linear=각도 두 점, Radial=중심→외곽 (삭제한 VoBox 로직 재활용).

## 셰이프별 고유 속성 + 기하

| 셰이프 | 고유 속성 | GetPath |
|---|---|---|
| **GsRect** | `CornerRadius` | RoundRect(bounds, r) |
| **GsCircle** | — | AddOval(bounds) (정사각이면 원, 아니면 타원) |
| **GsArc** | `StartAngle`,`SweepAngle`,`Thickness`,`RoundCap` | AddArc(정사각 inset) — stroke 전용(Fillable=false) |
| **GsLine** | `Points`(2점) | MoveTo/LineTo — Fillable=false |
| **GsPolygon** | `Points`(N점),`Closed` | MoveTo+LineTo…(+Close) — Fillable=Closed |
| **GsBezier** | `Points`(제어점),`Closed` | CubicTo 연결 — Fillable=Closed |

- `Points`: `[GoSizesProperty]`-류 `List<string>`, 각 `"x,y"` 정규화(0~1). 그리기 시 `(rt.Left+x*rt.Width, rt.Top+y*rt.Height)`.
- GsArc는 각도가 있어 Points 불필요(VoArc 로직 흡수).

---

## 신규/수정

| 파일 | 변경 |
|---|---|
| `Going.UI/Controls/Shapes/GsShape.cs` | **신규** — 베이스(효과 파이프라인) |
| `Going.UI/Controls/Shapes/GsRect.cs` 등 6개 | **신규** — 셰이프별 GetPath |
| `Going.UI/Enums/GsFillType.cs` | **신규** — Solid/Linear/Radial |
| `Going.UI/Controls/GoControl.cs` | `ClipToBounds` 가상 프로퍼티(default true) |
| `Going.UI/Utils/GUI.cs` | `Draw`의 ClipRect를 `ClipToBounds` 조건부로 |
| `Going.UI/Gudx/PATTERNS.md` | (Gs*는 leaf P1, 매트릭스 영향 적음) |

기존 펌프·gudx 자동 등록(IGoControl 리플렉션 스캔) 재사용.

---

## 테스트 전략

- **Clip 토글**: GsRect Clip=false → `ClipToBounds` false; GUI.Draw가 ClipRect 생략(가짜 컨테이너+자식, 바깥 픽셀 그려짐 검증 or ClipToBounds 단위).
- **GetPath 기하**: 각 셰이프 path bounds/포인트 수 검증(정규화 환산 포함).
- **효과 무크래시 렌더**: Fill/Stroke/Shadow/Glow/Rotation 조합으로 1×1 캔버스 FireDraw 예외 없음.
- **gudx 라운드트립**: 각 셰이프 속성 + `Points` List<string> 보존.
- **Fillable**: GsLine/열린 Polygon은 fill 안 함, Closed면 fill.

---

## 열린 결정 (유저 리뷰)

1. **GsCircle vs GsEllipse 네이밍** — bounds 맞춤 타원이라 비정사각이면 타원. 이름은 `GsCircle` 유지(요청대로) vs `GsEllipse`.
2. **효과 순서/품질** — glow를 fill 앞/뒤 어디에(현재: fill 뒤 깔기). 1차 기본값으로 충분.

---

## 결정 요약

| 항목 | 결정 |
|---|---|
| 베이스 | `GsShape : GoControl`, 효과(Fill/Stroke/Shadow/Glow/Rotate/Clip) 일괄 |
| 셰이프 | Rect/Circle/Arc/Line/Polygon/Bezier 6종, `GetPath`만 구현 |
| 점 좌표 | 정규화 0~1, `List<string>("x,y")` 직렬화 |
| Clip | `GoControl.ClipToBounds` 가상 → GsShape.Clip 구동, GUI 조건부 ClipRect |
| Fill | GsFillType(Solid/Linear/Radial) |
| 직렬화 | gudx 자동(P1 + List<string>) |
