using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Dashboard;
using UniFlow.Entity.Enums;

namespace UniFlow.Business.Services;

/// <summary>
/// Deterministic, template-based daily messages tuned by <see cref="PersonalityVibe"/>.
/// No Gemini or external AI calls — fast and cost-free for dashboard reads.
/// </summary>
public sealed class DailyMessageService : IDailyMessageService
{
    private static readonly IReadOnlyDictionary<(DailyMessageScenario Scenario, PersonalityVibe Vibe), string[]> Templates =
        BuildTemplates();

    public string BuildDailyMessage(DailyMessageContext context)
    {
        var scenario = ResolveScenario(context);
        var vibe = context.PersonalityVibe;

        if (!Templates.TryGetValue((scenario, vibe), out var options) || options.Length == 0)
        {
            options = Templates[(DailyMessageScenario.Steady, PersonalityVibe.Friendly)];
        }

        var index = SelectTemplateIndex(context.UserId, context.Today, scenario, vibe, options.Length);
        return FormatTemplate(options[index], context);
    }

    private static DailyMessageScenario ResolveScenario(DailyMessageContext context)
    {
        if (context.OverdueTasksCount > 3)
        {
            return DailyMessageScenario.CriticalOverdue;
        }

        if (context.OverdueTasksCount > 0)
        {
            return DailyMessageScenario.HasOverdue;
        }

        if (context.CompletedTodayCount >= 3)
        {
            return DailyMessageScenario.ProductiveDay;
        }

        if (context.PendingTodayCount >= 3)
        {
            return DailyMessageScenario.BusyToday;
        }

        return DailyMessageScenario.Steady;
    }

    private static int SelectTemplateIndex(
        long userId,
        DateTime today,
        DailyMessageScenario scenario,
        PersonalityVibe vibe,
        int templateCount)
    {
        var seed = HashCode.Combine(userId, today.Year, today.Month, today.Day, scenario, vibe);
        return Math.Abs(seed) % templateCount;
    }

    private static string FormatTemplate(string template, DailyMessageContext context)
    {
        var focus = context.BigThreeTasks.Count > 0
            ? context.BigThreeTasks[0].Title
            : "öncelikli işin";

        return template
            .Replace("{overdue}", context.OverdueTasksCount.ToString(), StringComparison.Ordinal)
            .Replace("{completed}", context.CompletedTodayCount.ToString(), StringComparison.Ordinal)
            .Replace("{pending}", context.PendingTodayCount.ToString(), StringComparison.Ordinal)
            .Replace("{focus}", focus, StringComparison.Ordinal);
    }

    private static IReadOnlyDictionary<(DailyMessageScenario, PersonalityVibe), string[]> BuildTemplates()
    {
        var map = new Dictionary<(DailyMessageScenario, PersonalityVibe), string[]>();

        foreach (PersonalityVibe vibe in Enum.GetValues<PersonalityVibe>())
        {
            map[(DailyMessageScenario.CriticalOverdue, vibe)] = vibe switch
            {
                PersonalityVibe.Friendly =>
                [
                    "{overdue} geciken işin var — bugün sadece bir tanesini kapatman bile fark yaratır.",
                    "Liste biraz kabarmış ({overdue} gecikme). Birlikte küçük bir adımla başlayalım.",
                ],
                PersonalityVibe.Strict =>
                [
                    "{overdue} geciken iş. Erteleme yok — bugün en az birini kapat.",
                    "Kritik gecikme: {overdue} iş. Planı netleştir ve uygula.",
                ],
                PersonalityVibe.Sarcastic =>
                [
                    "{overdue} geciken iş… Takvim seni özlemiş olmalı.",
                    "Gecikme sayısı {overdue}. Bu tempoyla final sürpriz olur.",
                ],
                PersonalityVibe.Motivational =>
                [
                    "{overdue} gecikme var — bugün birini bitirerek momentumu geri al!",
                    "Zor görünüyor ama {overdue} işten birini bugün kapatırsan gün kazanılır!",
                ],
                PersonalityVibe.Calm =>
                [
                    "{overdue} geciken iş var. Sakin sakin birini seç ve tamamla.",
                    "Gecikmeler birikmiş ({overdue}). Acele etmeden bir işe odaklan.",
                ],
                _ => [],
            };

            map[(DailyMessageScenario.HasOverdue, vibe)] = vibe switch
            {
                PersonalityVibe.Friendly =>
                [
                    "{overdue} geciken işin bekliyor — bugün {focus} ile başlamak iyi olur.",
                    "Küçük bir gecikme ({overdue}) normal; {focus} üzerinde ilerleyebilirsin.",
                ],
                PersonalityVibe.Strict =>
                [
                    "{overdue} gecikme. Bugün {focus} tamamlanmalı.",
                    "Geciken iş var. Önce {focus}, sonra diğerleri.",
                ],
                PersonalityVibe.Sarcastic =>
                [
                    "{overdue} iş gecikmiş… {focus} hâlâ orada, merak etme.",
                    "Gecikme: {overdue}. {focus} seni sabırla bekliyor.",
                ],
                PersonalityVibe.Motivational =>
                [
                    "{overdue} gecikme — {focus} ile bugün fark yarat!",
                    "Geciken {overdue} iş var; {focus} senin kazanacağın ilk zafer olabilir!",
                ],
                PersonalityVibe.Calm =>
                [
                    "{overdue} gecikmiş iş var. Bugün sadece {focus} yeterli.",
                    "Sakin bir tempo: önce {focus}, gerisi sonra.",
                ],
                _ => [],
            };

            map[(DailyMessageScenario.ProductiveDay, vibe)] = vibe switch
            {
                PersonalityVibe.Friendly =>
                [
                    "Bugün {completed} iş bitirdin — harika gidiyorsun!",
                    "{completed} tamamlanan iş, güzel bir gün oldu.",
                ],
                PersonalityVibe.Strict =>
                [
                    "{completed} iş tamam. Standartı koru, gevşeme.",
                    "Bugün {completed} kapanış. Aynı disiplinle devam.",
                ],
                PersonalityVibe.Sarcastic =>
                [
                    "{completed} iş bitmiş… Demek ki bugün çalıştın, şaşırtıcı.",
                    "Bugün {completed} iş — kutlama abartılmasın ama fena değil.",
                ],
                PersonalityVibe.Motivational =>
                [
                    "{completed} iş tamam! Bu enerjiyle bir tane daha!",
                    "Muhteşem — {completed} iş! Momentum seninle!",
                ],
                PersonalityVibe.Calm =>
                [
                    "Bugün {completed} iş tamamlandı. Dengeli bir ilerleme.",
                    "{completed} kapanış — sakin ve verimli bir gün.",
                ],
                _ => [],
            };

            map[(DailyMessageScenario.BusyToday, vibe)] = vibe switch
            {
                PersonalityVibe.Friendly =>
                [
                    "Bugün {pending} iş var — {focus} ile başlayalım.",
                    "{pending} bekleyen iş; adım adım ilerlersin.",
                ],
                PersonalityVibe.Strict =>
                [
                    "Bugün {pending} iş planlı. Önce {focus}.",
                    "{pending} iş bugün. Sırayı bozma: {focus} öncelik.",
                ],
                PersonalityVibe.Sarcastic =>
                [
                    "Bugün {pending} iş… Takvimin seni trollüyor olabilir.",
                    "{pending} iş bugün — {focus} en azından bir başlangıç.",
                ],
                PersonalityVibe.Motivational =>
                [
                    "{pending} iş bugün — {focus} ile tempoyu yakala!",
                    "Yoğun gün ({pending} iş)! {focus} senin ilk zaferin olsun!",
                ],
                PersonalityVibe.Calm =>
                [
                    "Bugün {pending} iş var. {focus} ile sade başla.",
                    "{pending} planlı iş — acele yok, {focus} yeterli.",
                ],
                _ => [],
            };

            map[(DailyMessageScenario.Steady, vibe)] = vibe switch
            {
                PersonalityVibe.Friendly =>
                [
                    "Sakin bir gün — {focus} üzerinde odaklanman yeterli.",
                    "Bugün dengeli görünüyor. {focus} iyi bir başlangıç.",
                ],
                PersonalityVibe.Strict =>
                [
                    "Gün sakin. {focus} bugün tamamlanacak hedef.",
                    "Plan net: önce {focus}, sonra değerlendir.",
                ],
                PersonalityVibe.Sarcastic =>
                [
                    "Sakin gün… Ya da erteleme molası mı? {focus} cevap versin.",
                    "Bugün sakin — {focus} yine listende duruyor.",
                ],
                PersonalityVibe.Motivational =>
                [
                    "Harika bir gün seni bekliyor — {focus} ile başla!",
                    "Küçük bir adım: {focus}. Gerisi gelir!",
                ],
                PersonalityVibe.Calm =>
                [
                    "Sakin bir gün. {focus} üzerinde hafifçe ilerle.",
                    "Bugün baskı yok — istersen {focus} ile devam et.",
                ],
                _ => [],
            };
        }

        return map;
    }

    private enum DailyMessageScenario
    {
        CriticalOverdue,
        HasOverdue,
        ProductiveDay,
        BusyToday,
        Steady,
    }
}
