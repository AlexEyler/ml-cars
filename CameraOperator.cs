using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOperator : MonoBehaviour
{
    public List<NewCarAgent> CarAgents;
    public float Speed;
    public float ZoomSpeed;

    private NewCarAgent linkedCarAgent = null;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private Vector3 velocity;

    void Start()
    {
        this.startingPosition = this.transform.position;
        this.startingRotation = this.transform.rotation;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            this.linkedCarAgent = null;
        }
        else
        {
            for (int i = 0; i < this.CarAgents.Count; i++)
            {
                if (Input.GetKey(KeyCode.Alpha1 + i))
                {
                    this.linkedCarAgent = this.CarAgents[i];
                    break;
                }
            }
        }

        if (this.linkedCarAgent != null)
        {
            var targetPosition = this.linkedCarAgent.transform.TransformPoint(0, 50, 0);
            this.transform.position = Vector3.SmoothDamp(this.transform.position, targetPosition, ref this.velocity, 0.2f);
        }
        else
        {
            var magnitude = this.Speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.D))
            {
                this.transform.Translate(new Vector3(magnitude, 0, 0));
            }

            if (Input.GetKey(KeyCode.A))
            {
                this.transform.Translate(new Vector3(-magnitude, 0, 0));
            }

            if (Input.GetKey(KeyCode.S))
            {
                this.transform.Translate(new Vector3(0, -magnitude, 0));
            }

            if (Input.GetKey(KeyCode.W))
            {
                this.transform.Translate(new Vector3(0, magnitude, 0));
            }

            this.transform.Translate(new Vector3(0, 0, this.ZoomSpeed * Input.mouseScrollDelta.y));
        }
    }
}
