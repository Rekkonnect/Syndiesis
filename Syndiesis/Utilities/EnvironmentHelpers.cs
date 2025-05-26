namespace Syndiesis.Utilities;

public static class EnvironmentHelpers
{
    public static bool IsRelease()
    {
#if DEBUG
        return false;
#else
        return true;
#endif
    }
}
