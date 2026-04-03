using System.Collections;
using UnityEngine;
using TMPro;
public partial class MsgFrame : MonoBehaviour
{
    public TMP_Text frameText;
    public SpriteRenderer bubble;
    public SpriteRenderer tips;
    private float additionalWitdh;

    // -----------------------------------------------------------------------------------
    // Start
    // -----------------------------------------------------------------------------------
#if _CLIENT
    private void Awake()
    {
        additionalWitdh = bubble.size.x;
    }


    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
#endif
    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public void ShowMessage(string msg)
    {
        if (msg != "")
        {
            //msg = msg.Replace(System.Environment.NewLine, " ");
            frameText.gameObject.SetActive(true);
            frameText.text = msg;
            //bubble.size = new Vector2(frameText.GetPreferredValues(msg + additionalWitdh).x, bubble.size.y);
            bubble.size = new Vector2(frameText.GetPreferredValues(msg + additionalWitdh).x, bubble.size.y);
            bubble.gameObject.SetActive(true);
            tips.gameObject.SetActive(true);
            
            StartCoroutine("ShowMsgFrameSequence");
        }
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    private IEnumerator ShowMsgFrameSequence()
    {
        yield return new WaitForSeconds((frameText.text.Length / 10) + 2.5f);
        //animator.SetBool("SHOW_MSG", false);
        
        yield return new WaitForSeconds(0.3f);
        bubble.gameObject.SetActive(false);
        tips.gameObject.SetActive(false);
        frameText.text = "";
        frameText.gameObject.SetActive(false);
    }

    // -----------------------------------------------------------------------------------
    //
    // -----------------------------------------------------------------------------------
    public float GetTextMeshWidth(TextMesh mesh, string txt)
    {
        float width = 0;
        foreach (char symbol in mesh.text)
        {
            CharacterInfo info;
            if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle))
            {
                width += info.advance;
            }
        }
        return width * mesh.characterSize * 0.1f;
    }

    // -----------------------------------------------------------------------------------
}