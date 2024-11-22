using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Highlighters.HighlighterTrigger;

public class GroundCheck : MonoBehaviour
{
    [Header("BoxCast Property")]
    [SerializeField] private Vector3 BoxSize;
    [SerializeField] private float maxDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private string groundTag = "Ground";

    [SerializeField] public float _RayCastDirY = 5f;


    [SerializeField]
    private Color _rayColor = Color.red;

    [Header("Debug")]
    [SerializeField] private bool drawGizmo;

    Vector3 RayStartPoint;

    private void OnDrawGizmos()
    {
        //float maxDistance = 100;

        RaycastHit hit;
        RayStartPoint = new Vector3(transform.position.x, transform.position.y - _RayCastDirY, transform.position.z);  
        // Physics.BoxCast (레이저를 발사할 위치, 사각형의 각 좌표의 절판 크기, 발사 방향, 충돌 결과, 회전 각도, 최대 거리)
        bool isHit = Physics.BoxCast(RayStartPoint, BoxSize/ 2, (-transform.up), out hit, transform.rotation, maxDistance);

        Gizmos.color = Color.red;
        if (isHit)
        {
            Gizmos.DrawRay(RayStartPoint, (-transform.up) * hit.distance);
            Gizmos.DrawWireCube(RayStartPoint + (-transform.up) * hit.distance, BoxSize);
        }
        else
        {
            Gizmos.DrawRay(RayStartPoint, (-transform.up) * maxDistance);
        }

    }

    public bool IsGrounded()
    {
        RaycastHit hit;
        RayStartPoint = new Vector3(transform.position.x, transform.position.y - _RayCastDirY, transform.position.z);
        return Physics.BoxCast(RayStartPoint, BoxSize / 2, (-transform.up), out hit, transform.rotation, maxDistance, groundLayer);
    }
}
