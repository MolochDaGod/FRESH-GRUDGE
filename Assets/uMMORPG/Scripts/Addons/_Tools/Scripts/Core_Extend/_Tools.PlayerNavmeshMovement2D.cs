#if _iMMO2D && _iMMOTOOLS
using UnityEngine;

public partial class PlayerNavMeshMovement2D
{
    [Header("[-=-[ Disable Moving ]-=-]")]
    public bool disableClickMove;
    public bool disableWASDMove;
#if _iMMOMOBILECONTROLS
    public bool disableJoystickMove;
#endif
}
#endif