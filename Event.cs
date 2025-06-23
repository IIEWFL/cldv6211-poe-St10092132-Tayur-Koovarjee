using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10092132.Models;

public partial class Event
{
    public int EventId { get; set; }

    [Required(ErrorMessage = "Event name is required")]
    [Display(Name = "Event Name")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Event name must be 3-100 characters")]
    public string EventName { get; set; } = null!;

    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event Date")]
    [DataType(DataType.DateTime)]
    [FutureDate(ErrorMessage = "Event date must be in the future")]
    public DateTime EventDate { get; set; }

    [Display(Name = "Description")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    // New field for event type categorization
    [Required(ErrorMessage = "Event type is required")]
    [Display(Name = "Event Type")]
    public int EventTypeId { get; set; }

    // Navigation property
    [ForeignKey("EventTypeId")]
    [Display(Name = "Category")]
    public virtual EventType? EventType { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

// Custom validation attribute for future dates
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date < DateTime.Now)
            {
                return new ValidationResult(ErrorMessage);
            }
        }
        return ValidationResult.Success;
    }
}
