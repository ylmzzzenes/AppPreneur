using System.ComponentModel.DataAnnotations;

namespace UniFlow.API.Contracts;

public sealed class SyllabusIngestFormRequest
{
    [Required]
    public string CourseCode { get; set; } = string.Empty;

    [Required]
    public string CourseTitle { get; set; } = string.Empty;

    [Required]
    public IFormFile File { get; set; } = null!;
}
