using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AppleController : MonoBehaviour, IControllerTarget
{
    [Header("Apple Settings")]
    [SerializeField]
    protected int points;

    [System.NonSerialized]
    public GenericPool originalPool;

    protected bool xNegative, yNegative, zNegative;

    private void Awake()
    {
        xNegative = ShouldBeNegative();
        yNegative = ShouldBeNegative();
        zNegative = ShouldBeNegative();
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Ground")) return;
        originalPool.ReturnObject(gameObject);
    }

    protected virtual bool ShouldBeNegative() => Random.Range(0, 2) == 0;

    protected virtual void Update() => transform.Rotate(GetRandomRotation(xNegative), GetRandomRotation(yNegative), GetRandomRotation(zNegative));

    protected virtual float GetRandomRotation(bool negative) => negative ? -GetRandomRotation() : GetRandomRotation();

    protected virtual float GetRandomRotation() => Random.Range(5, 10) * Time.deltaTime;

    public virtual void OnGrab()
    {
        Player.Instance.AddPoints(points);
        originalPool.ReturnObject(gameObject);
    }
}
