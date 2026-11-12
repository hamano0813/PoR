namespace PoR.Core.Memory;

public sealed class DolphinStatusChangedEventArgs : EventArgs
{
    public DolphinStatusChangedEventArgs(
        DolphinStatus oldStatus,
        DolphinStatus newStatus,
        string? message = null)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Message = message;
    }

    public DolphinStatus OldStatus { get; }

    public DolphinStatus NewStatus { get; }

    public string? Message { get; }
}
