namespace Inventory_Service.DTOs
{
    public class DeleteProductDTO
    {
        public required int Id { get; set; }
        public required int PostedBy { get; set; }
    }
}
