using System;
using System.ComponentModel.DataAnnotations;

namespace PKS_sem4_kr1.Models
{
    public class Book
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        [MaxLength(13)]
        public string ISBN { get; set; }
        
        public int PublishYear { get; set; }
        
        [Required]
        public int QuantityInStock { get; set; }
        
        public int AuthorId { get; set; }
        public int GenreId { get; set; }

        public Author Author { get; set; }
        public Genre Genre { get; set; }
    }
}