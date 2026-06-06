namespace Going.UI.Enums
{
    /// <summary>GsShape의 채우기 방식.</summary>
    public enum GsFillType
    {
        /// <summary>단색.</summary>
        Solid,
        /// <summary>선형 그라데이션 (Fill → FillColor2, GradientAngle 방향).</summary>
        Linear,
        /// <summary>방사형 그라데이션 (중심 Fill → 외곽 FillColor2).</summary>
        Radial,
    }
}
