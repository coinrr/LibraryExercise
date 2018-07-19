using System.ComponentModel.DataAnnotations;

namespace LibraryExercise.Models
{
    public class Video : LibraryAsset
    {
        [Required]
        public string Director { get; set; }
    }
}