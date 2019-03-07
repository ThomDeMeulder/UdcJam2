using UnityEngine;
using UnityEngine.XR;

public class MotionController : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField]
    protected XRNode type = XRNode.RightHand;
    [SerializeField]
    protected float grabThreshold = 0.6f;
    [SerializeField]
    protected string grabInputName;
    [SerializeField]
    protected string joystickVerticalInputName;
    [SerializeField]
    protected string joystickHorizontalInputName;
    [SerializeField]
    protected float joystickThreshold = 0.8f;

    [Header("Teleport Settings")]
    [SerializeField]
    protected GameObject prefab;

    protected GameObject teleportGameObject = null;
    protected bool grabThresholdReached, wantsToTeleport, gameEnded;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        switch(type)
        {
            case XRNode.CenterEye:
            case XRNode.GameController:
            case XRNode.HardwareTracker:
            case XRNode.Head:
            case XRNode.LeftEye:
            case XRNode.RightEye:
            case XRNode.TrackingReference:
                type = XRNode.RightHand;
                Debug.LogWarning("Motion Controller type must be of Right or Left!");
                break;
        }
    }
#endif

    protected virtual void Update()
    {
        UpdateHandLocationAndRotation();

        var vertical = Input.GetAxisRaw(joystickVerticalInputName);
        if (CheckIfWantsToTeleportAndCan(vertical)) Teleport(Input.GetAxisRaw(joystickHorizontalInputName), vertical);

        if (!(Player.Instance.GameState == GameState.Start || Player.Instance.GameState == GameState.PreEnd)) return;

        var grabInput = Input.GetAxisRaw(grabInputName);
        if (CheckPickupItem(grabInput)) TriggerItem();
    }

    protected virtual bool CheckIfWantsToTeleportAndCan(float input)
    {
        if (!wantsToTeleport && IsJoystickDown(input)) wantsToTeleport = true;
        return wantsToTeleport;
    }

    protected virtual void Teleport(float horizontal, float vertical)
    {
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit, 30.0f))
        {
            if (!hit.collider.CompareTag("Ground"))
            {
                DestroyTeleportGameObject();
                return;
            }

            var forwardDirection = hit.point - Player.Instance.transform.position;

            var rotationPoint = hit.point;
            rotationPoint += forwardDirection * vertical;
            rotationPoint += Quaternion.Euler(0.0f, 90.0f, 0.0f) * forwardDirection * -horizontal;

            UpdateTeleportGameObject(hit.point, -(rotationPoint - hit.point));

            if (!IsJoystickDown(horizontal) && !IsJoystickDown(vertical))
            {
                wantsToTeleport = false;

                var angle = Vector3.SignedAngle(forwardDirection, -(rotationPoint - hit.point), Vector3.up);
                Player.Instance.Teleport(hit.point, angle);
                DestroyTeleportGameObject();
                EventManager<PlayerTeleportEvent>.CallEvent(new PlayerTeleportEvent());
            }
        }
        else DestroyTeleportGameObject();
    }

    protected virtual void DestroyTeleportGameObject()
    {
        if (teleportGameObject == null) return;
        teleportGameObject.SetActive(false);
    }

    protected virtual void UpdateTeleportGameObject(Vector3 position, Vector3 rotation)
    {
        if (teleportGameObject == null)
        {
            teleportGameObject = Instantiate(prefab, position, Quaternion.LookRotation(rotation));
            UpdateRotation(teleportGameObject.transform);
            return;
        }

        if (!teleportGameObject.activeSelf) teleportGameObject.SetActive(true);
        teleportGameObject.transform.position = position;
        teleportGameObject.transform.forward = rotation;
        UpdateRotation(teleportGameObject.transform);
    }

    protected virtual bool IsJoystickDown(float input) => input >= joystickThreshold || input <= -joystickThreshold;

    protected virtual void UpdateHandLocationAndRotation()
    {
        transform.localPosition = InputTracking.GetLocalPosition(type);
        transform.localRotation = InputTracking.GetLocalRotation(type);
    }

    protected virtual void UpdateRotation(Transform transform)
    {
        var position = transform.position;
        position.y += 0.05f;
        transform.position = position;

        var local = transform.localEulerAngles;
        local.x = 90;
        transform.localEulerAngles = local;
    }

    protected virtual bool CheckPickupItem(float input)
    {
        if (grabThresholdReached && !GrabButtonDown(input)) grabThresholdReached = false;
        return GrabButtonDown(input) && !grabThresholdReached;
    }

    protected virtual void TriggerItem()
    {
        var colliders = Physics.OverlapSphere(transform.position, 0.1f);

        foreach (var collider in colliders)
        {
            var component = collider.GetComponent<IControllerTarget>();
            if (component == null) continue;

            component.OnGrab();
        }

        grabThresholdReached = true;
    }

    protected virtual bool GrabButtonDown(float input) => input >= grabThreshold;
}
