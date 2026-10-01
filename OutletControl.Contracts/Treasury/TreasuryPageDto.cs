namespace OutletControl.Contracts.Treasury;

public class TreasuryPageDto
{
    public TreasurySummaryDto Summary { get; set; } = new();

    public List<TreasuryMovementDto> Movements { get; set; } = new();
}
