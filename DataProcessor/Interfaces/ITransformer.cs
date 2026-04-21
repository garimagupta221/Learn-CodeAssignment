namespace DataProcessor.Interfaces
{
    public interface ITransformer<T>
    {
        T Transform(T input);
    }
}