namespace OutletControl.Contracts.Shifts;

public class OpenShiftRequest
{
    public int OutletId { get; set; }

    public decimal OpeningCash { get; set; }
}