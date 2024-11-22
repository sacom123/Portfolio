using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Telepathy;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PlayerInputCheck : NetworkBehaviour
{
    #region 선언부
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

    //Player의 Canvas 안에 있는 슬라이더 참조(플레이어가 생성됬을때 사운드 매니저에 알리기 위해)
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

    #region 구현부


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

    // 움직임
    [Client]
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!isAttack && !InventoryOpen && !QuestOpen && !dialogManager.isUpgrade && !PM.isDialog && !interactionInputcheck)
        {
            if(context.started || context.performed)
            {
                moveInput = context.ReadValue<Vector2>();
                PM.PS.SetSpeed(2f,this);
                PM.CmdUpdateInput(moveInput); // Command 호출
            }
            
            if(context.canceled)
            {
                moveInput = Vector2.zero;
                PM.PS.SetSpeed(0f,this);
                PM.CmdUpdateInput(moveInput); // Command 호출
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
                // 카메라의 앞 방향과 오른쪽 방향을 계산합니다.
                Vector3 forward = MainCam.transform.forward;
                Vector3 right = MainCam.transform.right;

                // y축 이동을 방지하기 위해 y 값을 0으로 설정합니다.
                forward.y = 0f;
                right.y = 0f;

                forward.Normalize();
                right.Normalize();
                // 캐릭터의 이동 방향을 계산합니다.
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

    // 달리기
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



    #region 카메라
    // 화면 돌리기
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

    // 자유시점
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

    #region 무기
    // 무기를 변경하는 이벤트
    public void OnWeapon(InputAction.CallbackContext cont)
    {
        // 조건문으로 인벤토리에 해당 번호의 무기 있는지, 없으면 무기 변경 불가능 메시지 출력
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

    [Header("무기 선택 창")]
    [SerializeField]
    GameObject[] Ui;

    public bool lootChest_InRange = false; //상자 상호작용 범위 확인용 플래그

    public GameObject currentLootChest; // 현재 상호작용 중인 상자 오브젝트

    public float lootingTime; // looting 시간이 설정된 변수,상자 열때마다 lootingTime업데이트 필요(만약 루팅 타임을 아이템 파밍 횟수에 따라 줄일 예정이라면)
    [SerializeField]
    public LootUIManager lootUIManager;
    [SerializeField]
    UIManager uiManger;
    public QuestManager questManager;

    #region 상호작용


    // 상호작용
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
                Debug.LogWarning("currentLootChest가 null입니다.");
                return;
            }

            LootChest lootChest = currentLootChest.GetComponent<LootChest>();

            if (lootChest == null)
            {
                Debug.LogWarning("LootChest 컴포넌트를 currentLootChest에서 찾을 수 없습니다.");
                return;
            }

            if (lootChest.gameObject.name == "TutorialChest")
            {
                if (questManager != null && questManager.IsQuestAccepted("t_Quest_001") && !questManager.IsQuestCompleted("t_Quest_001"))
                {
                    lootUIManager.Initialize(PM.InventoryManager); //인벤토리 매니저 설정
                    lootUIManager.OpenLootingUI(lootChest); //루팅 UI 열기
                }
                else
                {
                    Debug.Log("t_Quest_001이 수락 중이 아니거나 완료되어 상호작용할 수 없습니다.");
                }
            }
            else
            {
                lootUIManager.Initialize(PM.InventoryManager); //인벤토리 매니저 설정
                lootUIManager.OpenLootingUI(lootChest); //루팅 UI 열기
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
            if (npcData.npc_Id == "r1_npc_001"&&!isExit) //특정 NPC 이름
            {
                Debug.Log($"특정 NPC({npcData.npc_Id})와의 충돌 감지. 대화 시작.");
                ForceStartDialog(); //강제로 대화 시작
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
            currentLootChest = other.gameObject; //현재 상자 오브젝트 저장            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NpcCollider"))
        {
            npc_InRange = false;
            npcData = null;
            isCollector = false;
            dialogManager.CloseDialog();  //대화 종료            
        }
        else if (other.CompareTag("LootChest"))
        {
            lootChest_InRange = false; //상호작용 불가
        }
    }
    #endregion

    private void ForceStartDialog()
    {
        if (npcData == null)
        {
            Debug.LogWarning("NPCData가 null입니다. 대화를 시작할 수 없습니다.");
            return;
        }

        if (dialogManager == null)
        {
            Debug.LogError("DialogManager가 설정되지 않았습니다.");
            return;
        }

        //강제로 대화 상자를 열고 대화를 시작
        dialogBox.SetActive(true);
        dialogManager.ShowDialog(npcData.npc_Id, npcData.initial_Dialog_Id);
        dialogManager.SetCurrentNPC(npcData);
        PM.IsDialoging(true); //플레이어 대화 상태로 전환
        isExit = true;
    }

    public bool InventoryOpen = false;    

    // 인벤토리 키는 이벤트
    public void OnInventori(InputAction.CallbackContext con)
    {        
        if (!con.started) return;
        InventoriCheck();
        Debug.Log("인벤토리");
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
        // 퀘스트 창, 인벤토리 창, 상점 창이 열려 있을 때는 해당 창을 먼저 닫음
        if (QuestOpen)
        {
            QuestOpen = false;
            PlayerInteraction.QuestBox(questUI);
            if(PM.isDialog) UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.visible = false;
            Debug.Log("퀘스트 창 닫음");
        }
        else if (InventoryOpen)
        { 
            InventoryOpen = false;
            PM.InventoryManager.ExitInven();
            if (PM.isDialog) UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.visible = false;
            Debug.Log("인벤토리 창 닫음");
        }
        else if (dialogManager.isOpenShop)
        {
            Debug.Log("3");
            dialogManager.isOpenShop = false;
            dialogManager.ShopUI.SetActive(false);
            UnityEngine.Cursor.visible = false;
            Debug.Log("상점 창 닫음");
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
        // 모든 창이 닫혀 있을 때만 옵션 창을 열고 닫음
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
            OptionCheck(); // 옵션 창 상태 확인 및 열기/닫기
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

        // 대화가 이미 활성화된 상태에서 F 키를 다시 누르면 대화를 종료
        if (dialogBox.activeSelf && dialogManager.IsEndDialog())  // DialogManager에서 isEndDialog 확인
        {
            dialogManager.CloseDialog();  // 대화 종료       
            if (isC)
            {
                for (int i = 0; i < ui.Length; i++)
                {
                    ui[i].gameObject.SetActive(true);
                }
            }
        }

        else if (!dialogBox.activeSelf)  // 대화가 시작되지 않은 상태에서 F 키를 누르면 대화 시작
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