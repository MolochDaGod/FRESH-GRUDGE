/// Partial Player extension — adds the classTree component reference.
///
/// HOW TO USE:
///   1. Add the PlayerClassTree component to each Player prefab.
///   2. Drag it into the "classTree" field that appears in the Inspector.

public partial class Player
{
#if _IMMOCLASSTREE
    public PlayerClassTree classTree;
#endif
}
