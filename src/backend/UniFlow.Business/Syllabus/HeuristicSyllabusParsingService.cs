using System.Globalization;
using System.Text.RegularExpressions;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Dtos;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Syllabus;

/// <summary>
/// Development fallback when Gemini is not configured. Extracts common TR/EN deadline lines from plain text.
/// </summary>
public sealed class HeuristicSyllabusParsingService : ISyllabusParsingService
{
    private static readonly Dictionary<string, string> CategoryByKeyword = new(StringComparer.OrdinalIgnoreCase)
    {
        ["vize"] = "Midterm",
        ["midterm"] = "Midterm",
        ["final"] = "Final",
        ["odev"] = "Homework",
        ["ödev"] = "Homework",
        ["homework"] = "Homework",
        ["assignment"] = "Homework",
        ["quiz"] = "Quiz",
        ["proje"] = "Project",
        ["project"] = "Project",
    };

    private static readonly Dictionary<string, int> TurkishMonths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ocak"] = 1, ["subat"] = 2, ["şubat"] = 2, ["mart"] = 3, ["nisan"] = 4,
        ["mayis"] = 5, ["mayıs"] = 5, ["haziran"] = 6, ["temmuz"] = 7, ["agustos"] = 8,
        ["ağustos"] = 8, ["eylul"] = 9, ["eylül"] = 9, ["ekim"] = 10, ["kasim"] = 11,
        ["kasım"] = 11, ["aralik"] = 12, ["aralık"] = 12,
    };

    private static readonly Regex IsoDateRegex = new(
        @"\b(\d{4})-(\d{2})-(\d{2})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex DmySlashRegex = new(
        @"\b(\d{1,2})[./](\d{1,2})[./](\d{4})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex TurkishDateRegex = new(
        @"\b(\d{1,2})\s+([A-Za-zÇĞİÖŞÜçğıöşü]+)\s+(\d{4})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public Task<Result<IReadOnlyList<SyllabusTaskDraft>>> ParseTasksFromSyllabusTextAsync(
        string syllabusText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(syllabusText))
        {
            return Task.FromResult(Result<IReadOnlyList<SyllabusTaskDraft>>.Fail(
                "SYLLABUS_EMPTY",
                "Syllabus text is empty."));
        }

        var drafts = new List<SyllabusTaskDraft>();
        foreach (var line in syllabusText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.Length < 4)
            {
                continue;
            }

            var dueDate = TryParseDate(trimmed);
            if (dueDate is null)
            {
                continue;
            }

            var category = InferCategory(trimmed);
            var title = BuildTitle(trimmed, category);

            drafts.Add(new SyllabusTaskDraft
            {
                Title = title,
                Description = trimmed,
                DueDate = dueDate,
                Category = category,
            });
        }

        return Task.FromResult(Result<IReadOnlyList<SyllabusTaskDraft>>.Success(drafts));
    }

    private static string InferCategory(string line)
    {
        foreach (var (keyword, category) in CategoryByKeyword)
        {
            if (line.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return category;
            }
        }

        return "Other";
    }

    private static string BuildTitle(string line, string category)
    {
        var colon = line.IndexOf(':');
        if (colon > 0 && colon < 40)
        {
            return line[..colon].Trim();
        }

        return category switch
        {
            "Midterm" => "Vize",
            "Final" => "Final",
            "Homework" => "Odev",
            "Quiz" => "Quiz",
            "Project" => "Proje",
            _ => "Gorev",
        };
    }

    private static DateTime? TryParseDate(string line)
    {
        var iso = IsoDateRegex.Match(line);
        if (iso.Success &&
            DateTime.TryParseExact(iso.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var isoDate))
        {
            return isoDate.Date;
        }

        var dmy = DmySlashRegex.Match(line);
        if (dmy.Success &&
            DateTime.TryParseExact(dmy.Value, new[] { "d/M/yyyy", "d.M.yyyy", "dd/MM/yyyy", "dd.MM.yyyy" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dmyDate))
        {
            return dmyDate.Date;
        }

        var tr = TurkishDateRegex.Match(line);
        if (tr.Success &&
            int.TryParse(tr.Groups[1].Value, out var day) &&
            int.TryParse(tr.Groups[3].Value, out var year) &&
            TurkishMonths.TryGetValue(tr.Groups[2].Value, out var month))
        {
            try
            {
                return new DateTime(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        return null;
    }
}
