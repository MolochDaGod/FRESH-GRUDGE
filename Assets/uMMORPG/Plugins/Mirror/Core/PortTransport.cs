// Transports that have a configurable port can inherit from PortTransport
// instead of Transport. NetworkServer and NetworkManagerHUD use this to
// display the port in the HUD and log it on headless server startup.
namespace Mirror
{
    public abstract class PortTransport : Transport
    {
        public abstract ushort Port { get; set; }
    }
}
