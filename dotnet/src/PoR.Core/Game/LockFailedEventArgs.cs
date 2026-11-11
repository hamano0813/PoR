namespace PoR.Core.Game;

public sealed class LockFailedEventArgs : EventArgs
{
    public LockFailedEventArgs(uint address, Exception exception)
    {
        Address = address;
        Exception = exception;
    }

    public uint Address { get; }

    public Exception Exception { get; }
}
