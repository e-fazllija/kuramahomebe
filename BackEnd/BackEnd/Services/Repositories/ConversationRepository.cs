using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;

namespace BackEnd.Services.Repositories
{
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(AppDbContext context) : base(context) { }
    }
}
