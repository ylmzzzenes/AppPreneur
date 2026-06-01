using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface IStudyPlanService
{
    Task<Result<StudyPlanResponse>> GenerateAsync(
        long userId,
        StudyPlanRequest request,
        CancellationToken cancellationToken = default);
}
