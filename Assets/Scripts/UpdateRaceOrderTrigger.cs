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

    //Cada coche tiene un trigger para detectar cuándo sucede un adelantamiento, 
    //y en ese momento se actualiza el orden de la carrera.
    //Esto se hace para que no se esté comprobando continuamente en qué posición están los corredores
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            polePositionManager.UpdateRaceProgress();
        }
    }
}
