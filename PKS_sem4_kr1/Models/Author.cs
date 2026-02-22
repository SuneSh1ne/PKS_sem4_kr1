using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PKS_sem4_kr1.Models
{
    public class Author
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        
        public DateTime BirthDate { get; set; }
        
        [MaxLength(100)]
        public string Country { get; set; }

        public ICollection<Book> Books { get; set; }
        
        public string FullName => $"{FirstName} {LastName}";
    }
}