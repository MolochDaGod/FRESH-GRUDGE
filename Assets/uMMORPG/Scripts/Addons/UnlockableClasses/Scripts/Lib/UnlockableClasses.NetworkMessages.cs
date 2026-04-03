#if _iMMOUNLOCKABLECLASSES
using Mirror;

// ---------------------------------------------------------------------------------------
// ClassesAvailableMsg
// ---------------------------------------------------------------------------------------
public partial struct ClassesAvailableMsg : NetworkMessage
{
    public string[] unlockedClasses;
}
#endif