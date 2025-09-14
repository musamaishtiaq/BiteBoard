namespace BiteBoard.Api.DTOs.Requests
{
    public class OrderItemModifierDto
    {
        public string Id { get; set; }
        public string OrderItemId { get; set; }
        public string ModifierOptionId { get; set; }
    }
}
