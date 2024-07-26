using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DeviceManager.Models;


public class DeviceViewModel
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
    public DateTime? DateAdded { get; set; }
}
