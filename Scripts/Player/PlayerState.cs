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
    
    public int EquippedWeaponID { get; set; } // 현재 장착된 무기 ID

    public WeaponBase EquippedWeapon { get; set; } // 현재 장착된 무기

    #region 캡슐화

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
            return ATK + (EquippedWeapon != null ? EquippedWeapon.Atk : 0); // 기본 ATK + 무기 ATK
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
    public int PLAYERID; // 플레이어 ID
    [SyncVar]
    public float HP; // 체력
    [SyncVar]
    public float MP;
    [SyncVar]
    public int LEVEL; // 레벨
    [SyncVar]
    public int XP; // 경험치

    public PlayerInfoDTO()
    {
        // 기본값으로 초기화할 수 있습니다.
        PLAYERID = 0;
        HP = 100f; // 기본 체력
        MP =  100f; // 기본 마나
        LEVEL = 1; // 기본 레벨
        XP = 0; // 기본 경험치
    }

    // 생성자
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

    #region 플레이어 기본 스텟
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
        Debug.Log("이동 속도 변경");
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


    #region 플레이어 무기 관련
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
        // 충돌한 객체가 "NextStage" 태그를 가지고 있는지 확인합니다.
        if (other.CompareTag("NextStage"))
        {
            // 로컬 플레이어인지 확인합니다.
            if (isLocalPlayer)
            {
                if (IsSceneActive("Tutorial"))
                {
                    this.Round = "1Round";
                    ChangeRound(this, this.Round);
                    Debug.Log("다음 스테이지로 이동");
                }
                else if (IsSceneActive("1Round"))
                {
                    this.Round = "2Round";
                    ChangeRound(this, this.Round);
                    Debug.Log("다음 스테이지로 이동");
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

    #region  스텟 초기 설정
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
        Debug.Log("체력 연동");
        //PM.HpBar.value = CurrentHp / MaxHP;
        UpdateHPUI(newHp);
    }

    private void UpdateHPUI(float value)
    {
        PM.HpBar.value = value / MaxHP;
    }

    public void SyncMp(float oldMp, float newMp)
    {
        Debug.Log("마나 연동");
        UpdateMPUI(newMp);
    }

    private void UpdateMPUI(float value)
    {
        PM.MpBar.value = value / MaxMP;
    }


    public void SyncXp(float oldXp, float newXp)
    {
        Debug.Log("경험치 연동");
        UpdateXPUI(newXp);
    }
    private void UpdateXPUI(float value)
    {
        PM.XpBar.value = value / MaxMP;
    }

    // SyncVar가 변경될 때 호출되는 후크 메서드
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
        if (PM.isLocalPlayer) // 클라이언트가 권한이 있을 경우만 실행
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
            PM.PS.SetAnimState(PM); // 서버에 상태 변경 요청
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
