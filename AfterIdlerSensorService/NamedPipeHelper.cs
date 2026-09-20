using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace AfterIdlerSensorService;

internal static class NamedPipeHelper
{
    public static NamedPipeServerStream CreateServer(
        string pipeName)
    {
        PipeSecurity security = new PipeSecurity();

        SecurityIdentifier users =
            new SecurityIdentifier(
                WellKnownSidType.AuthenticatedUserSid,
                null);

        security.AddAccessRule(
            new PipeAccessRule(
                users,
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            pipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            4096,
            4096,
            security);
    }
}