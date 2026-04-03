
//(c8

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BurnyDev.UMMORPG.AddonCustomVehicle
{
    public class UIShowToolTipVehicle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject tooltipPrefab;
        [TextArea(1, 30)] public string text = "";

        // instantiated tooltip
        GameObject current;

        void CreateToolTip()
        {
            // instantiate
            current = Instantiate(tooltipPrefab, Vector3.zero, Quaternion.identity);

            // put to foreground
            current.transform.SetParent(GameObject.Find("Canvas").transform, false); // canvas
            current.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z);
            current.transform.SetAsLastSibling(); // last one means foreground

            // set text immediately. don't wait for next Update(), otherwise there
            // is 1 frame with a small tooltip without a text which is odd.
            current.GetComponentInChildren<Text>().text = text;
        }

        void ShowToolTip(float delay)
        {
            Invoke(nameof(CreateToolTip), delay);
        }

        // helper function to check if the tooltip is currently shown
        // -> useful to only calculate item/skill/etc. tooltips when really needed
        public bool IsVisible() => current != null;

        void DestroyToolTip()
        {
            // stop any running attempts to show it
            CancelInvoke(nameof(CreateToolTip));

            // destroy it
            Destroy(current);
        }

        public void OnPointerEnter(PointerEventData d)
        {
            ShowToolTip(0.5f);
        }

        public void OnPointerExit(PointerEventData d)
        {
            DestroyToolTip();
        }

        void Update()
        {
            // always copy text to tooltip. it might change dynamically when
            // swapping items etc., so setting it once is not enough.
            if (current) current.GetComponentInChildren<Text>().text = text;
        }

        void OnDisable()
        {
            DestroyToolTip();
        }

        void OnDestroy()
        {
            DestroyToolTip();
        }
    }
}
