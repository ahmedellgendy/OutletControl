namespace OutletControl.Application.Common.Stock;

public static class StockCostCalculator
{
    public static decimal CalculateNewAverageCost(
        int currentQuantity,
        decimal currentAverageUnitCost,
        int receivedQuantity,
        decimal receivedUnitCost)
    {
        if (receivedQuantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(receivedQuantity),
                "Received quantity must be greater than zero.");

        if (currentQuantity < 0)
            throw new ArgumentOutOfRangeException(
                nameof(currentQuantity));

        var currentValue =
            currentQuantity * currentAverageUnitCost;

        var receivedValue =
            receivedQuantity * receivedUnitCost;

        var newQuantity =
            currentQuantity + receivedQuantity;

        if (newQuantity == 0)
            return 0m;

        return Math.Round(
            (currentValue + receivedValue) / newQuantity,
            4);
    }
}
