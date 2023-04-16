using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
   
    [Header("Steering")]
    [SerializeField] float maxSteerAngle = 45f;
    [SerializeField] float targetSteerAngle = 0f;

    [Header("Paths")]
    public Transform path;
    private List<Transform> nodes;
    private int currentNode = 0;

    [Header("Wheels")]
    [SerializeField] WheelCollider leftFrontWheel;
    [SerializeField] WheelCollider rightFrontWheel;
    [SerializeField] WheelCollider leftRearWheel;
    [SerializeField] WheelCollider rightRearWheel;

    [Header("Engine")]
    [SerializeField] float maxMotorTorque = 200f;
    [SerializeField] float permanentMaxMotorTorque = 200f;
    [SerializeField] bool isBraking;
    [SerializeField] float turningSpeed = 4f;
    [SerializeField] bool reverse = false;
    [SerializeField] float minBrakingDistance = 20f;
    [SerializeField] float maxReverseTorque = -80f;
    [SerializeField] float maxSpeed = 14f;

    [Header("Body")]
    [SerializeField] Vector3 centerOfMass = new Vector3(0, 0.1f, 0f);
    private Rigidbody rb;
    private Transform terrain;

    [Header("Sensors")]
    public float sensorLength = 8f;
    public Vector3 frontSensorPosition = new Vector3(0, 0.2f, 1.5f);
    public float frontSideSensorPosition = .5f;
    public float frontSensorAngle = 20f;
    public bool avoiding = false;

    void Start()
    {

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        rb.position = new Vector3(0, 0.55f, 3.9f);

        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();
       

        for (int i = 0; i < pathTransforms.Length; i++)
        {
           
            if (pathTransforms[i] != path.transform)
            {
                nodes.Add(pathTransforms[i]);
            }
        }
       
    }

    private void FixedUpdate()
    {
        Sensors();
        RunSteer();
        Drive();
        CheckWayPointDistance();
        Brake();
        LerpToSteerAngle();
        Reverse();
    }

    private void RunSteer()
    {
        if (avoiding) return;
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;

        targetSteerAngle = newSteer;

    }

    private void Drive()
    {
        

        if (!isBraking && rb.velocity.magnitude < maxSpeed)
        {
            leftRearWheel.motorTorque = maxMotorTorque;
            rightRearWheel.motorTorque = maxMotorTorque;
        }
        else
        {
            leftRearWheel.motorTorque = 0f;
            rightRearWheel.motorTorque = 0f;
        }

    }

    private void CheckWayPointDistance()
    {
        if (Vector3.Distance(transform.position, nodes[currentNode].position) < 5f)
        {
            if (currentNode == nodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }

        if (Vector3.Distance(transform.position, nodes[currentNode].position) < minBrakingDistance && rb.velocity.magnitude > maxSpeed)
        {
            isBraking = true;
        }
        else
        {
            isBraking = false;
        }

       
        Debug.Log("Current Node "+currentNode + "NC " + nodes.Count);
    }

    private void Brake()
    {

        if (isBraking && rb.velocity.magnitude > maxSpeed)
        {
            rightRearWheel.brakeTorque = maxMotorTorque *4f;
            leftRearWheel.brakeTorque = maxMotorTorque *4f;
        }
        else
        {
            rightRearWheel.brakeTorque = 0f;
            leftRearWheel.brakeTorque = 0f;
        }
    }

    private void Sensors()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position;
        sensorStartPos += transform.forward * frontSensorPosition.z;
        sensorStartPos += transform.up * frontSensorPosition.y;
        float avoidingMultiplier = 0f;
        int avoidFromCenter = 0;
        avoiding = false;

        // Front center sensor
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
            }
        }
        

        // Right front sensor
        sensorStartPos += transform.right * frontSideSensorPosition;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidingMultiplier -= 1f;
                avoidFromCenter++;
            }
        }
        
        // Right angle sensor
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength + 3))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidingMultiplier -= .3f;
            }
        }


        // Left front sensor
        sensorStartPos -= transform.right * frontSideSensorPosition * 2;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidingMultiplier += 1f;
                avoidFromCenter++;
            }
        }
       
        // Left angle sensor
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength + 3))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidingMultiplier += .3f;
            }
        }

  

        if (avoidingMultiplier == 0 || avoidFromCenter > 1)
        {
            // Front center sensor
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(sensorStartPos, hit.point);
                    avoiding = true;
                    if(hit.normal.x < 0)
                    {
                        avoidingMultiplier = 1f;
                    }else
                    {
                        avoidingMultiplier = -1f;
                    }
                }
            }
        }

        if (avoiding)
        {
            targetSteerAngle = maxSteerAngle * avoidingMultiplier;
        }
       

    }

    private void LerpToSteerAngle()
    {
        leftFrontWheel.steerAngle = Mathf.Lerp(leftFrontWheel.steerAngle, targetSteerAngle, Time.deltaTime * turningSpeed);
        rightFrontWheel.steerAngle = Mathf.Lerp(rightFrontWheel.steerAngle, targetSteerAngle, Time.deltaTime * turningSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            reverse = true;
            terrain = collision.gameObject.transform;
        }else
        {
            terrain = null;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        reverse = false;
        

    }


    private void Reverse()
    {
        if (reverse && (terrain != null &&  Vector3.Distance(transform.position, terrain.gameObject.transform.position) < 1))
        {
            targetSteerAngle = -targetSteerAngle;
            maxMotorTorque = maxReverseTorque;
        }
        if(terrain != null && Vector3.Distance(transform.position, terrain.gameObject.transform.position) > 6)
        {
            reverse = false;
            maxMotorTorque = permanentMaxMotorTorque;
            terrain = null;
        }
    }
}
