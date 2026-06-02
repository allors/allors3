using System;
using Nuke.Common.Tooling;

public static class IProcessExtensions
{
    private const int SUCCESS = 0;

    public static void ThrowOnFailure(this IProcess @this)
    {
        if (!@this.WaitForExit())
        {
            throw new Exception($"{@this}");
        }

        if (@this.ExitCode != SUCCESS)
        {
            throw new Exception($"{@this.Output}");
        }
    }

    public static void KillTree(this IProcess @this)
    {
        try
        {
            // IProcess.Kill() only kills the direct child (e.g. the npm wrapper),
            // orphaning grandchildren such as the Angular dev server.
            System.Diagnostics.Process.GetProcessById(@this.Id).Kill(entireProcessTree: true);
        }
        catch (Exception)
        {
            // Already exited.
        }
    }
}
