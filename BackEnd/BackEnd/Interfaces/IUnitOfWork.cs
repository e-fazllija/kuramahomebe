using BackEnd.Data;
using BackEnd.Interfaces.IRepositories;
using BackEnd.Services.Repositories;

namespace BackEnd.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        AppDbContext dbContext { get; }
        ICustomerRepository CustomerRepository { get; }
        IRealEstatePropertyRepository RealEstatePropertyRepository { get; }
        IRealEstatePropertyPhotoRepository RealEstatePropertyPhotoRepository { get; }
        IRequestRepository RequestRepository { get; }
        ICalendarRepository CalendarRepository { get; }
        IDocumentsTabRepository DocumentsTabRepository { get; }
        
        Task<int> SaveAsync();
        int Save();

    }
}