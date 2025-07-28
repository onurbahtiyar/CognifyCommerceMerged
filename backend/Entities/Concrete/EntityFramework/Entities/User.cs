using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Entities.Concrete.EntityFramework.Entities;

[Table("User")]
[Index("Guid", Name = "UQ_User_Guid", IsUnique = true)]
public class User : IEntity {
    [Key]
    public int Id { get; set; }

    public Guid Guid { get; set; }

    [StringLength(75)]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? Password { get; set; }

    [StringLength(75)]
    public string? FirstName { get; set; }

    [StringLength(75)]
    public string? LastName { get; set; }

    [StringLength(75)]
    public string? Username { get; set; }

    [StringLength(50)]
    public string? PhoneNumber { get; set; }


    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastLoginDate { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
