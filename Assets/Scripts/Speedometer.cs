using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    public Rigidbody theCar;
    public float maxSpeed = 0.0f;
    public float minSpeedPointerAngle;
    public float maxSpeedPointerAngle;
    public RectTransform pointerHolder;
    public TMPro.TMP_Text speedLabel;

    // Update is called once per frame
    void FixedUpdate()
    {
        float speed = theCar.velocity.magnitude * 3.6f;
        speedLabel.text = (int)speed + "";
        speedLabel.alignment = TMPro.TextAlignmentOptions.Center;
        pointerHolder.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minSpeedPointerAngle, maxSpeedPointerAngle, speed / maxSpeed));
    }
}
