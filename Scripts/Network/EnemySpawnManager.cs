using AYellowpaper.SerializedCollections;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawnManager : NetworkBehaviour
{
    [SerializedDictionary("Stage", "Enemy")]
    public SerializedDictionary<Stage, List<NetworkIdentity>> m_SpawnList = new SerializedDictionary<Stage, List<NetworkIdentity>>();
    [SerializedDictionary("Stage", "Position")]
    public SerializedDictionary<string,List<Transform>> SpawnPoint = new SerializedDictionary<string, List<Transform>>();

    public int spawnCount = 0;
    
    public  void SetSpawnPoint(string Stage,List<Transform> spawnPoint)
    {
        SpawnPoint.Add(Stage, spawnPoint);
    }

    [Server]
    public void SpawnEnemy(Stage stage,string SceneName)
    {
        Debug.Log("SpawnEnemy");
        List<uint> SpawnList = new List<uint>();
        spawnCount = 0;
        if(GameManager.Instance.gameSctors.player.Count > 0)
        {
            if (GameManager.Instance.SpawnEnemys.TryGetValue(SceneName, out List<uint> list2))
            {
                Debug.Log("A");
                return;
            }
            else
            {
                for (int j = 0; j < GameManager.Instance.gameSctors.player.Count; j++)
                {
                    if (GameManager.Instance.gameSctors.player[j].gameObject.scene.name == SceneName)
                    {
                        SpawnPoint.TryGetValue(SceneName, out List<Transform> point2);
                        for(int i=0;i<point2.Count; i++)
                        {
                            if (spawnCount >= point2.Count)
                            {
                                GameManager.Instance.SpawnEnemys.Add(SceneName, SpawnList);
                                break;
                            }
                            Transform spawnPosition = null;
                            int index = Random.Range(0, point2.Count);
                            spawnPosition = point2[index];

                            var enemy = Instantiate(m_SpawnList[stage][Random.Range(0, m_SpawnList[stage].Count)], spawnPosition.position, Quaternion.identity);
                            Debug.Log(enemy.GetComponent<NetworkIdentity>().sceneId);
                            
                            NetworkServer.Spawn(enemy.gameObject);
                            SpawnList.Add(enemy.netId);
                            spawnCount++;
                            Debug.Log("spawned");
                            
                        }
                    }

                }
            }
        }
        else if("Tutorial" == SceneName)
        {
            SpawnPoint.TryGetValue(SceneName, out List<Transform> point);
            for (int i = 0; i < point.Count; i++)
            {
                if (spawnCount >= point.Count)
                {
                    GameManager.Instance.SpawnEnemys.Add(SceneName, SpawnList);
                    break;
                }
                Transform spawnPosition = null;
                int index = Random.Range(0, point.Count);
                spawnPosition = point[index];

                var enemy = Instantiate(m_SpawnList[stage][0], spawnPosition.position, Quaternion.identity);

                if (enemy.CompareTag("Enemy"))
                {
                    NetworkServer.Spawn(enemy.gameObject);
                    SpawnList.Add(enemy.netId);
                    Debug.Log("Spawn Tutorial Enemy");
                    spawnCount++;
                }
            }
        }
    }
    //특정조건에서 게임매니져를 통해 불려서 소환

    public void EventSpawn(Stage stage,string SceneName)
    {
        var enemy = Instantiate(m_SpawnList[stage][0],transform.position, Quaternion.identity);
        List<uint> SpawnList = new List<uint>();
        SpawnList.Add(enemy.netId);

        if (enemy.CompareTag("Enemy"))
        {
            NetworkServer.Spawn(enemy.gameObject);
        }
        GameManager.Instance.SpawnEnemys.Add(SceneName, SpawnList);
    }
   
    public void ReSpawn(GameObject g , float time)
    {
        StartCoroutine(Re(g,time));
        Debug.Log("REspawn");
    }
    private IEnumerator Re(GameObject g,float time)
    {
        yield return YieldCache.WaitForSeconds(time);
        g.SetActive(true);
        ClientReSpawn(g);
    }
    [ClientRpc]
    private void ClientReSpawn(GameObject g)
    {
        g.SetActive(true);
    }
}

public enum Stage
{
    Stage1 = 1,
    Stage2 = 2,
    Stage3 = 3,
    BossStage,
}