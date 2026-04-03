#if _SQLITE
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManipulationTools : MonoBehaviour
{
    public static UIManipulationTools singleton;
    public Canvas canvas;

    public TMP_Text nameText;
    [Space(14)]
    public GameObject rotationX;
    [Space(14)]
    public GameObject rotationY;
    [Space(14)]
    public GameObject rotationZ;
    [Space(14)]
    public GameObject positionY;
    [Space(14)]
    public Button killButton;
    [Space(14)]
    public Button deleteButton;
    [Space(14)]
    public Button saveButton;

    Player player;
    GameObject myTarget;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

#if UMMORPG_2D_C || UMMORPG_2D_R
    void Start()
    {
        rotationY.SetActive(false);
        saveButton.gameObject.SetActive(false);
    }
#endif

    public GameObject MyTarget
    {
        get { return myTarget; }
        set
        {
            if (myTarget != value)
            {
                myTarget = value;

                nameText.text = "";
                canvas.enabled = false;
                //Debug.Log("tools[close]");
            }
        }
    }

    public void RotateX(float value)
    {
        player.CmdObjectRotateX(myTarget.gameObject, +value);
    }

    public void RotateY(float value)
    {
        player.CmdObjectRotateY(myTarget.gameObject, +value);
    }

    public void RotateZ(float value)
    {
        player.CmdObjectRotateZ(myTarget.gameObject, +value);
    }

    public void PositionY(float value)
    {
        player.CmdObjectPositionY(myTarget.gameObject, +value);
    }

    public void Kill()
    {
        player.CmdEntityKill(myTarget.gameObject);
    }

    public void Delete()
    {
        player.CmdObjectDelete(myTarget.gameObject);
    }

    public void Save()
    {
        player.CmdObjectSave(myTarget.gameObject);
    }

    public void Show(GameObject target, bool uniqueId = true)
    {
        if (!player) player = Player.localPlayer;
       
        if (player != null)
        {           
            if (player.CheckAccess())
            {
                myTarget = target;
                canvas.enabled = true;

                if (canvas.enabled)
                {
                    if (myTarget != null)
                    {
                        if (uniqueId)
                        {
                            if (myTarget.CompareTag("Monster"))
                            {
                                ButtonsActivation(false, true);
                            }
                            else if (myTarget.CompareTag("Npc"))
                            {
                                ButtonsActivation(false, false);
                            }
                            else if (myTarget.CompareTag("Environment"))
                            {
#if UMMORPG_3D_C || UMMORPG_3D_R
                                ButtonsActivation(true, false);
#else
                                ButtonsActivation(false, false);
#endif
                            }
                        }
                        else
                        {
                            if (myTarget.CompareTag("Monster"))
                            {
                                ButtonsActivation(false, true, false);
                            }
                        }
                        nameText.text = myTarget.name;
                    }
                }
            }
        }
    }

    void ButtonsActivation(bool isActive, bool monster, bool uniqueId = true)
    {
#if UMMORPG_3D_C || UMMORPG_3D_R
        if (uniqueId)
        {
            rotationX.SetActive(isActive);
            rotationZ.SetActive(isActive);
            positionY.SetActive(isActive);
        }
        else
        {
            rotationX.SetActive(false);
            rotationZ.SetActive(false);
            positionY.SetActive(false);
        }
#endif
        killButton.gameObject.SetActive(monster);
        deleteButton.gameObject.SetActive(uniqueId);
#if UMMORPG_3D_C || UMMORPG_3D_R
        saveButton.gameObject.SetActive(uniqueId);
#endif
    }

    public void Hide()
    {
        MyTarget = null;
    }
}

#endif
