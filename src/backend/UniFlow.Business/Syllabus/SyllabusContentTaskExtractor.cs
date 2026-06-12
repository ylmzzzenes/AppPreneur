using System.Text.RegularExpressions;
using UniFlow.Business.Dtos;

namespace UniFlow.Business.Syllabus;

/// <summary>
/// Builds study tasks from "Dersin Amacı" and "Dersin İçeriği" sections when no dated deadlines exist.
/// </summary>
public static class SyllabusContentTaskExtractor
{
    private const string StudyCategory = "Study";

    private static readonly string[] NextSectionMarkers =
    [
        "Dersin Koordinatörü",
        "Dersi Veren",
        "Ders Öğrenme Kazanımları",
        "Ön Koşul",
        "Dersin Seviyesi",
        "Dersin Verilişi",
        "AKTS",
        "Haftalık",
        "Değerlendirme",
        "Kaynak",
        "Learning Outcomes",
        "Course Objective",
        "Course Content",
    ];

    private static readonly Regex LearningOutcomeLineRegex = new(
        @"(yapabilmek|edebilmek|kavrayabilmek|bilmek|söyleyebilmek|çizebilmek|tanımlayabilmek|anlayabilmek|uygulayabilmek|açıklayabilmek)\.?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex InlineSectionRegex = new(
        @"^(?<label>Dersin Amacı|Dersin İçeriği|Ders Öğrenme Kazanımları)\s*[:\|]?\s*(?<value>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static IReadOnlyList<SyllabusTaskDraft> ExtractContentTasks(string syllabusText)
    {
        if (string.IsNullOrWhiteSpace(syllabusText))
        {
            return [];
        }

        var normalized = syllabusText.Replace("\r\n", "\n", StringComparison.Ordinal);
        var tasks = new List<SyllabusTaskDraft>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var objective = ExtractSectionValue(normalized, "Dersin Amacı");
        if (!string.IsNullOrWhiteSpace(objective))
        {
            AddTask(tasks, seen, new SyllabusTaskDraft
            {
                Title = TruncateTitle("Ders amacı: " + CompactWhitespace(objective), 120),
                Description = CompactWhitespace(objective),
                Category = StudyCategory,
            });
        }

        var contentBlock = ExtractSectionBlock(normalized, "Dersin İçeriği");
        AddContentItems(tasks, seen, contentBlock);

        var outcomesBlock = ExtractSectionBlock(normalized, "Ders Öğrenme Kazanımları");
        AddContentItems(tasks, seen, outcomesBlock);

        foreach (var line in normalized.Split('\n'))
        {
            var trimmed = CompactWhitespace(line);
            if (trimmed.Length < 15)
            {
                continue;
            }

            var inline = InlineSectionRegex.Match(trimmed);
            if (inline.Success)
            {
                var label = inline.Groups["label"].Value;
                var value = inline.Groups["value"].Value.Trim();
                if (label.Contains("Amacı", StringComparison.OrdinalIgnoreCase) && value.Length > 10)
                {
                    AddTask(tasks, seen, new SyllabusTaskDraft
                    {
                        Title = TruncateTitle("Ders amacı: " + value, 120),
                        Description = value,
                        Category = StudyCategory,
                    });
                }
                else if (value.Length > 10)
                {
                    AddOutcomeLine(tasks, seen, value);
                }

                continue;
            }

            if (LearningOutcomeLineRegex.IsMatch(trimmed))
            {
                AddOutcomeLine(tasks, seen, trimmed);
            }
        }

        return tasks;
    }

    private static void AddContentItems(List<SyllabusTaskDraft> tasks, HashSet<string> seen, string block)
    {
        if (string.IsNullOrWhiteSpace(block))
        {
            return;
        }

        foreach (var item in SplitContentItems(block))
        {
            AddOutcomeLine(tasks, seen, item);
        }
    }

    private static IEnumerable<string> SplitContentItems(string block)
    {
        foreach (var rawLine in block.Split('\n'))
        {
            var line = CompactWhitespace(rawLine);
            if (line.Length < 10)
            {
                continue;
            }

            if (StartsWithSectionMarker(line))
            {
                continue;
            }

            line = Regex.Replace(line, @"^[\-\*•●]\s*", string.Empty);
            line = Regex.Replace(line, @"^\d+[\.\)]\s*", string.Empty);
            line = CompactWhitespace(line);

            if (line.Length >= 10)
            {
                yield return line;
            }
        }
    }

    private static void AddOutcomeLine(List<SyllabusTaskDraft> tasks, HashSet<string> seen, string line)
    {
        var text = CompactWhitespace(line);
        if (text.Length < 10)
        {
            return;
        }

        AddTask(tasks, seen, new SyllabusTaskDraft
        {
            Title = TruncateTitle(text, 120),
            Description = text,
            Category = StudyCategory,
        });
    }

    private static void AddTask(List<SyllabusTaskDraft> tasks, HashSet<string> seen, SyllabusTaskDraft draft)
    {
        var key = draft.Title.Trim();
        if (!seen.Add(key))
        {
            return;
        }

        tasks.Add(draft);
    }

    private static string ExtractSectionValue(string text, string sectionName)
    {
        var block = ExtractSectionBlock(text, sectionName);
        return CompactWhitespace(block.Replace('\n', ' '));
    }

    private static string ExtractSectionBlock(string text, string sectionName)
    {
        var lines = text.Split('\n');
        var collecting = false;
        var buffer = new List<string>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                if (collecting && buffer.Count > 0)
                {
                    buffer.Add(string.Empty);
                }

                continue;
            }

            if (LineStartsSection(line, sectionName))
            {
                collecting = true;
                var inline = InlineSectionRegex.Match(line);
                if (inline.Success && inline.Groups["label"].Value.Contains(sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    var value = inline.Groups["value"].Value.Trim();
                    if (value.Length > 0)
                    {
                        buffer.Add(value);
                    }
                }

                continue;
            }

            if (collecting && StartsWithSectionMarker(line))
            {
                break;
            }

            if (collecting)
            {
                buffer.Add(line);
            }
        }

        return string.Join('\n', buffer).Trim();
    }

    private static bool LineStartsSection(string line, string sectionName) =>
        line.StartsWith(sectionName, StringComparison.OrdinalIgnoreCase)
        || line.Contains(sectionName + ":", StringComparison.OrdinalIgnoreCase)
        || line.Contains(sectionName + " |", StringComparison.OrdinalIgnoreCase);

    private static bool StartsWithSectionMarker(string line)
    {
        foreach (var marker in NextSectionMarkers)
        {
            if (line.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string CompactWhitespace(string value) =>
        Regex.Replace(value.Trim(), @"\s+", " ");

    private static string TruncateTitle(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..(maxLength - 1)].TrimEnd() + "…";
}
