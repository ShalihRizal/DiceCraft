using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        var draggable = other.GetComponent<DiceDrag>();
        if (draggable != null)
        {
            draggable.SetTrashZoneStatus(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var draggable = other.GetComponent<DiceDrag>();
        if (draggable != null)
        {
            draggable.SetTrashZoneStatus(false);
        }
    }
}
