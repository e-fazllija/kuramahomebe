using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;


namespace BackEnd.Services.Repositories
{
    public class RequestRepository : GenericRepository<Request>, IRequestRepository
    {
        public RequestRepository(AppDbContext context) : base(context){}
    }
}
