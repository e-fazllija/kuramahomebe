using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;


namespace BackEnd.Services.Repositories
{
    public class DocumentsTabRepository : GenericRepository<DocumentsTab>, IDocumentsTabRepository
    {
        public DocumentsTabRepository(AppDbContext context) : base(context){}
    }
}
