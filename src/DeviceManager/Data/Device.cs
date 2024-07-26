using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DeviceManager.Data;


public class Device
{
    // add properties 
    public int Id { get; set; }
    [Required]
    [MaxLength(450)]
    public string? UserId { get; set; }
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    public string? Description { get; set; }
    [DataType(DataType.Date)]
    public DateTime? DateAdded { get; set; }
    // add navigation property to user who's device this is
    public virtual DeviceManagerAppUser? User { get; set; }
}
