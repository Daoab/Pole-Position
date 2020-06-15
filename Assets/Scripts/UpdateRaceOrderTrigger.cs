using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateRaceOrderTrigger : MonoBehaviour
{
    PolePositionManager polePositionManager;

    // Start is called before the first frame update
    void Start()
    {
        polePositionManager = FindObjectOfType<PolePositionManager>();
    }

    private void OnTriggerStay(Collider other)
    {
        
    }
}
