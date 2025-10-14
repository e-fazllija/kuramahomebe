using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;


namespace BackEnd.Services.Repositories
{
    public class RealEstatePropertyRepository : GenericRepository<RealEstateProperty>, IRealEstatePropertyRepository
    {
        public RealEstatePropertyRepository(AppDbContext context) : base(context){}
    }
}
