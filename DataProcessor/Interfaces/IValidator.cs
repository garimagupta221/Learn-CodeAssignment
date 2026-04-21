namespace DataProcessor.Interfaces
{
    public interface IValidator<T>
    {
        bool Validate(T item, out string error);
    }
}