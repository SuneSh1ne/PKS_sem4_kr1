using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKS_sem4_kr1.Models
{
    public class Genre
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; }
        
        public ICollection<Book> Books { get; set; }
    }
}