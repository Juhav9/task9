using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DeviceManager.Models;
using Microsoft.AspNetCore.Identity;

namespace DeviceManager.Data;

public class DeviceManagerAppUser : IdentityUser 
{
    // add navigation properties to user's devices and user roles
    public virtual ICollection<Device>? Devices { get; set;}
    public virtual ICollection<IdentityUserRole<string>>? UserRoles { get; set; }
}
