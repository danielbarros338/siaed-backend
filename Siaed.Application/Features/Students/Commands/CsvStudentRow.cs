using CsvHelper.Configuration.Attributes;

namespace Siaed.Application.Features.Students.Commands;

public sealed class CsvStudentRow
{
    [Name("NomeCompleto")]
    public string FullName { get; set; } = string.Empty;

    [Name("TipoDocumento")]
    public string DocumentType { get; set; } = string.Empty;

    [Name("NumeroDocumento")]
    public string DocumentId { get; set; } = string.Empty;

    [Name("DataNascimento")]
    public string BirthDate { get; set; } = string.Empty;

    [Name("ClassId")]
    public string ClassId { get; set; } = string.Empty;

    [Name("DataMatricula")]
    public string EnrollmentDate { get; set; } = string.Empty;

    [Name("Observacoes")]
    public string? Notes { get; set; }
}
