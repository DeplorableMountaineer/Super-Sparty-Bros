using UnityEngine;

// this comes from the Unity Standard Assets
public class Camera2DFollow : MonoBehaviour
{
    public Transform target;
    public float damping = 1;
    public float lookAheadFactor = 3;
    public float lookAheadReturnSpeed = 0.5f;
    public float lookAheadMoveThreshold = 0.1f;
    private Vector3 _mCurrentVelocity;
    private Vector3 _mLastTargetPosition;
    private Vector3 _mLookAheadPos;

    // private variables
    private float _mOffsetZ;

    // Use this for initialization
    private void Start()
    {
        Vector3 position = target.position;
        _mLastTargetPosition = position;
        Transform transform1 = transform;
        _mOffsetZ = (transform1.position - position).z;
        transform1.parent = null;

        // if target not set, then set it to the player
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;

        if (target == null)
            Debug.LogError("Target not set on Camera2DFollow.");
    }

    // Update is called once per frame
    private void Update()
    {
        if (target == null)
            return;

        // only update lookahead pos if accelerating or changed direction
        float xMoveDelta = (target.position - _mLastTargetPosition).x;

        bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

        if (updateLookAheadTarget)
            _mLookAheadPos = Vector3.right * (lookAheadFactor * Mathf.Sign(xMoveDelta));
        else
            _mLookAheadPos =
                Vector3.MoveTowards(_mLookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);

        Vector3 position = target.position;
        Vector3 aheadTargetPos = position + _mLookAheadPos + Vector3.forward * _mOffsetZ;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref _mCurrentVelocity, damping);

        transform.position = newPos;

        _mLastTargetPosition = position;
    }
}