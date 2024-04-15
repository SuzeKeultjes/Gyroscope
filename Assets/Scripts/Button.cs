using System.Collections;
using System.Collections.Generic;
using Unity.Content;
using UnityEngine;

public class Button : MonoBehaviour
{

    PlayerMovement pm;

    // Start is called before the first frame update
    void Start()
    {
        pm = FindAnyObjectByType<PlayerMovement>();
    }

    public void Shoot()
    {
        if (pm.ammo == 1)
        {
            pm.ammo--;
            pm.startTimer = true;
            Debug.Log("Shoot");


            new GameObject("Bullet");

        }
    }
}
