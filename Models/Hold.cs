using System;

namespace LibraryExercise.Models
{
    public class Hold
    {
        public int Id { get; set; }
        public LibraryAsset LibraryAsset { get; set; }
        public LibraryCard LibraryCard { get; set; }
        public DateTime HoldPlaced { get; set; }
    }
}