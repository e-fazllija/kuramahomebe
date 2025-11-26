using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;


namespace BackEnd.Services.Repositories
{
    public class RealEstatePropertyPhotoRepository : GenericRepository<RealEstatePropertyPhoto>, IRealEstatePropertyPhotoRepository
    {
        public RealEstatePropertyPhotoRepository(AppDbContext context) : base(context){}
    }
}
