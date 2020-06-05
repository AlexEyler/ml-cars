using System;
using System.Collections.Generic;
using System.Linq;
using EasyRoads3Dv3;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(RCC_CarControllerV3))]
public class NewCarAgent : CarAgent
{
    private RCC_CarControllerV3 carControllerV3;
    private Vector3 rigidStartPosition;
    private Quaternion rigidStartRotation;
    private float orgSteerAngle;
    private float orgSpeed;

    void Start()
    {
        this.carControllerV3 = this.GetComponent<RCC_CarControllerV3>();
        this.startPosition = this.carControllerV3.transform.position;
        this.startRotation = this.carControllerV3.transform.rotation;
        this.rigidStartPosition = this.carControllerV3.rigid.transform.localPosition;
        this.rigidStartRotation = this.carControllerV3.rigid.transform.localRotation;
        this.orgSteerAngle = this.carControllerV3.steerAngle;
        this.orgSpeed = this.carControllerV3.speed;

        var roadNetwork = new ERRoadNetwork();
        this.road = roadNetwork.GetRoadByGameObject(RoadObject);
        this.spline = this.road.GetSplinePointsCenter();
        this.BuildSplinePointQueue();
    }

    protected override Rigidbody Rigidbody => this.carControllerV3.rigid;

    public override void OnEpisodeBegin()
    {
        this.BuildSplinePointQueue();
        if (this.EnableMovingTarget)
        {
            this.UpdateTargetPosition();
        }

        // Reset values in car
        this.carControllerV3.gasInput = 0;
        this.carControllerV3.gasInput = 0f;
        this.carControllerV3.steerInput = 0f;
        this.carControllerV3.brakeInput = 0f;
        this.carControllerV3.clutchInput = 0f;
        this.carControllerV3.handbrakeInput = 0f;
        this.carControllerV3.boostInput = 0f;
        this.carControllerV3.speed = this.orgSpeed;
        this.carControllerV3.steerAngle = this.orgSteerAngle;
        this.carControllerV3.rigid.ResetCenterOfMass();
        this.carControllerV3.rigid.ResetInertiaTensor();
        this.carControllerV3.rigid.angularVelocity = Vector3.zero;
        this.carControllerV3.rigid.velocity = Vector3.zero;
        this.carControllerV3.transform.position = this.startPosition;
        this.carControllerV3.transform.rotation = this.startRotation;
        this.carControllerV3.rigid.transform.localPosition = this.rigidStartPosition;
        this.carControllerV3.rigid.transform.localRotation = this.rigidStartRotation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.ForwardVelocity);
        sensor.AddObservation(this.DistanceToClosestSplinePoint);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        this.carControllerV3.gasInput = Mathf.Clamp(vectorAction[0], 0f, 1f);
        this.carControllerV3.steerInput = Mathf.Clamp(vectorAction[1], -1f, 1f);
        this.carControllerV3.brakeInput = Mathf.Clamp(vectorAction[2], 0f, 1f);
        this.carControllerV3.clutchInput = Mathf.Clamp(vectorAction[3], 0f, 1f);
        this.carControllerV3.handbrakeInput = Mathf.Clamp(vectorAction[4], 0f, 1f);
        this.carControllerV3.boostInput = Mathf.Clamp(vectorAction[5], 0f, 1f);

        // Add rewards
        // Encourage forward movement
        if (this.ForwardVelocity > 0)
        {
            this.AddReward(this.ForwardVelocity / 5f);
        }
        else
        {
            this.AddReward(4f * this.ForwardVelocity);
        }

        // Encourage moving at all
        if (Math.Abs(this.ForwardVelocity) < 1e-5f)
        {
            this.AddReward(-0.1f);
        }

        if (this.StepCount > 50_000 || this.IsOffRoad)
        {
            this.SetReward(-500f);
            this.EndEpisodeV2();
        }

        if (this.DistanceToTarget < 15f)
        {
            if (EnableMovingTarget)
            {
                this.SplinePointQueue.Dequeue();

                this.AddReward(100f);

                if (!this.SplinePointQueue.Any())
                {
                    this.SuccessfulCompletions++;
                    this.AddReward(100f);
                    this.EndEpisodeV2();
                }
                else
                {
                    this.UpdateTargetPosition();
                }
            }
            else
            {
                this.SuccessfulCompletions++;
                this.AddReward(this.SplinePointQueue.Count * 100f + 100f);
                this.EndEpisodeV2();
            }
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
        actionsOut[2] = Input.GetAxis("Jump");
        actionsOut[3] = Input.GetAxis("Clutch");
        actionsOut[4] = Input.GetAxis("Handbrake");
        actionsOut[5] = Input.GetAxis("Boost");
    }
}