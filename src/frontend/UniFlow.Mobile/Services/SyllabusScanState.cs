using UniFlow.Mobile.Models;

namespace UniFlow.Mobile.Services;

public sealed class SyllabusScanState : ISyllabusScanState
{
    public SyllabusScanResponseDto? Current { get; set; }

    public void Clear() => Current = null;
}
