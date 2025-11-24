using UnityEngine;
using UnityEngine.EventSystems;

public class TrashUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging)
        {
            GameObject draggedObj = eventData.pointerDrag;
            if (draggedObj != null)
            {
                DiceDrag diceDrag = draggedObj.GetComponent<DiceDrag>();
                if (diceDrag != null)
                {
                    diceDrag.SetTrashZoneStatus(true);
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.dragging)
        {
            GameObject draggedObj = eventData.pointerDrag;
            if (draggedObj != null)
            {
                DiceDrag diceDrag = draggedObj.GetComponent<DiceDrag>();
                if (diceDrag != null)
                {
                    diceDrag.SetTrashZoneStatus(false);
                }
            }
        }
    }
}
