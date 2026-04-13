namespace PressSubscription.Models;

public class Subscription
{
    public int Id { get; set; }

    public int SubscriberId { get; set; }
    public Subscriber Subscriber { get; set; } = null!;

    public int PublicationId { get; set; }
    public Publication Publication { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public int Months { get; set; }
}