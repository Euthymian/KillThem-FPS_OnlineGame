using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    PlayerController pc;
    PhotonView pv;
    [SerializeField] Transform cam;
    [SerializeField] Transform playerHip;
    [SerializeField] LayerMask grappleableLayerMask;
    [SerializeField] LineRenderer lr;

    [Header("Grappling")]
    [SerializeField] float maxGrappleDistance;
    [SerializeField] float grappleDelayTime;
    [SerializeField] float overShootYAxis;

    Vector3 grapplePoint;

    [Header("Cooldown")]
    [SerializeField] float grappleCooldown;
    float grappleCooldownTimer;

    [Header("Keycode")]
    KeyCode grappleKey = KeyCode.Q;

    bool isGrappling;

    private void Start()
    {
        pc = GetComponent<PlayerController>();
        pv = GetComponent<PhotonView>();
        lr.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();

        if(grappleCooldownTimer > 0) grappleCooldownTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        // Update the start point of maneuver wire
        if (isGrappling)
        {
            if (pv.IsMine)
                lr.SetPosition(0, playerHip.position);
            pv.RPC(nameof(RPC_UpdateWireHip), RpcTarget.Others, playerHip.position);
        }
    }

    [PunRPC]
    void RPC_UpdateWireHip(Vector3 hipPos)
    {
        lr.SetPosition(0, hipPos);
    }

    void StartGrapple()
    {
        if (grappleCooldownTimer > 0) return;

        isGrappling = true;
        pc.needFreezeMove = true;

        if (pv.IsMine)
        {
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxGrappleDistance, grappleableLayerMask))
            {
                grapplePoint = hit.point;

                Invoke(nameof(ExecuteGrapple), grappleDelayTime);
            }
            else
            {
                grapplePoint = cam.position + cam.forward * maxGrappleDistance;

                Invoke(nameof(StopGrapple), grappleDelayTime);
            }

            pv.RPC(nameof(RPC_EnableWire), RpcTarget.Others, grapplePoint);
        }

        lr.enabled = true;
        // Set end point of maneuver wire to grapplePoint
        lr.SetPosition(1, grapplePoint);
    }

    [PunRPC]
    void RPC_EnableWire(Vector3 grapplePoint)
    {
        lr.enabled = true;
        // Set end point of maneuver wire to grapplePoint
        lr.SetPosition(1, grapplePoint);
    }

    void ExecuteGrapple()
    {
        pc.needFreezeMove = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overShootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overShootYAxis;

        pc.JumpToPos(grapplePoint, highestPointOnArc);
        grappleCooldownTimer = grappleCooldown;

        //Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        pv.RPC(nameof(RPC_DisableWire), RpcTarget.Others);
        pc.needFreezeMove = false;

        isGrappling = false;


        lr.enabled = false;
    }

    [PunRPC]
    void RPC_DisableWire()
    {
        lr.enabled = false;
    }
}
