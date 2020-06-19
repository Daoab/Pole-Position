using UnityEngine;

[System.Serializable]
//Parámetros de las ruedas de los coches
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}