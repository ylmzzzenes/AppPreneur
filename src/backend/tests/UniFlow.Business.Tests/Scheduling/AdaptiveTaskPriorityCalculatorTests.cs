using FluentAssertions;
using UniFlow.Business.Dtos;
using UniFlow.Business.Scheduling;
using UniFlow.Entity.Enums;
using Xunit;

namespace UniFlow.Business.Tests.Scheduling;

public sealed class AdaptiveTaskPriorityCalculatorTests
{
    private static readonly DateTime ReferenceUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly AdaptiveTaskPriorityCalculator _sut = new();

    [Fact]
    public void CalculateScore_DueToday_OutscoresDueInThirtyDays()
    {
        var today = _sut.CalculateScore(CreateInput(dueDate: ReferenceUtc.Date));
        var farFuture = _sut.CalculateScore(CreateInput(dueDate: ReferenceUtc.Date.AddDays(30)));

        today.Should().BeGreaterThan(farFuture);
    }

    [Fact]
    public void CalculateScore_Overdue_OutscoresDueNextWeek()
    {
        var overdue = _sut.CalculateScore(CreateInput(dueDate: ReferenceUtc.Date.AddDays(-2)));
        var nextWeek = _sut.CalculateScore(CreateInput(dueDate: ReferenceUtc.Date.AddDays(7)));

        overdue.Should().BeGreaterThan(nextWeek);
        overdue.Should().BeGreaterThanOrEqualTo(80, "overdue tasks should remain in the upper priority band");
    }

    [Fact]
    public void CalculateScore_WhenStatusIsDone_ReturnsMinimumScore()
    {
        var doneScore = _sut.CalculateScore(CreateInput(
            dueDate: ReferenceUtc.Date.AddDays(-5),
            category: "Final",
            difficulty: 5,
            status: TaskItemStatus.Done));

        var pendingSameDue = _sut.CalculateScore(CreateInput(
            dueDate: ReferenceUtc.Date.AddDays(-5),
            category: "Final",
            difficulty: 5,
            status: TaskItemStatus.Pending));

        doneScore.Should().Be(1);
        pendingSameDue.Should().BeGreaterThan(doneScore);
    }

    [Theory]
    [InlineData(null, "Homework", 3)]
    [InlineData(null, "Final", 5)]
    [InlineData(-10, "Midterm", 3)]
    [InlineData(0, "Quiz", 3)]
    [InlineData(1, "Project", 4)]
    [InlineData(45, "OTHER", 2)]
    [InlineData(120, null, 3)]
    public void CalculateScore_VariedInputs_AlwaysWithinInclusiveRange1To100(
        int? daysFromReference,
        string? category,
        int difficulty)
    {
        DateTime? due = daysFromReference.HasValue
            ? ReferenceUtc.Date.AddDays(daysFromReference.Value)
            : null;

        var score = _sut.CalculateScore(CreateInput(dueDate: due, category: category, difficulty: difficulty));

        score.Should().BeInRange(1, 100);
    }

    [Fact]
    public void CalculateScore_NullDueDate_DoesNotThrowAndReturnsValidScore()
    {
        var act = () => _sut.CalculateScore(CreateInput(dueDate: null));

        act.Should().NotThrow();
        act().Should().BeInRange(1, 100);
    }

    [Fact]
    public void CalculateScore_SameInput_ReturnsSameScoreOnRepeatedCalls()
    {
        var input = CreateInput(
            dueDate: ReferenceUtc.Date.AddDays(3),
            category: "Midterm",
            difficulty: 4);

        var first = _sut.CalculateScore(input);
        var second = _sut.CalculateScore(input);

        first.Should().Be(second);
    }

    [Fact]
    public void CalculateScore_FinalCategory_OutscoresHomeworkForSameDueDate()
    {
        var due = ReferenceUtc.Date.AddDays(5);

        var finalScore = _sut.CalculateScore(CreateInput(dueDate: due, category: "Final"));
        var homeworkScore = _sut.CalculateScore(CreateInput(dueDate: due, category: "Homework"));

        finalScore.Should().BeGreaterThan(homeworkScore);
    }

    [Fact]
    public void CalculateScore_HigherDifficulty_OutscoresLowerDifficultyForSameDueAndCategory()
    {
        var due = ReferenceUtc.Date.AddDays(4);

        var hard = _sut.CalculateScore(CreateInput(dueDate: due, category: "Quiz", difficulty: 5));
        var easy = _sut.CalculateScore(CreateInput(dueDate: due, category: "Quiz", difficulty: 1));

        hard.Should().BeGreaterThan(easy);
    }

    [Fact]
    public void CalculateScore_DifficultyOutsideRange_IsClampedWithoutThrowing()
    {
        var act = () => _sut.CalculateScore(CreateInput(
            dueDate: ReferenceUtc.Date.AddDays(2),
            difficulty: 99));

        act.Should().NotThrow();
        act().Should().BeInRange(1, 100);
    }

    [Fact]
    public void CalculateScore_OverdueScore_IsAtLeastAsHighAsDueToday()
    {
        var overdue = _sut.CalculateScore(CreateInput(dueDate: ReferenceUtc.Date.AddDays(-1)));
        var dueToday = _sut.CalculateScore(CreateInput(dueDate: ReferenceUtc.Date));

        overdue.Should().BeGreaterThanOrEqualTo(dueToday);
    }

    private static TaskSchedulingInput CreateInput(
        DateTime? dueDate = null,
        string? category = null,
        int difficulty = 3,
        TaskItemStatus? status = null) =>
        new()
        {
            DueDate = dueDate,
            Category = category,
            Difficulty = difficulty,
            ReferenceUtc = ReferenceUtc,
            Status = status,
        };
}
