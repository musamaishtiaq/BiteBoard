namespace BiteBoard.Api.DTOs.Requests
{
    public class CategoryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
