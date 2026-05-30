using UniFlow.Mobile.Models;

namespace UniFlow.Mobile.Services;

public interface ISyllabusScanState
{
    SyllabusScanResponseDto? Current { get; set; }

    void Clear();
}
