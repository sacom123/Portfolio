using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Telepathy;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PlayerInputCheck : NetworkBehaviour
{
    #region �����
    public PlayerManager PM;

    public bool PlayerisLocal = false;

    [SerializeField] float RunSpeed = 5f;
    
    [SerializeField] float rotationSpeed = 10f;

    public CharacterController characterController;

    [SerializeField]public Camera MainCam;

    public static bool isDIalog = false;

    public Animator animator;

    public bool isAttack = false;

    [Header("Move")]
    [SerializeField]
    public Vector2 moveInput;

    public bool CanMove = true;
    public Vector2 lookInput;
    
    private bool freeLookSwitch;

    [Header("Jump")]
    public Vector3 _PlayerVelocity;

    [SerializeField]
    float maxjumpheight = 5f;
    float newVelocity = 0f;

    private float _gravity = -9.81f;
    

    [SerializeField]
    public DialogManager dialogManager;

    public bool npc_InRange = false;

    public NPCData npcData;

    [SerializeField]
    GameObject dialogBox;

    [SerializeField]
    GameObject questUI;

    [SerializeField]
    float CurrnetHP;

    [SerializeField]
    private QuickSlotManager quickSlotManager;
    [SerializeField]
    private UpgradeWeapon upgradeWeapon; 

    [SerializeField]
    private GroundCheck GroundCheck;
    [SerializeField]
    private bool isGrounded;

    //Player�� Canvas �ȿ� �ִ� �����̴� ����(�÷��̾ ���������� ���� �Ŵ����� �˸��� ����)
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;



    private void Start()
    {
        
        PM.PS = this.GetComponent<PlayerState>();
        SoundManager.instance.RegisterSliders(masterSlider, bgmSlider, sfxSlider);
        lootingTime = 5f;

    }

    #endregion

    #region ������


    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (isServer)
            return;

        if (!PM.isDialog && PM.PS.AnimEnd && !InventoryOpen && !QuestOpen && !dialogManager.isUpgrade)
        {
            //Jump();
            ApplyGravity();
            Move();
            Look();
        }
        else
        {
            _PlayerVelocity = Vector3.zero;

        }
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (isServer)
            return;


        isGrounded = GroundCheck.IsGrounded();
        if (isGrounded)
        {
            animator.SetBool("Jump", false);
        }

        if(moveInput != Vector2.zero)
        {
            characterController.Move(_PlayerVelocity * PM.PS.CurrentMoveSpeed * Time.deltaTime);
        }
        else
        {
            characterController.Move(newVelocity * Vector3.up * Time.deltaTime);
        }
        
    }

    // ������
    [Client]
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!isAttack && !InventoryOpen && !QuestOpen && !dialogManager.isUpgrade && !PM.isDialog && !interactionInputcheck)
        {
            if(context.started || context.performed)
            {
                moveInput = context.ReadValue<Vector2>();
                PM.PS.SetSpeed(2f,this);
                PM.CmdUpdateInput(moveInput); // Command ȣ��
            }
            
            if(context.canceled)
            {
                moveInput = Vector2.zero;
                PM.PS.SetSpeed(0f,this);
                PM.CmdUpdateInput(moveInput); // Command ȣ��
            }
        }
        else
        {
            moveInput = Vector2.zero;
            PM.PS.SetSpeed(0f, this);
            PM.CmdUpdateInput(moveInput);
        }

    }

    private void Move()
    {
        if (!freeLookSwitch)
        {
            if (CanMove)
            {
                // ī�޶��� �� ����� ������ ������ ����մϴ�.
                Vector3 forward = MainCam.transform.forward;
                Vector3 right = MainCam.transform.right;

                // y�� �̵��� �����ϱ� ���� y ���� 0���� �����մϴ�.
                forward.y = 0f;
                right.y = 0f;

                forward.Normalize();
                right.Normalize();
                // ĳ������ �̵� ������ ����մϴ�.
                _PlayerVelocity = (forward * moveInput.y + right * moveInput.x).normalized;
                _PlayerVelocity.y = newVelocity;

                //animator.SetFloat("MoveX", PM.MoveInput.x);
                //animator.SetFloat("MoveY", PM.MoveInput.y);
            }
        }

    }


    private void ApplyGravity()
    {
        if(isGrounded && newVelocity <0.0f)
        {
            newVelocity = -1.0f;
        }
        else
        {
            newVelocity += _gravity * Time.deltaTime;
        }

        _PlayerVelocity.y = newVelocity;
    }

    // �޸���
    public void OnRun(InputAction.CallbackContext context)
    {
        Debug.Log("Run");
        if (context.started)
        {
            if(isLocalPlayer)
            {
                PM.PS.SetSpeed(RunSpeed,this);
            }
                
        }
        if (context.canceled)
        {
            if(isLocalPlayer)
            {
                PM.PS.SetSpeed(2f, this);
            }
                
        }
    }

    public void OnJump(InputAction.CallbackContext con)
    {
        if (!con.started) return;
        if (!isGrounded) return;

        animator.SetBool("Jump", true);
        newVelocity += maxjumpheight;
    }



    #region ī�޶�
    // ȭ�� ������
    public void OnLook(InputAction.CallbackContext context)
    {
        if(!QuestOpen && !InventoryOpen)
        {
            lookInput = context.ReadValue<Vector2>();
        }
        else if (QuestOpen || InventoryOpen)
        {
            lookInput = new Vector2(0, 0);
        }
    }

    // ��������
    public void OnFreeLook(InputAction.CallbackContext context)
    {
        if (context.started)
            freeLookSwitch = context.ReadValueAsButton();

        if (context.canceled)
            freeLookSwitch = false;

    }
    private void Look()
    {
        if (!freeLookSwitch)
        {
            Quaternion cameraRo = MainCam.transform.rotation;
            Vector3 currentEuler = transform.rotation.eulerAngles;
            Quaternion adjust = Quaternion.Euler(currentEuler.x, cameraRo.eulerAngles.y, cameraRo.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, adjust, rotationSpeed * Time.deltaTime);
        }
    }
    #endregion
    #endregion

    #region ����
    // ���⸦ �����ϴ� �̺�Ʈ
    public void OnWeapon(InputAction.CallbackContext cont)
    {
        // ���ǹ����� �κ��丮�� �ش� ��ȣ�� ���� �ִ���, ������ ���� ���� �Ұ��� �޽��� ���
        if (cont.started)
        {
            switch (cont.control.name)
            {
                case "up":
                case "1":
                    ItemCheck(1);
                    break;
                case "down":
                case "2":
                    ItemCheck(2);
                    break;
                case "left":
                case "3":
                    ItemCheck(3);
                    break;
                case "right":
                case "4":
                    ItemCheck(4);
                    break;
            }
        }
    }

    private void ItemCheck(int slotId)
    {
        Debug.Log(quickSlotManager.inventory.QuickSlots[slotId - 1].myItem.myItem.GetItemType);

        switch (quickSlotManager.inventory.QuickSlots[slotId-1].myItem.myItem.GetItemType)
        {
            case ItemType.Weapon:
                WeaponEqAni(quickSlotManager.inventory.QuickSlots[slotId-1].myItem.myItem._WeaponID, PM);
                break;
            case ItemType.potion:
                ItemUse(quickSlotManager.inventory.QuickSlots[slotId - 1].myItem.myItem, slotId - 1);
                break;
        }
    }
    public void WeaponEqAni(int WeaponID, PlayerManager PM)
    {
        PM.ChangeWeapon(WeaponID);
    }
       
    private void ItemUse(WeaponBase Base,int index)
    {
        switch(Base.Item)
        {
            case ItemClass.Hpotion:
                CmdUseHpP(Base._Amount, PM.PS);
                if (quickSlotManager.inventory.QuickSlots[index].myItem.Amount > 1)
                {
                    quickSlotManager.inventory.QuickSlots[index].myItem.DisplayAmount(-1);
                }
                else if(quickSlotManager.inventory.QuickSlots[index].myItem.Amount >= 1)
                {
                    quickSlotManager.inventory.QuickSlots[index].myItem.DisplayAmount(-1);
                    Destroy(quickSlotManager.inventory.QuickSlots[index].myItem.gameObject);
                    quickSlotManager.inventory.QuickSlots[index].myItem = null;
                    quickSlotManager.inventory.QuickSlots[index].CurrentItem = null;
                }                
                break;
            case ItemClass.Mpotion:
                CmdUseMpP(Base._Amount, PM.PS);
                if(quickSlotManager.inventory.QuickSlots[index].myItem.Amount > 0)
                {
                    quickSlotManager.inventory.QuickSlots[index].myItem.DisplayAmount(-1);
                }
                else if (quickSlotManager.inventory.QuickSlots[index].myItem.Amount >= 1)
                {
                    quickSlotManager.inventory.QuickSlots[index].myItem.DisplayAmount(-1);
                    Destroy(quickSlotManager.inventory.QuickSlots[index].myItem.gameObject);
                    quickSlotManager.inventory.QuickSlots[index].myItem = null;
                    quickSlotManager.inventory.QuickSlots[index].CurrentItem = null;
                }
                break;
        }
    }
    [Command]
    public void CmdUseHpP(int amount,PlayerState PS)
    {
        if(PS.CurrentHp + amount > PS.MaxHP)
        {
            PS.CurrentHp = PS.MaxHP;
        }
        else
        {
            PS.CurrentHp += amount;            
        }

    }


    [Command]
    public void CmdUseMpP(int amount, PlayerState PS)
    {
        if(PS.CurrentMp + amount > PS.MaxMP)
        {
            PS.CurrentMp = PS.MaxMP;
        }
        else
        {
            PS.CurrentMp += amount;
        }
    }


    #endregion

    private bool interactionInputcheck;
    private bool QuestInputCheck;
    private bool isCollector = false;

    [Header("���� ���� â")]
    [SerializeField]
    GameObject[] Ui;

    public bool lootChest_InRange = false; //���� ��ȣ�ۿ� ���� Ȯ�ο� �÷���

    public GameObject currentLootChest; // ���� ��ȣ�ۿ� ���� ���� ������Ʈ

    public float lootingTime; // looting �ð��� ������ ����,���� �������� lootingTime������Ʈ �ʿ�(���� ���� Ÿ���� ������ �Ĺ� Ƚ���� ���� ���� �����̶��)
    [SerializeField]
    public LootUIManager lootUIManager;
    [SerializeField]
    UIManager uiManger;
    public QuestManager questManager;

    #region ��ȣ�ۿ�


    // ��ȣ�ۿ�
    public void InputInteraction(InputAction.CallbackContext con)
    {
        if (!con.started) return;

        if (npc_InRange == true)
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", 0);
            PM.IsDialoging(true);
            PlayerInteraction.SetDialogManager(dialogManager, dialogBox, npcData, isCollector);
            PlayerInteraction.ui = Ui;
        }
        if (lootChest_InRange)
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", 0);

            if (currentLootChest == null)
            {
                Debug.LogWarning("currentLootChest�� null�Դϴ�.");
                return;
            }

            LootChest lootChest = currentLootChest.GetComponent<LootChest>();

            if (lootChest == null)
            {
                Debug.LogWarning("LootChest ������Ʈ�� currentLootChest���� ã�� �� �����ϴ�.");
                return;
            }

            if (lootChest.gameObject.name == "TutorialChest")
            {
                if (questManager != null && questManager.IsQuestAccepted("t_Quest_001") && !questManager.IsQuestCompleted("t_Quest_001"))
                {
                    lootUIManager.Initialize(PM.InventoryManager); //�κ��丮 �Ŵ��� ����
                    lootUIManager.OpenLootingUI(lootChest); //���� UI ����
                }
                else
                {
                    Debug.Log("t_Quest_001�� ���� ���� �ƴϰų� �Ϸ�Ǿ� ��ȣ�ۿ��� �� �����ϴ�.");
                }
            }
            else
            {
                lootUIManager.Initialize(PM.InventoryManager); //�κ��丮 �Ŵ��� ����
                lootUIManager.OpenLootingUI(lootChest); //���� UI ����
            }

        }
    }

    

    public bool QuestOpen = false;

    public void QuestInteraction(InputAction.CallbackContext con)
    {
        if (!QuestInputCheck && !QuestOpen && con.started)
        {
            UnityEngine.Cursor.visible = true;
            QuestInputCheck = true;
            QuestOpen = true;
            PlayerInteraction.QuestBox(questUI);
        }
        else if(QuestOpen && con.started)
        {
            if (PM.isDialog || lootUIManager.isLootingWindow || InventoryOpen)
            {
                UnityEngine.Cursor.visible = true;
            }
            else
            {
                UnityEngine.Cursor.visible = false;
            }
            QuestOpen = false;
            PlayerInteraction.QuestBox(questUI);
        }

        if (con.canceled)
            QuestInputCheck = false;
    }

    public bool isExit = false;
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("NpcCollider"))
        {
            npcData = other.GetComponent<NPC>().npcData;
            if (npcData.npc_Id == "r1_npc_001"&&!isExit) //Ư�� NPC �̸�
            {
                Debug.Log($"Ư�� NPC({npcData.npc_Id})���� �浹 ����. ��ȭ ����.");
                ForceStartDialog(); //������ ��ȭ ����
            }
            else
            {
                npc_InRange = true;
                if (npcData.npc_Id == "npc_002")
                {
                    isCollector = true;
                }
            }
        }
        else if (other.CompareTag("LootChest"))
        {
            lootChest_InRange = true;
            currentLootChest = other.gameObject; //���� ���� ������Ʈ ����            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NpcCollider"))
        {
            npc_InRange = false;
            npcData = null;
            isCollector = false;
            dialogManager.CloseDialog();  //��ȭ ����            
        }
        else if (other.CompareTag("LootChest"))
        {
            lootChest_InRange = false; //��ȣ�ۿ� �Ұ�
        }
    }
    #endregion

    private void ForceStartDialog()
    {
        if (npcData == null)
        {
            Debug.LogWarning("NPCData�� null�Դϴ�. ��ȭ�� ������ �� �����ϴ�.");
            return;
        }

        if (dialogManager == null)
        {
            Debug.LogError("DialogManager�� �������� �ʾҽ��ϴ�.");
            return;
        }

        //������ ��ȭ ���ڸ� ���� ��ȭ�� ����
        dialogBox.SetActive(true);
        dialogManager.ShowDialog(npcData.npc_Id, npcData.initial_Dialog_Id);
        dialogManager.SetCurrentNPC(npcData);
        PM.IsDialoging(true); //�÷��̾� ��ȭ ���·� ��ȯ
        isExit = true;
    }

    public bool InventoryOpen = false;    

    // �κ��丮 Ű�� �̺�Ʈ
    public void OnInventori(InputAction.CallbackContext con)
    {        
        if (!con.started) return;
        InventoriCheck();
        Debug.Log("�κ��丮");
    }

    public void InventoriCheck()
    {       
        InventoryOpen = !InventoryOpen;
        if (InventoryOpen)
        {
            UnityEngine.Cursor.visible = true;
            PM.InventoryManager._InvenOpen();
        }
        else
        {
            if(QuestOpen|| PM.isDialog || OptionOpen) UnityEngine.Cursor.visible = true;
            
            else UnityEngine.Cursor.visible = false;

            quickSlotManager.UpdateQuickSlotDisplay();
            PM.InventoryManager.ExitInven();
        }        
    }

    public bool OptionOpen;

    public void OnOption(InputAction.CallbackContext con)
    {
        if (!con.started) return;
        // ����Ʈ â, �κ��丮 â, ���� â�� ���� ���� ���� �ش� â�� ���� ����
        if (QuestOpen)
        {
            QuestOpen = false;
            PlayerInteraction.QuestBox(questUI);
            if(PM.isDialog) UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.visible = false;
            Debug.Log("����Ʈ â ����");
        }
        else if (InventoryOpen)
        { 
            InventoryOpen = false;
            PM.InventoryManager.ExitInven();
            if (PM.isDialog) UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.visible = false;
            Debug.Log("�κ��丮 â ����");
        }
        else if (dialogManager.isOpenShop)
        {
            Debug.Log("3");
            dialogManager.isOpenShop = false;
            dialogManager.ShopUI.SetActive(false);
            UnityEngine.Cursor.visible = false;
            Debug.Log("���� â ����");
        }
        else if (dialogManager.isUpgrade)
        {
            dialogManager.isUpgrade = false;
            dialogManager.CloseUpgrade();            
        }
        else if(lootUIManager.isLootingWindow)
        {
            lootUIManager.lootBoxWindow.SetActive(false);
            lootUIManager.isLootingWindow = false;
            if(!PM.isDialog || !InventoryOpen || !QuestOpen || !OptionOpen)   UnityEngine.Cursor.visible = false;
        }
        // ��� â�� ���� ���� ���� �ɼ� â�� ���� ����
        else
        {
            OptionOpen = !OptionOpen;
            if (OptionOpen)
            {
                UnityEngine.Cursor.visible = true;
            }
            else
            {
                if (PM.isDialog)
                {
                    UnityEngine.Cursor.visible = true;
                }
                else
                {
                    UnityEngine.Cursor.visible = false;
                }
            }
            OptionCheck(); // �ɼ� â ���� Ȯ�� �� ����/�ݱ�
        }
    }

    public void OptionCheck()
    {
        
        if (OptionOpen)
        {
            uiManger.OpenOptionsWindow();
            Time.timeScale = 0;
        }
        else
        {
            uiManger.CloseOptionsWindow();
            Time.timeScale = 1;            
        }

    }

    public void CloseQuestUI()
    {
        QuestOpen = false;
    }

    public void CloseShopUI()
    {
        dialogManager.isOpenShop = false;
        UnityEngine.Cursor.visible = false;
    }
}

public static class PlayerInteraction
{
    private static DialogManager dialogManager;
    private static NPCData npcData;
    private static GameObject dialogBox;

    public static GameObject[] ui { get; set; }


    public static void SetDialogManager(DialogManager DM, GameObject DB, NPCData nPC, bool isCollector)
    {
        dialogManager = DM;
        dialogBox = DB;
        npcData = nPC;
        DialogBox(isCollector);
    }
    private static void DialogBox(bool isC)
    {

        // ��ȭ�� �̹� Ȱ��ȭ�� ���¿��� F Ű�� �ٽ� ������ ��ȭ�� ����
        if (dialogBox.activeSelf && dialogManager.IsEndDialog())  // DialogManager���� isEndDialog Ȯ��
        {
            dialogManager.CloseDialog();  // ��ȭ ����       
            if (isC)
            {
                for (int i = 0; i < ui.Length; i++)
                {
                    ui[i].gameObject.SetActive(true);
                }
            }
        }

        else if (!dialogBox.activeSelf)  // ��ȭ�� ���۵��� ���� ���¿��� F Ű�� ������ ��ȭ ����
        {
            dialogBox.SetActive(true);
            dialogManager.ShowDialog(npcData.npc_Id, npcData.initial_Dialog_Id);
            dialogManager.SetCurrentNPC(npcData);
        }
    }
    public static void QuestBox(GameObject questUI)
    {
        if (!questUI.activeSelf)
        {
            questUI.SetActive(true);
        }
        else if (questUI.activeSelf)
        {
            
            questUI.SetActive(false);
        }
    }
}