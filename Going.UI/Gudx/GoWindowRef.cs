namespace Going.UI.Gudx;

/// <summary>
/// Master.gudx reference to a separate Window .gudx file.
/// Emitted as &lt;GoWindowRef File="Windows/Settings.gudx"/&gt; by Task 12's Master split logic.
/// Phase 0: placeholder marker class — wiring lives in Task 12 SerializeGoDesignToFiles.
/// </summary>
public sealed class GoWindowRef
{
    public string? File { get; set; }
}
