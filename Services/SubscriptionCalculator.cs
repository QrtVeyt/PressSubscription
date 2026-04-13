using PressSubscription.Models;

namespace PressSubscription.Services;

public static class SubscriptionCalculator
{
    public static void Calculate(Subscription s)
    {
        s.BaseCost = s.Publication.PricePerMonth * s.Months;

        s.DeliveryCost = s.BaseCost * 0.01m;   // 1%
        s.Vat = s.BaseCost * 0.18m;            // 18%

        s.TotalCost = s.BaseCost + s.DeliveryCost + s.Vat;
    }
}