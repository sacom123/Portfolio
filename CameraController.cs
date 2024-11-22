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
            Debug.Log("üũ��");
            lookInput = player.lookInput;
            // ī�޶��� ���� ������ ó��
            float mouseY = lookInput.y * lookSpeed;
            verticalLookRotation -= mouseY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f); // ������ �ɾ��ݴϴ�

            // ī�޶��� ���� ȸ�� ����
            virtualCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
        }
    }
}
