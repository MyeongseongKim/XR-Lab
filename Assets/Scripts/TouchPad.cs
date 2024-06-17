using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction.Input;


[System.Serializable]
public class EventVector3 : UnityEvent<float, float, float>
{
}


public class TouchPad : MonoBehaviour
{
    public TouchHand _leftTouchHand;
    public TouchHand _rightTouchHand;

    public EventVector3 _onHandTranslated;
    public EventVector3 _onHandRotated;
    private Vector3 _netTranslation;
    private Quaternion _netRotation;

    private Collider _touchColliderBox;
    private Dictionary<Collider, GameObject> _currentTouchPointObjs;
    private Dictionary<TouchFinger, Vector3> _currentTouchPoints;
    private Dictionary<TouchFinger, Vector3> _previousTouchPoints;

    public GameObject _touchPointPrefab;


    // Start is called before the first frame update
    void Start()
    {
        _touchColliderBox = this.GetComponent<Collider>();
        _currentTouchPointObjs = new Dictionary<Collider, GameObject>();
        _currentTouchPoints = new Dictionary<TouchFinger, Vector3>();
        _previousTouchPoints = new Dictionary<TouchFinger, Vector3>();

        _netTranslation = Vector3.zero;
        _netRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        // Check collider is active. If not, destroy it.
        List<Collider> colliders = new List<Collider>(_currentTouchPointObjs.Keys);
        foreach (var collider in colliders) {
            if (!collider.gameObject.activeSelf) {
                DestroyTouchPoint(collider);
            }
        }

        if (_netTranslation != Vector3.zero) {
            _onHandTranslated.Invoke(_netTranslation.x, _netTranslation.y, _netTranslation.z);
            _netTranslation = Vector3.zero;
        }

        if (_netRotation != Quaternion.identity) {
            _onHandRotated.Invoke(_netRotation.eulerAngles.x, _netRotation.eulerAngles.y, _netRotation.eulerAngles.z);
            _netRotation = Quaternion.identity;
        }
    }


    void OnTriggerEnter(Collider collider) 
    {
        if (_currentTouchPointObjs.ContainsKey(collider)) {
            DestroyTouchPoint(collider);
        }
        CreateTouchPoint(collider);
    }

    void OnTriggerStay(Collider collider) 
    {
        UpdateTouchPoint(collider);

        TouchFinger touchFinger = collider.gameObject.GetComponent<TouchColliderInfo>().touchFinger;
        TouchHand touchHand;
        if (touchFinger.handedness == Handedness.Left) {
            touchHand = _leftTouchHand;
        HandFinger handFinger = touchFinger.handFinger;
        }
        else {
            touchHand = _rightTouchHand;
        }

        Pose[] previousFingerJointPoses = touchHand.GetPreviousHandJointPoses()[handFinger];
        Pose[] currentFingerJointPoses = touchHand.GetCurrentHandJointPoses()[handFinger];

        Vector3 previousRoot = previousFingerJointPoses[(int) TouchHand.FingerJointIndex.Root].position;
        Vector3 currentRoot = currentFingerJointPoses[(int) TouchHand.FingerJointIndex.Root].position;
        Vector3 previousStart = previousFingerJointPoses[(int) TouchHand.FingerJointIndex.Proximal].position;
        Vector3 currentStart = currentFingerJointPoses[(int) TouchHand.FingerJointIndex.Proximal].position;
        Vector3 previousPad = previousFingerJointPoses[(int) TouchHand.FingerJointIndex.Pad].position;
        Vector3 currentPad = currentFingerJointPoses[(int) TouchHand.FingerJointIndex.Pad].position;

        Vector3 previousRootToStart = previousStart - previousRoot;
        Vector3 currentRootToStart = currentStart - currentRoot;
        Vector3 previousStartToPad = previousPad - previousStart;
        Vector3 currentStratToPad = currentPad - currentStart;

        // rotation
        Vector3 rotationFrom = Vector3.ProjectOnPlane(previousRootToStart, this.transform.up);
        Vector3 rotationTo = Vector3.ProjectOnPlane(currentRootToStart, this.transform.up);
        Quaternion rotation = Quaternion.Inverse(Quaternion.FromToRotation(rotationFrom, rotationTo));
        _netRotation = Quaternion.SlerpUnclamped(Quaternion.identity, rotation, 0.2f) * _netRotation;

        // translation
        Vector3 globalFrom = Vector3.ProjectOnPlane(previousRoot, this.transform.up);
        Vector3 globalTo = Vector3.ProjectOnPlane(currentRoot, this.transform.up);
        Vector3 global = globalTo - globalFrom;
        Vector3 localFrom = Vector3.ProjectOnPlane(previousRootToStart, this.transform.up);
        Vector3 localTo = Vector3.ProjectOnPlane(currentRootToStart, this.transform.up);
        Vector3 local = currentStratToPad - Quaternion.FromToRotation(localFrom, localTo) * previousStartToPad;
        Vector3 translation = Quaternion.Inverse(this.transform.rotation) * Vector3.ProjectOnPlane(-(global + local), this.transform.up);
        _netTranslation += translation * 0.2f;
    }

    void OnTriggerExit(Collider collider) 
    {
        DestroyTouchPoint(collider);
    }


    private void CreateTouchPoint(Collider collider) {
        TouchFinger touchFinger = collider.gameObject.GetComponent<TouchColliderInfo>().touchFinger;
        Vector3 contact = _touchColliderBox.ClosestPoint(collider.transform.position);
        _currentTouchPointObjs.Add(collider, Instantiate(_touchPointPrefab, this.transform));
        _currentTouchPointObjs[collider].transform.position = contact;
        _currentTouchPoints.Add(touchFinger, _currentTouchPointObjs[collider].transform.localPosition);
        _previousTouchPoints.Add(touchFinger, _currentTouchPointObjs[collider].transform.localPosition);
    }

    private void UpdateTouchPoint(Collider collider) {
        TouchFinger touchFinger = collider.gameObject.GetComponent<TouchColliderInfo>().touchFinger;
        Vector3 contact = _touchColliderBox.ClosestPoint(collider.transform.position);
        _previousTouchPoints[touchFinger] = _currentTouchPoints[touchFinger];
        _currentTouchPointObjs[collider].transform.position = contact;
        _currentTouchPoints[touchFinger] = _currentTouchPointObjs[collider].transform.localPosition;
    }
    
    private void DestroyTouchPoint(Collider collider) {
        TouchFinger touchFinger = collider.gameObject.GetComponent<TouchColliderInfo>().touchFinger;
        Destroy(_currentTouchPointObjs[collider]);
        _currentTouchPointObjs.Remove(collider);
        _currentTouchPoints.Remove(touchFinger);
        _previousTouchPoints.Remove(touchFinger);
    }

}
