using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInfo
{

    private float HP;
    private float MP;

    private int PlayerID;

    private int ATK;
    private int LEVEL;
    private int XP;
    private int WeaponID;
    private float ATTACKSPEED;

    private int ZskillATK;
    private int XskillATK;

    private float ZskillCoolTime;
    private float XskillCoolTime;
    
    public int EquippedWeaponID { get; set; } // ���� ������ ���� ID

    public WeaponBase EquippedWeapon { get; set; } // ���� ������ ����

    #region ĸ��ȭ

    public float Hp
    {
        get { return HP; }
        set 
        {
            HP = (HP > 0) ? value : 0;
        }
    }
    public float MaxHP { get; set; } = 10;
    
    public float Mp
    {
        get { return MP; }
        set {
            MP = (MP > 0) ? value : 0;
        }
    }
    public float MaxMp { get; set; } = 10;


    public int Atk
    {
        get
        {
            return ATK + (EquippedWeapon != null ? EquippedWeapon.Atk : 0); // �⺻ ATK + ���� ATK
        }
        set { ATK = value; }        
    }
    public float AttackSpeed
    {
        get { return ATTACKSPEED; }
        set { ATTACKSPEED = value; }
    }
    
    public int Level
    {
        get { return LEVEL; }
        set { LEVEL = value; }
    }
    public int Xp{
        get { return XP; }
        set { XP = value; }
    }
    
    public float MoveSpeed { get; set; } = 5f;
    public float JumpHeight{ get; set; } = 5f;

    public int WeaponId
    {
        get { return WeaponID; }
        set { WeaponID = value; }
    }
    public bool WeaponEquip { get; set; } = false;

    public int PLAYER_ID
    {
        get { return PlayerID; }
        set { PlayerID = value; }
    }

    public  int ZATK
    {
        get { return ZskillATK; }
        set { ZskillATK = value; }
    }
    public  int XATK
    {
        get { return XskillATK; }
        set { XskillATK = value; }
    }
    public float ZskillCoolTimer
    {
        get { return ZskillCoolTime; }
        set { ZskillCoolTime = value; }
    }
    public float XskillCoolTimer
    {
        get { return XskillCoolTime; }
        set { XskillCoolTime = value; }
    }

    #endregion

    

}

[System.Serializable]
public class PlayerInfoDTO
{
    [SyncVar]
    public int PLAYERID; // �÷��̾� ID
    [SyncVar]
    public float HP; // ü��
    [SyncVar]
    public float MP;
    [SyncVar]
    public int LEVEL; // ����
    [SyncVar]
    public int XP; // ����ġ

    public PlayerInfoDTO()
    {
        // �⺻������ �ʱ�ȭ�� �� �ֽ��ϴ�.
        PLAYERID = 0;
        HP = 100f; // �⺻ ü��
        MP =  100f; // �⺻ ����
        LEVEL = 1; // �⺻ ����
        XP = 0; // �⺻ ����ġ
    }

    // ������
    public PlayerInfoDTO(int playerId, float hp, float mp,int level, int xp)
    {
        this.PLAYERID = playerId;
        this.HP = hp;
        this.MP = mp;
        this.LEVEL = level;
        this.XP = xp;
    }
}


public class PlayerState : NetworkBehaviour
{
    public PlayerManager PM;

    #region �÷��̾� �⺻ ����
    [SyncVar(hook =nameof(SyncHp))]
    public float CurrentHp = 100f;
    [SyncVar(hook =nameof(SyncMp))]
    public float CurrentMp = 100f;
    
    [SyncVar]
    public int CurrentLevel = 1;

    [SyncVar(hook = nameof(SyncXp))]
    public float CurrentXp = 0f;

    [SyncVar]
    public float MaxHP = 100f;

    [SyncVar]
    public float MaxMP = 100f;
    
    [SyncVar]
    public float MaxXP = 100f;

    [SyncVar(hook = nameof(OnMoveSpeedChange))]
    public float CurrentMoveSpeed = 2f;

    private void OnMoveSpeedChange(float oldSpeed, float newSpeed)
    {
        Debug.Log("�̵� �ӵ� ����");
        PIC.animator.SetFloat("MoveSpeed", newSpeed);
    }
    
    [SyncVar(hook =nameof(OnCurrentWeaponIDChange))]
    public int CurrentWeaponID;

    private void OnCurrentWeaponIDChange(int oldWeapon, int newWeapon)
    {
        PIC.animator.SetInteger("WeaponID", newWeapon);
    }

    [SyncVar]
    public float CurrentAtk;
    [SyncVar]
    public float CurrentAttackSpeed;
    [SyncVar]
    public int CurrentZSkillATK;
    [SyncVar]
    public int CurrentXSkillATK;
    [SyncVar]
    public float CurrentZSkillCoolTime;
    [SyncVar]
    public float CurrentXSkillCoolTime;

    [SyncVar]
    public string Round = "Tutorial";

    //[SyncVar(hook =nameof(SyncWeaponEquip))]
    [SyncVar]
    public bool WeaponEquip;

    [SyncVar]
    public int gold = 5000;

    private void SyncWeaponEquip(bool oldValue, bool newValue)
    {
        if(newValue == true)
        {
            CurrentWeaponOBJ.SetActive(true);
            CurrentWeaponBackOBj.SetActive(false);
            CurrentAura.SetActive(true);
        }
        else if(newValue == false)
        {
            CurrentWeaponOBJ.SetActive(false);
            CurrentWeaponBackOBj.SetActive(true);
            CurrentAura.SetActive(false);
        }
    }

    [SyncVar]
    public bool AnimEnd;
    #endregion


    #region �÷��̾� ���� ����
    [SyncVar]
    public NetworkIdentity CurrentWeaponNetID;

    public GameObject CurrentWeaponOBJ;
    [SyncVar]
    public NetworkIdentity CurrentWeaponBackNetID;

    public GameObject CurrentWeaponBackOBj;
    [SyncVar]
    public NetworkIdentity CurrentAuraNetID;

    public GameObject CurrentAura;


    [SyncVar(hook =nameof(OnHandWeaponPositionChanged))]
    public Vector3 HandWeaponPosition;

    public Transform HandWeaponPoint;

    [SyncVar(hook = nameof(OnBackWeaponPositionChanged))]
    public Vector3 BackWeaponPosition;

    public Transform BackWeaponPoint;
    [SyncVar(hook = nameof(OnAuraPositionChanged))]
    public Vector3 AuraPosition;

    public Transform PlayerAuraPoint;
    [SyncVar]
    public Vector3 SwordEffectPosition;
    public Transform SwordEffect;

    #endregion

    #region
    [SerializeField] private PlayerInputCheck PIC;


    public WeaponBase CurrentWeapon;

    public GameObject[] FirstWeapons;
    public GameObject[] SecondWeapons;
    public GameObject[] ThirdWeapons;


    [SerializeField] string controlScheme;
    public bool PlayerIsDead { get; set; } = false;
    public int ComboCount = 0;
    #endregion

    [Command]
    private void ChangeRound(PlayerState PS,string Round)
    {
        PS.Round = Round;
        CmdChangeStage(PS.PM, PS.Round);
    }

    public void CmdChangeStage(PlayerManager Pm,string Round)
    {
        GameManager.Instance.CmdUnloadScene(Pm,Round);
    }

    public void ChestSpawnInNewRound()
    {
        CmdUnloadScene(this.PM);
    }
    [Server]
    private void CmdUnloadScene(PlayerManager PM)
    {
        Scene NextRound = SceneManager.GetSceneByName(PM.PS.Round);
        GameObject[] gameObjects = NextRound.GetRootGameObjects();
        switch (PM.PS.Round)
        {
            case "1Round":
                if (GameManager.Instance.lootManager.lootPoints.TryGetValue(2, out List<LootPoint> list))
                {
                    GameManager.Instance.lootManager.SpawnChest(2);
                }
                break;
            case "2Round":
                if (GameManager.Instance.lootManager.lootPoints.TryGetValue(3, out List<LootPoint> list2))
                {
                    GameManager.Instance.lootManager.SpawnChest(3);
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // �浹�� ��ü�� "NextStage" �±׸� ������ �ִ��� Ȯ���մϴ�.
        if (other.CompareTag("NextStage"))
        {
            // ���� �÷��̾����� Ȯ���մϴ�.
            if (isLocalPlayer)
            {
                if (IsSceneActive("Tutorial"))
                {
                    this.Round = "1Round";
                    ChangeRound(this, this.Round);
                    Debug.Log("���� ���������� �̵�");
                }
                else if (IsSceneActive("1Round"))
                {
                    this.Round = "2Round";
                    ChangeRound(this, this.Round);
                    Debug.Log("���� ���������� �̵�");
                }
            }
        }
    }

    bool IsSceneActive(string sceneName)
    {        
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }
    
    private void Start()
    {
        PM = GetComponent<PlayerManager>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SetState(this);
    }

    [SyncVar]
    private bool isInit = false;

    #region  ���� �ʱ� ����
    public void SetState(PlayerState ps)
    {
        if(isInit) return;

        ps.MaxHP = 100f;
        ps.MaxMP = 100f;
        ps.MaxXP = 100;
        ps.CurrentLevel = 1;
        
        
        ps.CurrentWeaponID = 0;
        ps.CurrentAtk = 10;
        ps.CurrentAttackSpeed = 1f;
        ps.CurrentZSkillATK = 0;
        ps.CurrentXSkillATK = 0;
        ps.CurrentZSkillCoolTime = 0;
        ps.CurrentXSkillCoolTime = 0;
        //CurrentMoveSpeed = 2f;
        ps.WeaponEquip = false;
        ps.AnimEnd = true;

        ps.CurrentHp = 100f;
        ps.CurrentMp = 100f;
        ps.CurrentXp = 0;
        ps.Round = "Tutorial";
        isInit = true;
    }

    [Command]
    public void SetSpeed(float speed,PlayerInputCheck PIC)
    {
        CurrentMoveSpeed = speed;
    }

    #endregion


    public void AddXp(float xp)
    {
        CurrentXp += xp;
        if (CurrentXp >= MaxXP)
        {
            CurrentLevel++;
            CurrentXp = 0;
        }
    }

    public void SyncHp(float oldHp, float newHp)
    {
        Debug.Log("ü�� ����");
        //PM.HpBar.value = CurrentHp / MaxHP;
        UpdateHPUI(newHp);
    }

    private void UpdateHPUI(float value)
    {
        PM.HpBar.value = value / MaxHP;
    }

    public void SyncMp(float oldMp, float newMp)
    {
        Debug.Log("���� ����");
        UpdateMPUI(newMp);
    }

    private void UpdateMPUI(float value)
    {
        PM.MpBar.value = value / MaxMP;
    }


    public void SyncXp(float oldXp, float newXp)
    {
        Debug.Log("����ġ ����");
        UpdateXPUI(newXp);
    }
    private void UpdateXPUI(float value)
    {
        PM.XpBar.value = value / MaxMP;
    }

    // SyncVar�� ����� �� ȣ��Ǵ� ��ũ �޼���
    private void OnHandWeaponPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        Debug.Log($"HandWeaponPosition changed from {oldValue} to {newValue}");
    }

    private void OnBackWeaponPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        Debug.Log($"BackWeaponPosition changed from {oldValue} to {newValue}");
    }

    private void OnAuraPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        Debug.Log($"AuraPosition changed from {oldValue} to {newValue}");
    }

    public void animsetstate(PlayerManager PM)
    {
        if (PM.isLocalPlayer) // Ŭ���̾�Ʈ�� ������ ���� ��츸 ����
        {
            if (PM.PS.WeaponEquip == false)
            {
                PM.PS.WeaponEquip = true;
                PM.PS.CurrentWeaponOBJ.SetActive(true);
                PM.PS.CurrentWeaponBackOBj.SetActive(false);
                PM.PS.CurrentAura.SetActive(true);
            }
            else if (PM.PS.WeaponEquip == true)
            {
                PM.PS.WeaponEquip = false;
                PM.PS.CurrentWeaponOBJ.SetActive(false);
                PM.PS.CurrentWeaponBackOBj.SetActive(true);
                PM.PS.CurrentAura.SetActive(false);
            }
            PM.PS.SetAnimState(PM); // ������ ���� ���� ��û
        }
    }

    [Command]
    public void SetAnimState(PlayerManager Pm)
    {
        if(Pm.PS.WeaponEquip == false)
        {
            Pm.PS.WeaponEquip = true;
            Pm.PS.CurrentWeaponOBJ.SetActive(true);
            Pm.PS.CurrentWeaponBackOBj.SetActive(false);
            Pm.PS.CurrentAura.SetActive(true);
        }
        else if(PM.PS.WeaponEquip == true)
        {
            Pm.PS.WeaponEquip = false;
            Pm.PS.CurrentWeaponOBJ.SetActive(false);
            Pm.PS.CurrentWeaponBackOBj.SetActive(true);
            Pm.PS.CurrentAura.SetActive(false);
        }
    }
    
}
