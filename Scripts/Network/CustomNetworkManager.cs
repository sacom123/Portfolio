using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{

    public GameObject GameManagerPrefabs;
    public List<GameObject> prefabsToRegister = new List<GameObject>();  // 서버와 클라이언트에서 공유할 프리팹들
    public Weaponcollection weaponCollection;
    GameObject GameManagerIns;

    public override void Start()
    {
        base.Start();

        //CustomNetworkManager.singleton.networkAddress = "192.168.219.44";

        CustomNetworkManager.singleton.networkAddress = "localHost";

        EnemySpawnManager ESM = GameManagerPrefabs.GetComponent<GameManager>().ESM;
        EnemyRegistserdInServer(ESM);

        Weaponcollection WC = GameManagerPrefabs.GetComponent<GameManager>().weaponCollection;
        RegisterPrefabs(WC.GetFirstWeapon());
        RegisterPrefabs(WC.GetSecondWeapon());
        RegisterPrefabs(WC.GetThirdWeapon());

        RegisterPrefabs(GameManagerPrefabs.GetComponent<GameManager>().Loot.goldChestPrefab);
        RegisterPrefabs(GameManagerPrefabs.GetComponent<GameManager>().Loot.ironChestPrefab);
        RegisterPrefabs(GameManagerPrefabs.GetComponent<GameManager>().Loot.woodenChestPrefab);

        RegisterEffectPrefabs(GameManagerPrefabs.GetComponent<GameManager>().Em.SetEffectPrefabs());

        RegisteredListInServer(WC.GetItemPreFabs(ItemType.item));
        RegisteredListInServer(WC.GetItemPreFabs(ItemType.misc)); 

    }

    public override void Update()
    {
        base.Update();

        if(Input.GetKeyDown(KeyCode.F1))
        {
            StartServer();
        }
    }

    public void RegisterPrefabs(List<WeaponBase>WB)
    {
       for(int i=0;i<prefabsToRegister.Count;i++)
        {
            prefabsToRegister.Add(WB[i].Weapon);
            prefabsToRegister.Add(WB[i].BackWeapon);
            prefabsToRegister.Add(WB[i].PA);
            
        }
        RegisteredInServer(WB);
    }

    private void RegisteredInServer(List<WeaponBase> WB)
    {
        
        for (int i = 0; i < WB.Count; i++)
        {
            spawnPrefabs.Add(WB[i].Weapon);
            spawnPrefabs.Add(WB[i].BackWeapon);        
            if (spawnPrefabs.Contains(WB[i].PA) == false)        
            {        
                spawnPrefabs.Add(WB[i].PA);        
            }        
            else        
            {        
                return;        
            }        
        }
    }

    private void RegisteredListInServer(List<GameObject> obj)
    {
        for(int i=0;i<obj.Count;i++)
        {
            spawnPrefabs.Add(obj[i]);
        }
    }

    private void EnemyRegistserdInServer(EnemySpawnManager ESM)
    {
        for(int i=1;i<=ESM.m_SpawnList.Count;i++)
        {
            for(int j = 0; j < ESM.m_SpawnList[(Stage)i].Count;j++)
            {
                spawnPrefabs.Add(ESM.m_SpawnList[(Stage)i][j].gameObject);
            }
        }
    }

    private void  RegisterPrefabs(GameObject obj)
    {
        spawnPrefabs.Add(obj);
    }

    private  void RegisterEffectPrefabs(GameObject[] EffectList)
    {
        for(int i=0;i<EffectList.Length;i++)
        {
            if (EffectList[i] != null)
                spawnPrefabs.Add(EffectList[i]);
            else if (EffectList[i] == null)
                Debug.Log("EffectList is null");
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        GameManagerIns = Instantiate(GameManagerPrefabs);
        NetworkServer.Spawn(GameManagerIns);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("OnServerConnect");
        if (GameManager.Instance.gameSctors.player.Count >= 1)
        {
            Debug.Log("플레이어가 이미 존재합니다.");
            // 새로운 클라이언트에 활성 씬 동기화
            GameManager.Instance.GetComponent<GameManager>().SyncScenesWithClient(conn);
        }
    }

    //  클라가 접속, OnServerAddPlayer가 호출되기 전에 호출
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        
    }
    // 서버

    
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log("플레이어 접속 ");
        // 클라이언트 준비 상태 설정
        
        base.OnServerAddPlayer(conn);

        if(conn.identity != null)
        {
            Debug.Log("플레이어 리스트에 추가");
            SceneManager.MoveGameObjectToScene(conn.identity.gameObject, SceneManager.GetSceneByName("Tutorial"));
            GameManager.Instance.gameSctors.player.Add(conn.identity.gameObject);
            GameManager.Instance.gameSctors.m_playerList.Add(conn.identity.netId);
        }
        
    }

    public override void OnClientDisconnect()
    {
        if (SoundManager.instance != null)
        {
            Destroy(SoundManager.instance.gameObject);
        }
        Debug.Log("서버 연결 끊김. 재접속 시도 중...");
        base.OnClientDisconnect();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // 연결 해제된 클라이언트의 데이터 초기화
        if (conn.identity != null)
        {
            if(GameManager.Instance != null)
            {
                if (GameManager.Instance.gameSctors.player.Contains(conn.identity.gameObject))
                {
                    GameManager.Instance.gameSctors.player.Remove(conn.identity.gameObject);
                }
                NetworkServer.Destroy(conn.identity.gameObject);
            }

            if(SoundManager.instance != null)
            {
                Destroy(SoundManager.instance.gameObject);
            }
        }
        base.OnServerDisconnect(conn);
    }
    
}
