using System;
using UnityEngine;

public class TriggerForwarder : MonoBehaviour
{
    //Simple script that forwards a child trigger enter command to the parent
    private void OnTriggerEnter(Collider other)
    {
        transform.parent.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
    }
}
