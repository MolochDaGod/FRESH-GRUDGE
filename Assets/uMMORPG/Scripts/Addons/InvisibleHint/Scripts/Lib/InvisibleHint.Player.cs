public partial class Player
{
    protected UI_InvisibleHint _UI_InvisibleHint;

    // -----------------------------------------------------------------------------------
    // InvisibleHint_Show
    // -----------------------------------------------------------------------------------
    public void InvisibleHint_Show(string message, float hideAfter)
    {
        if (_UI_InvisibleHint == null)
            _UI_InvisibleHint = FindObjectOfType<UI_InvisibleHint>();

        _UI_InvisibleHint.Show(message);

        if (hideAfter > 0)
            Invoke("InvisibleHint_Hide", hideAfter);
    }

    // -----------------------------------------------------------------------------------
    // InvisibleHint_Hide
    // -----------------------------------------------------------------------------------
    public void InvisibleHint_Hide()
    {
        if (_UI_InvisibleHint == null)
            _UI_InvisibleHint = FindObjectOfType<UI_InvisibleHint>();

        _UI_InvisibleHint.Hide();
    }

    // -----------------------------------------------------------------------------------
}