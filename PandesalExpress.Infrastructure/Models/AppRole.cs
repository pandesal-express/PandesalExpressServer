using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PandesalExpress.Infrastructure.Models;

[Table("roles")]
public sealed class AppRole : IdentityRole<Ulid>
{
    public AppRole() { Id = Ulid.NewUlid(); }

    public AppRole(string roleName) : base(roleName) { Id = Ulid.NewUlid(); }
}
