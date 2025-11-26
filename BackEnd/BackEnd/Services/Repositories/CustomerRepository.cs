using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;


namespace BackEnd.Services.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context) : base(context){}
    }
}
