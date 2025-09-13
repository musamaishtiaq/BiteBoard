using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using BiteBoard.Data.Enums;
using System;

namespace BiteBoard.Data.Entities
{
    [MultiTenant]
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] ProfilePicture { get; set; }
        public bool IsActive { get; set; } = false;
    }
}