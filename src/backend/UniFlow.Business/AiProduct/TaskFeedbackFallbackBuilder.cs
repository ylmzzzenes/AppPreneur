using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;

namespace UniFlow.Business.AiProduct;

public static class TaskFeedbackFallbackBuilder
{
    public static TaskFeedbackResponse Build(
        TaskItemSummary task,
        TaskItemStatus newStatus,
        AiUserProfileContext profile)
    {
        var vibe = profile.PersonalityVibe;

        return newStatus switch
        {
            TaskItemStatus.Done => new TaskFeedbackResponse
            {
                Message = vibe switch
                {
                    PersonalityVibe.Strict => $"{task.Title} tamam — sıradaki göreve geç.",
                    PersonalityVibe.Sarcastic => $"{task.Title} bitti. Şaşırtıcı derecede verimli.",
                    PersonalityVibe.Calm => $"{task.Title} tamamlandı. Güzel bir ilerleme.",
                    _ => $"Harika! {task.Title} tamamlandı.",
                },
                Tone = "Motivational",
                NextAction = "Big 3 listendeki bir sonraki göreve bak.",
                IsFallback = true,
            },
            TaskItemStatus.Missed => new TaskFeedbackResponse
            {
                Message = vibe switch
                {
                    PersonalityVibe.Strict => $"{task.Title} kaçırıldı. Bugün 30 dk ayır ve telafi et.",
                    PersonalityVibe.Sarcastic => $"{task.Title} kaçmış. Panik yok, plan yap.",
                    _ => $"{task.Title} kaçırıldı ama hâlâ telafi edilebilir.",
                },
                Tone = "Direct",
                NextAction = "Görevi daha küçük parçalara böl ve bugün 30 dakika ayır.",
                IsFallback = true,
            },
            _ => new TaskFeedbackResponse
            {
                Message = $"{task.Title} bekliyor. Bugün başlamak için iyi bir zaman.",
                Tone = "Calm",
                NextAction = task.DueDate.HasValue
                    ? $"Son tarih {task.DueDate.Value:dd MMM} — 25 dakikalık bir oturum planla."
                    : "25 dakikalık bir odak oturumu planla.",
                IsFallback = true,
            },
        };
    }
}
