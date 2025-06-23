using System.ComponentModel.DataAnnotations;

namespace ST10092132.Models;

public class EventType
{
    public int EventTypeId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Event Type")]
    public string EventTypeName { get; set; } = null!;

    public virtual ICollection<Event>? Events { get; set; }
}
