using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerEvent : MonoBehaviour
{

    [SerializeField] string colliderTagToDetect = "none";
    bool _inTrigger = false;
    [SerializeField] string _triggerType;
    public delegate void Triggered(bool inTrigger, string triggerType);
    public static event Triggered OnTrigger;
    public delegate void bkdsifjh();
    public static event bkdsifjh stuff;

    public bool IsColliding
    {
        get
        {
            return _inTrigger;
        }
        set
        {
            if (_inTrigger == value)
            {
                return;
            }

            _inTrigger = value;

            if (OnTrigger != null)
            {
                OnTrigger(_inTrigger, _triggerType);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == colliderTagToDetect)
        {
            IsColliding = true;
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.transform.tag == colliderTagToDetect)
        {
            IsColliding = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == colliderTagToDetect)
        {
            IsColliding = false;
        }
    }
}
