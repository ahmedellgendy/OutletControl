namespace OutletControl.Contracts.Shifts;

public class CloseShiftRequest
{
    public int ShiftId { get; set; }

    public decimal ActualCash { get; set; }

    public string? Notes { get; set; }
}