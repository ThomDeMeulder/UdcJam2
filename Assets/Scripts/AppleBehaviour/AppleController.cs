using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AppleController : MonoBehaviour, IControllerTarget
{
    [Header("Apple Settings")]
    [SerializeField]
    protected int points;

    [System.NonSerialized]
    public GenericPool originalPool;

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Ground")) return;
        originalPool.ReturnObject(gameObject);
    }

    public virtual void OnGrab()
    {
        Player.Instance.AddPoints(points);
        originalPool.ReturnObject(gameObject);
    }
}
