using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float lookSpeed = 2f;
    [SerializeField] PlayerInputCheck player;
    [SerializeField] CinemachineVirtualCamera virtualCamera;

    private Vector2 lookInput;
    private float verticalLookRotation = 0f;

    void Update()
    {
        if(!player.QuestOpen && !player.InventoryOpen)
        {
            Debug.Log("체크요");
            lookInput = player.lookInput;
            // 카메라의 상하 움직임 처리
            float mouseY = lookInput.y * lookSpeed;
            verticalLookRotation -= mouseY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f); // 제한을 걸어줍니다

            // 카메라의 상하 회전 적용
            virtualCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
        }
    }
}
