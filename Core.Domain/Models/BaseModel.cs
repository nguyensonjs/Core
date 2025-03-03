using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Models
{
    public class BaseModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDeleted { get; set; } = false;
    }
}
