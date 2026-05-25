namespace Siaed.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, Guid id)
        : base($"{entityName} com ID '{id}' não foi encontrado.") { }
}
