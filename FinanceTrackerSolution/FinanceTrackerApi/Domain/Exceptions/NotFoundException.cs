namespace FinanceTrackerApi.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, Guid id)
            : base($"{name} with id {id} not found") { }
    }
}
