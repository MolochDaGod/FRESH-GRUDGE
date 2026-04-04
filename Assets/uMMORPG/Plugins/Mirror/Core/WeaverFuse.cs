// Mirror's Weaver replaces WeaverFuse.Weaved() to return 'true' after
// processing. If weaving never ran, it returns 'false' and NetworkServer/
// NetworkClient throw an early exception with a clear message.
// This stub always returns true for the uMMORPG bundled Mirror build.
namespace Mirror
{
    public static class WeaverFuse
    {
        // Weaver replaces this with 'return true' after processing.
        // Returning true here because uMMORPG uses the bundled Weaver
        // which runs automatically on compile.
        public static bool Weaved() => true;
    }
}
