namespace BiteBoard.Api.DTOs.Requests
{
    public class DealItemDto
    {
        public string Id { get; set; }
        public string MenuItemId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
