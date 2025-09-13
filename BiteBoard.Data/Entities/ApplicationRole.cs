using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using System;

namespace BiteBoard.Data.Entities
{
    [MultiTenant]
    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}