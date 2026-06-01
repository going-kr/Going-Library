namespace Going.UI.ViewObjects
{
    /// <summary>VoBox의 채우기 방식.</summary>
    public enum VoFillType
    {
        /// <summary>단색.</summary>
        Solid,
        /// <summary>선형 그라데이션 (Background → FillColor2, GradientAngle 방향).</summary>
        Linear,
        /// <summary>방사형 그라데이션 (중심 Background → 외곽 FillColor2).</summary>
        Radial,
    }
}
