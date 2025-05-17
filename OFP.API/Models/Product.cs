namespace OFP.API.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string ProdName { get; set; } = null!;

        public string? Size { get; set; }

        public string? Description { get; set; }

        public int Price { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
