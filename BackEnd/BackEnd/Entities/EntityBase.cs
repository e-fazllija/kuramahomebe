using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Entities
{
    public class EntityBase
    {
        public int Id { get; set; } 
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
