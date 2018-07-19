using System.ComponentModel.DataAnnotations;

namespace LibraryExercise.Models
{
    public class Book : LibraryAsset            
    {
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string DeweyIndex { get; set; }
    }
}