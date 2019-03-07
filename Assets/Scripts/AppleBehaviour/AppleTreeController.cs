using System.Collections;
using UnityEngine;

public class AppleTreeController : MonoBehaviour, IEventListener<GameStartEvent>, IEventListener<GameEndEvent>
{
    [Header("Spawn Settings")]
    [SerializeField]
    protected Transform[] spawns;
    [SerializeField]
    [Range(1.0f, 100.0f)]
    protected float redAppleSpawnChance = 25.0f;
    [SerializeField]
    [Range(1.0f, 100.0f)]
    protected float goldenAppleSpawnChance = 5.0f;
    [SerializeField]
    protected float spawnEveryAmount = 8.0f;

    [Header("Apple Settings")]
    [SerializeField]
    protected GameObject redApple;
    [SerializeField]
    protected GameObject goldenApple;
    
    protected GenericPool redApplePool, goldenApplePool;
    protected Coroutine appleSpawnCoroutine;

    protected virtual void Awake()
    {
        redApplePool = GenericPool.Create(gameObject, spawns.Length * 2, redApple);
        goldenApplePool = GenericPool.Create(gameObject, spawns.Length, goldenApple);

        EventManager<GameStartEvent>.AddListener(this);
        EventManager<GameEndEvent>.AddListener(this);
    }

    protected virtual IEnumerator SpawnApples()
    {
        while (true)
        {
            SpawnApples(redApplePool, redAppleSpawnChance);
            SpawnApples(goldenApplePool, goldenAppleSpawnChance);
            yield return new WaitForSeconds(spawnEveryAmount);
        }
    }

    protected virtual void SpawnApples(GenericPool pool, float chance)
    {
        for (var i = 0; i < spawns.Length; i++)
        {
            if (Random.Range(1, 100) <= chance)
            {
                var apple = pool.GetActiveGameObject();
                apple.GetComponent<AppleController>().originalPool = pool;
                apple.transform.position = spawns[i].position;
                apple.transform.rotation = spawns[i].rotation;
            }
        }
    }

    public virtual void OnEvent(GameStartEvent eventData) => appleSpawnCoroutine = StartCoroutine(SpawnApples());

    public virtual void OnEvent(GameEndEvent eventData) => StopCoroutine(appleSpawnCoroutine);
}
