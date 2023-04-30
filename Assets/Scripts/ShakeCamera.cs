using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeCamera : MonoBehaviour
{
    [Header("Camera Shake Values")]
    [SerializeField] Camera shakyCamera = null;
    [SerializeField] float shakeFrequency = 15;
    [SerializeField] float maxAngle = 3;
    [SerializeField, Range(0.001f, 1)] float maxOffset = 1.5f;
    [SerializeField] bool debugMode = false;

    float camShakeValue = 0;
    float shake, seed;

    bool debugShake;

    private void Awake()
    {
        seed = Random.value;
    }

    private void Update()
    {
        if (debugMode)
        {
            CheckInput();
        }
    }

    void FixedUpdate()
    {
        if (debugMode)
        {
            if (debugShake)
                AddTrauma(.25f, 1);
        }

        CameraShake();
    }

    //intensity must be between 0 and 1
    public void AddTrauma(float duration, float intensityScale, float frequency = int.MinValue, float maxRotAngle = int.MinValue, float maxPosOffset = int.MinValue)
    {
        camShakeValue = duration;
        
        shake = intensityScale; //could normalize this

        if (frequency != int.MinValue)
            shakeFrequency = frequency;
        if (maxRotAngle != int.MinValue)
            maxAngle = maxRotAngle;
        if (maxPosOffset != int.MinValue)
            maxOffset = maxPosOffset;
    }

    void CameraShake()
    {
        if (camShakeValue > 0)
        {
            //Noise calculations
            float angle = maxAngle * shake * (Mathf.PerlinNoise(seed, Time.time * shakeFrequency) * 2 - 1);
            float xOffset = maxOffset * shake * (Mathf.PerlinNoise(seed + 1, Time.time * shakeFrequency) * 2 - 1);
            float yOffset = maxOffset * shake * (Mathf.PerlinNoise(seed + 2, Time.time * shakeFrequency) * 2 - 1);

            //Translational shake
            shakyCamera.transform.localPosition = new Vector3(xOffset, yOffset, 0);
            transform.localPosition = new Vector3(transform.localPosition.x + shakyCamera.transform.localPosition.x, 
                                                  transform.localPosition.y + shakyCamera.transform.localPosition.y, 
                                                  transform.localPosition.z);

            //Rotational shake
            shakyCamera.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, angle);
            transform.localEulerAngles = shakyCamera.transform.localEulerAngles;
            
            shakyCamera.transform.localEulerAngles = Vector3.zero;
            shakyCamera.transform.position = transform.position;

            camShakeValue -= Time.fixedDeltaTime;
        }
    }

    void CheckInput()
    {
        debugShake = Input.GetKeyDown(KeyCode.S);
    }
}
