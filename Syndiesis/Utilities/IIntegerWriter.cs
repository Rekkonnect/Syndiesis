namespace Syndiesis.Utilities;

internal interface IIntegerWriter
{
    protected static bool NeedsSeparator(int writerLength, int groupLength)
    {
        return groupLength > 0
            && (writerLength % (groupLength + 1) == groupLength);
    }
}
