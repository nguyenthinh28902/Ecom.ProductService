namespace Ecom.ProductService.Core.Abstractions.Persistence
{
    public interface IUnitOfWork
    {
        IRepository<T> Repository<T>() where T : class;
        Task SaveChangesAsync();
        void SaveChanges();
    }
}
