using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    //Movement
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float minAngle;
    [SerializeField]
    private float maxAngle;
    [SerializeField]
    private CharacterController controller;

    public bool gyro;
    public bool accelerometer;

    //Rotation
    private float initialYAngle = 0f;
    private float appliedGyroYAngle = 0f;
    private float appliedGyroXAngle = 0f;
    private float calibrationYAngle = 0f;
    private Transform rawGyroRotation;

    //Settings
    [SerializeField]
    private float smooting = 0.1f;

    //Shooting
    [Header("TextMeshProGUI")]
    [SerializeField]
    private TextMeshProUGUI reloadTimer;
    [SerializeField]
    private TextMeshProUGUI reloadText;
    [SerializeField]
    private TextMeshProUGUI ammoAmountText;
    [SerializeField]
    private Image colourIndicator;

    [Header("Button Variables")]
    public int ammo = 1;
    public bool startTimer = false;
    float timer = 2;
    bool canReload = false;

    //Start fucntion with a timer
    private IEnumerator Start()
    {
        //Reload GUI off
        reloadText.enabled = false;

        //enable gyroscope on Android device (isn't automatically enabled on Android)
        Input.gyro.enabled = true;
        //Set framerate for every device
        Application.targetFrameRate = 60;
        //Set initialYAngle variable
        initialYAngle = transform.eulerAngles.y;

        //Create GameObject for future Gyro references, stores values of phone
        rawGyroRotation = new GameObject("GyroRaw").transform;
        rawGyroRotation.position = transform.position;
        rawGyroRotation.rotation = transform.rotation;

        yield return new WaitForSeconds(1);

        StartCoroutine(CalibrateYAngle());
    }

    void Update()
    {
        #region Timer
        //Count down
        if (startTimer)
        {
            timer -= Time.deltaTime;
        }

        //TMPro elements
        int timerInt = (int)timer;
        timerInt++;
        reloadTimer.text = timerInt.ToString();
        ammoAmountText.text = ammo.ToString();

        //Reset Timer
        if (timer <= 0)
        {
            ReloadVariables(true);
            startTimer = false;
            timer = 2;
        }
        #endregion

        //Measure the Shake and Reload
        if (Input.acceleration.magnitude > 3f && canReload)
        {
            Reload(); 
        }

        //Test how these values are measured
        Vector3 testMove = new Vector3(Input.acceleration.x, 0, Input.acceleration.z);

        //Sprint if there is a sudden increase in acceleration along the Z axes of the phone and adjust speed accordingly
        if (accelerometer)
        {
            if (testMove.z < -0.60f)
            {
                speed = 50;
            }
            else
            {
                speed = 5;
            }
        }

        //Increase speed if angle is correct
        if (gyro)
        {
            if (appliedGyroXAngle > minAngle && appliedGyroXAngle < maxAngle)
            {
                speed = 100;
                colourIndicator.color = Color.green;
            }
            else if (appliedGyroXAngle > (minAngle - 3f) && appliedGyroXAngle < minAngle || appliedGyroXAngle > maxAngle && appliedGyroXAngle < (maxAngle + 3f))
            {
                speed = 25;
                colourIndicator.color = Color.yellow;
            }
            else
            {
                speed = 5;
                colourIndicator.color = Color.red;
            }
        }

        #region Movement
        //If acceleration is measured in the phone, using the Accelerometer as input, and applies speed * time so the player can move forward with an even pace
        Vector3 move = new Vector3(Input.acceleration.x * speed * Time.deltaTime, 0, -Input.acceleration.z * speed * Time.deltaTime);

        //.TransformDirection -> chages the LOCAL direction into the WORLD direction
        Vector3 rotMovement = transform.TransformDirection(move);
        controller.Move(rotMovement);
        #endregion

        # region Rotation
        //Gets Gyroscope Euler rotational values from phone as reference point
        ApplyGyroRotation();
        //Cannot write these two functions as one since "appliedGyroYAngle" variable needs to be solved first so it can later be used in the CalibrateYAngle IEnumerator funtion, which gives the value for "calibrationYAngle" which is used in the next function
        ApplyCalibration();

        //Match player rotation with phones gyroscope rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, rawGyroRotation.rotation, smooting);
        #endregion
    }

    //IEnumerator is a type of function that can be halted by a yield statement
    IEnumerator CalibrateYAngle()
    {
        //Smoothing changes here so the player rotates quicker towards the correct angle
        float tempSmoothing = smooting;
        smooting = 1;
        //Get the 
        calibrationYAngle = appliedGyroYAngle - initialYAngle;
        yield return null;
        //reset the smoothing
        smooting = tempSmoothing;
    }

    void ApplyGyroRotation()
    {
        //Measure the .attitude (orientation in space) from the Gyro
        rawGyroRotation.rotation = Input.gyro.attitude;
        //Rotate z axes 180 degrees relative to Self
        rawGyroRotation.Rotate(0f, 0f, 180f, Space.Self);
        //Rotate x & y axes 90 and 180 degrees respectively relative to World
        rawGyroRotation.Rotate(90f, 180f, 0f, Space.World);
        //Set y orientation of phone
        appliedGyroYAngle = rawGyroRotation.eulerAngles.y;
        appliedGyroXAngle = rawGyroRotation.eulerAngles.x;
    }

    void ApplyCalibration()
    {
        //Rotate - y axes relative to World
        rawGyroRotation.Rotate(0f, -calibrationYAngle, 0f, Space.World);
    }
    void Reload()
    {
        ReloadVariables(false);
        ammo++;
        Debug.Log("Reload");
    }

    void ReloadVariables(bool value)
    {
        canReload = value;
        reloadText.enabled = value;
    }
}
