using BackEnd.Data;
using BackEnd.Interfaces;
using BackEnd.Interfaces.IRepositories;
using BackEnd.Services.Repositories;

namespace BackEnd.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public AppDbContext dbContext { get; set; }
        public UnitOfWork(AppDbContext context)
        {
            this._context = context;
            this.dbContext = context;

            CustomerRepository = new CustomerRepository(this._context);
            RealEstatePropertyRepository = new RealEstatePropertyRepository(this._context);
            RealEstatePropertyPhotoRepository = new RealEstatePropertyPhotoRepository(this._context);
            RequestRepository = new RequestRepository(this._context);
            CalendarRepository = new CalendarRepository(this._context);
            DocumentsTabRepository = new DocumentsTabRepository(this._context);
        }

      
        public ICustomerRepository CustomerRepository
        {
            get;
            private set;
        }
        public ICalendarRepository CalendarRepository
        {
            get;
            private set;
        }
        public IRealEstatePropertyRepository RealEstatePropertyRepository
        {
            get;
            private set;
        }
        public IRealEstatePropertyPhotoRepository RealEstatePropertyPhotoRepository
        {
            get;
            private set;
        }
        public IRequestRepository RequestRepository
        {
            get;
            private set;
        }
        public IDocumentsTabRepository DocumentsTabRepository
        {
            get;
            private set;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();

        }

        public int Save()
        {
            return  _context.SaveChanges();

        }
    }
}