using AYellowpaper.SerializedCollections;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �÷��̾ �׾��� ���׾���
/// �÷��̾ ��Ƽ ����Ʈ�� �ް� �� ����Ʈ�� ��Ƽ�� �����ߴ���
/// �÷��̾ ��Ƽ�� ���� �ִ���
/// �÷��̾��� ���� ����
/// 
/// </summary>
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance; 
    public EnemySpawnManager ESM;
    public LootManager Loot;

    Camera Maincam;

    // NetworkIdentity ��� netId�� SyncDictionary�� ����
    public readonly SyncDictionary<uint, uint> spawnedWeapons2 = new SyncDictionary<uint, uint>();

    public readonly SyncDictionary<string, List<uint>> SpawnEnemys = new SyncDictionary<string, List<uint>>();

    [SerializeField]
    public GameSctors gameSctors;

    // player���� 
    private List<PlayerManager> Players = new List<PlayerManager>();
    
    public GameObject PlayerPrefab;
    
    public Weaponcollection weaponCollection;
    
    public EffectManager Em;
    
    public ResourcesManager RM;
    public LootManager lootManager;
    [SerializedDictionary("Round", "LootPoint")]
    public SerializedDictionary<int, List<LootPoint>> lootPointList = new SerializedDictionary<int, List<LootPoint>>();

    public TestSpawner TS;
    public BossSpawner BS;
    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // �ߺ��� �ν��Ͻ� ����
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� ����Ǿ �ν��Ͻ��� ����
            
        }
        //SceneManager.sceneLoaded += OnSceneLoaded;
        Maincam = Camera.main;
        
    }


    #region Drtiy Flag

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

   [SyncVar]
    public bool isLoad = false;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Check");
        if (isLoad == false)
        {
            Debug.Log("Check2");
            isLoad = true;
            StartCoroutine(CheckDebug());
        }
        if (!isServer) return;
    }


    IEnumerator CheckDebug()
    {
        yield return new WaitForEndOfFrame();
        yield return SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
        GameObject[] rootObjects = SceneManager.GetSceneByName("Tutorial").GetRootGameObjects();
        for(int i=0;i<rootObjects.Length; i++)
        {
            if (rootObjects[i].CompareTag("LootPointList"))
            {
                var lootPoints = rootObjects[i].GetComponent<LootPointList>().LootPointList1;
                lootPointList.Add(1, lootPoints);
                break;
            }
        }
        

        GameObject[] rootObjects2 = SceneManager.GetSceneByBuildIndex(2).GetRootGameObjects();
        foreach (var item in rootObjects2)
        {
            if (item.CompareTag("LootPointList"))
            {
                var lootPoints = item.GetComponent<LootPointList>().LootPointList1;
                lootPointList.Add(2, lootPoints);
                break;
            }
        }

        yield return SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);
        GameObject[] rootObjects3 = SceneManager.GetSceneByBuildIndex(3).GetRootGameObjects();
        foreach (var item in rootObjects3)
        {
            if (item.CompareTag("LootPointList"))
            {

                var lootPoints = item.GetComponent<LootPointList>().LootPointList1;
                lootPointList.Add(3,lootPoints);
                break;
            }
        }

        lootManager.FindLootPoints();
        lootManager.SpawnChest(1);
        ESM.SpawnEnemy(Stage.Stage1,"Tutorial");

        for(int i=0; i< NetworkManager.startPositions.Count; i++)
        {
            Debug.Log(NetworkManager.startPositions[i].position);
        }
    }

    private void Start()
    {
        if (isServer)
        {
            Maincam.gameObject.SetActive(false);
        }
        
    }

    

    #endregion


    // ���⸦ NetworkIdentity�� netId�� ����� ����
    public GameObject GetWeaponPrefabByNetId(uint netId)
    {
        
        if (NetworkServer.Weaponspawned.TryGetValue(netId, out NetworkIdentity networkIdentity))
        {
            return networkIdentity.gameObject;
        }
        else
        {
            Debug.LogError($"No weapon found for NetId: {netId}");
            return null;
        }
    }

    public GameObject GetEnemyPrefabByNetId(uint netId)
    {

        if (NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity networkIdentity))
        {
            return networkIdentity.gameObject;
        }
        else
        {
            Debug.LogError($"No weapon found for NetId: {netId}");
            return null;
        }
    }
    

    public void CmdUnloadScene(PlayerManager PM,string Round)
    {
        gameSctors.RpcLoadSceneDelay(PM,Round);
    }

    public void PlayerGoNewRound(string Round)
    {
        if (!isServer) return;
        RpcLoadScene(Round);
    }

    [ClientRpc]
    public void RpcLoadScene(string Round)
    {
        Scene Addtive  = SceneManager.GetSceneByName(Round);
        if (Addtive.isLoaded)
        {
            return;
        }
        else
        {
            SceneManager.LoadSceneAsync(Round, LoadSceneMode.Additive);
        }
    }

    #region player Velocity
    [Server]
    public void SpawnChest(GameObject gameObject,int SceneIndex)
    {
        if (!isServer) return;

        NetworkServer.Spawn(gameObject);

        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByBuildIndex(SceneIndex));

    }

    [Server]
    public void EffectSpawn(GameObject gameObject)
    {
        NetworkServer.Spawn(gameObject);
    }

    #endregion

    // player -> netconection -> method
    private PlayerManager FindPlayerManagerByConnection(NetworkConnection conn)
    {
        foreach (var player in Players)
        {
            if (player.connectionToClient == conn)
            {
                return player;
            }
        }
        return null;
    }

    public void TutorialEnemySpawn()
    {
        TS.spawnTest();
    }

    // end game

    public void EndGame()
    {
        if (isServer)
        {
            //gameSctors.player.Remove();
        }
        else
        {
            //TargetRpc -> �ش��ϴ� Ŭ���̾�Ʈ�� �Ѿ��
            
            SceneManager.LoadScene("Ending");
        }
    }

    [Server]
    public void HandleClientSceneRequest(NetworkConnection conn, string sceneName)
    {
        if (IsScenePreloaded(sceneName))
        {
            conn.Send(new SceneMessage { sceneName = sceneName });
        }
        else
        {
            // �ʿ��� ��� ���� �̸� �ε��ϰ� Ŭ���̾�Ʈ���� �߰� �ε� ����
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    private bool IsScenePreloaded(string sceneName)
    {
        // ���� �ε�� �� ��Ͽ��� �ش� ���� �ִ��� Ȯ��
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
                return true;
        }
        return false;
    }

    public void SyncScenesWithClient(NetworkConnection conn)
    {
        foreach(var player in NetworkServer.spawned.Values)
        {
            if (player.gameObject.CompareTag("Player"))
            {
                if (player.GetComponent<PlayerState>().Round != "Tutorial")
                {
                    Debug.Log("Ʃ�丮���� �ƴ϶� �ٸ� �� ����");
                    TargetLoadScene(conn, player.GetComponent<PlayerState>().Round);
                    break;
                }
                else if(player.GetComponent<PlayerState>().Round == "Tutorial")
                {
                    gameSctors.SyncSectorStates(conn, "Tutorial");
                }
            }
        }
    }


    [TargetRpc]
    void TargetLoadScene(NetworkConnection target, string sceneName)
    {
        Debug.Log("TargetLoadScene");
        // Ŭ���̾�Ʈ���� ���� Additive�� �ε�
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    [Server]
    public void EnemySpawnInNewRound(string Round)
    {
        if(!isServer) return;

        StartCoroutine(CheckScene(Round));
    }

    IEnumerator CheckScene(string Round)
    {
        Scene scene = SceneManager.GetSceneByName(Round);

        while(!scene.isLoaded)
        {
            yield return null;
        }

        switch (Round)
        {
            case "1Round":
                ESM.SpawnEnemy(Stage.Stage2, "1Round");
                BS.Spawnboss();
                break;
            case "2Round":
                ESM.SpawnEnemy(Stage.Stage3, "2Round");
                break;
        }
    }

}