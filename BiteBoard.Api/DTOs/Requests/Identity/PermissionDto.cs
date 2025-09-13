namespace BiteBoard.API.DTOs.Requests.Identity
{
    public class PermissionDto
    {
        public string RoleId { get; set; }
        public IList<RoleClaimsDto> RoleClaims { get; set; }

        public PermissionDto()
        {
            RoleClaims = new List<RoleClaimsDto>();
        }
    }
}
