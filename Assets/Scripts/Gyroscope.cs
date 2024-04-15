using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gyroscope : MonoBehaviour
{
    // Start is called before the first frame update

    private Vector3 rotation;
    void Start()
    {
        //enable gyroscope on Android device (isn't automatically enabled on Android)
        Input.gyro.enabled = true;

        //Zodat alle rotatie waarden 0 zijn voordat het feest begint
        rotation = Vector3.zero;
        
    }

    // Update is called once per frame
    void Update()
    {
        //
        rotation.z = Input.gyro.rotationRateUnbiased.z;
        rotation.x = -Input.gyro.rotationRateUnbiased.x;
        //.attitude = orientation in space
        //transform.rotation = Input.gyro.attitude;

        transform.Rotate(rotation.x, 0, rotation.z);
    }
}
