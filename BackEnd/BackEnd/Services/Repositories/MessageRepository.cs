using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;

namespace BackEnd.Services.Repositories
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(AppDbContext context) : base(context) { }
    }
}
