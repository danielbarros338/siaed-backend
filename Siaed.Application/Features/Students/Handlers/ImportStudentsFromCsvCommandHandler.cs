using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Siaed.Application.Common;
using Siaed.Application.Features.Students.Commands;
using Siaed.Application.Interfaces;
using Siaed.Domain.Entities;
using Siaed.Domain.Enums;

namespace Siaed.Application.Features.Students.Handlers;

public sealed class ImportStudentsFromCsvCommandHandler
    : IRequestHandler<ImportStudentsFromCsvCommand, Result<ImportSummaryDto>>
{
    private const int MaxRows = 500;

    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolClassRepository _classRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportStudentsFromCsvCommandHandler(
        IStudentRepository studentRepository,
        ISchoolClassRepository classRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ImportSummaryDto>> Handle(
        ImportStudentsFromCsvCommand request,
        CancellationToken cancellationToken)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
        };

        List<CsvStudentRow> rows;
        try
        {
            using var ms = new MemoryStream(request.FileContent);
            using var reader = new StreamReader(ms);
            using var csv = new CsvReader(reader, config);
            rows = csv.GetRecords<CsvStudentRow>().Take(MaxRows + 1).ToList();
        }
        catch (Exception ex)
        {
            return Result<ImportSummaryDto>.Failure($"Erro ao ler o arquivo CSV: {ex.Message}");
        }

        if (rows.Count > MaxRows)
            return Result<ImportSummaryDto>.Failure($"O arquivo excede o limite de {MaxRows} linhas por importação.");

        var errors = new List<string>();
        int imported = 0;
        int skipped = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            int rowNumber = i + 2; // 1-based + header
            var row = rows[i];

            if (string.IsNullOrWhiteSpace(row.FullName))
            {
                errors.Add($"Linha {rowNumber}: NomeCompleto é obrigatório.");
                skipped++;
                continue;
            }

            if (!Enum.TryParse<DocumentType>(row.DocumentType, ignoreCase: true, out var docType))
            {
                errors.Add($"Linha {rowNumber}: TipoDocumento '{row.DocumentType}' inválido. Use: Cpf, RegistroEstrangeiro ou IdInterno.");
                skipped++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.DocumentId))
            {
                errors.Add($"Linha {rowNumber}: NumeroDocumento é obrigatório.");
                skipped++;
                continue;
            }

            if (!DateOnly.TryParseExact(row.BirthDate, ["yyyy-MM-dd", "dd/MM/yyyy"], null, DateTimeStyles.None, out var birthDate))
            {
                errors.Add($"Linha {rowNumber}: DataNascimento '{row.BirthDate}' inválida. Use yyyy-MM-dd ou dd/MM/yyyy.");
                skipped++;
                continue;
            }

            if (!Guid.TryParse(row.ClassId, out var classId))
            {
                errors.Add($"Linha {rowNumber}: ClassId '{row.ClassId}' é um GUID inválido.");
                skipped++;
                continue;
            }

            if (!DateOnly.TryParseExact(row.EnrollmentDate, ["yyyy-MM-dd", "dd/MM/yyyy"], null, DateTimeStyles.None, out var enrollmentDate))
            {
                errors.Add($"Linha {rowNumber}: DataMatricula '{row.EnrollmentDate}' inválida. Use yyyy-MM-dd ou dd/MM/yyyy.");
                skipped++;
                continue;
            }

            var schoolClass = await _classRepository.GetByIdAsync(classId, cancellationToken);
            if (schoolClass is null)
            {
                errors.Add($"Linha {rowNumber}: ClassId '{classId}' não encontrada.");
                skipped++;
                continue;
            }

            var alreadyExists = await _studentRepository.ExistsByDocumentAsync(
                row.DocumentId, cancellationToken);

            if (alreadyExists)
            {
                errors.Add($"Linha {rowNumber}: Aluno com documento '{row.DocumentId}' já cadastrado.");
                skipped++;
                continue;
            }

            var student = Student.Create(
                row.FullName.Trim(),
                docType,
                row.DocumentId.Trim(),
                birthDate,
                classId,
                enrollmentDate,
                row.Notes?.Trim());

            await _studentRepository.AddAsync(student, cancellationToken);
            imported++;
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        var summary = new ImportSummaryDto(imported, skipped, errors.AsReadOnly());
        return Result<ImportSummaryDto>.Success(summary);
    }
}
