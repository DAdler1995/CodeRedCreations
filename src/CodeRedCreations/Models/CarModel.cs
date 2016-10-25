using System.ComponentModel.DataAnnotations;

namespace CodeRedCreations.Models
{
    public class CarModel
    {
        [Key]
        public int CarId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string TrimLevel { get; set; }
    }
}
