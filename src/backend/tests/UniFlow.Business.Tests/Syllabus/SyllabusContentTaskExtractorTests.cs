using FluentAssertions;
using UniFlow.Business.Syllabus;
using Xunit;

namespace UniFlow.Business.Tests.Syllabus;

public sealed class SyllabusContentTaskExtractorTests
{
    [Fact]
    public void ExtractContentTasks_FromMat111Syllabus_CreatesObjectiveAndContentTasks()
    {
        const string text = """
            Dersin Amacı
            Öğrencilere kendi alanları ile ilgili matematiksel becerilerin kazandırılması.
            Dersin İçeriği
            Sayılar sınıflandırmasını yapabilmek
            Eşitsizlik ve mutlak değer kavramlarını bilmek
            Analitik Düzlemi ve koordinat sistemi kavrayabilmek
            Polinomları ve özdeşlikleri kavrayabilmek
            Fonksiyonu tanımlayıp çeşitlerini ve özelliklerini söyleyebilmek
            Trigonometriyi ve trigonometrik fonksiyonları kavrayabilmek
            Trigonometrik fonksiyonların grafiklerini çizebilmek
            Ders Öğrenme Kazanımları
            """;

        var tasks = SyllabusContentTaskExtractor.ExtractContentTasks(text);

        tasks.Should().NotBeEmpty();
        tasks.Should().Contain(t => t.Title.Contains("Ders amacı", StringComparison.OrdinalIgnoreCase));
        tasks.Should().Contain(t => t.Title.Contains("Sayılar sınıflandırmasını", StringComparison.OrdinalIgnoreCase));
        tasks.Should().Contain(t => t.Title.Contains("Trigonometrik fonksiyonların grafiklerini", StringComparison.OrdinalIgnoreCase));
        tasks.Should().OnlyContain(t => t.Category == "Study");
    }

    [Fact]
    public void ExtractContentTasks_InlineTableRow_ExtractsObjectiveAndItems()
    {
        const string text = """
            Dersin Amacı | Öğrencilere kendi alanları ile ilgili matematiksel becerilerin kazandırılması.
            Dersin İçeriği | Sayılar sınıflandırmasını yapabilmek
            Eşitsizlik ve mutlak değer kavramlarını bilmek
            """;

        var tasks = SyllabusContentTaskExtractor.ExtractContentTasks(text);

        tasks.Should().HaveCountGreaterThanOrEqualTo(3);
    }
}
