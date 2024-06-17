using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Input;


public class TouchPad : MonoBehaviour
{
    private Collider _touchCollider;
    private Dictionary<Collider, GameObject> _touchPoints;

    public Hand _leftHand;
    public Hand _rightHand;
    public GameObject _fingerTipPrefab;
    public GameObject _touchPointPrefab;

    private readonly HandJointId[] JOINT_TIPS = {
        HandJointId.HandThumbTip,
        HandJointId.HandIndexTip,
        HandJointId.HandMiddleTip,
        HandJointId.HandRingTip,
        HandJointId.HandPinkyTip
    };

    private Dictionary<HandJointId, GameObject> _leftFingerTips;
    private Dictionary<HandJointId, GameObject> _rightFingerTips;


    // Start is called before the first frame update
    void Start()
    {
        _touchCollider = this.GetComponent<Collider>();
        _touchPoints = new Dictionary<Collider, GameObject>();

        _leftFingerTips = new Dictionary<HandJointId, GameObject>();
        GameObject fingerTipsLeftObj = new GameObject("FingerTipsLeft");
        fingerTipsLeftObj.transform.SetParent(_leftHand.gameObject.transform);
        foreach (HandJointId jointId in JOINT_TIPS) {
            GameObject tipObj = Instantiate(_fingerTipPrefab, fingerTipsLeftObj.transform);
            tipObj.name = "Left" + jointId.ToString();
            _leftFingerTips.Add(jointId, tipObj);
            tipObj.SetActive(false);
        }
        
        _rightFingerTips = new Dictionary<HandJointId, GameObject>();
        GameObject fingerTipsRightObj = new GameObject("FingerTipsRight");
        fingerTipsRightObj.transform.SetParent(_rightHand.gameObject.transform);
        foreach (HandJointId jointId in JOINT_TIPS) {
            GameObject tipObj = Instantiate(_fingerTipPrefab, fingerTipsRightObj.transform);
            tipObj.name = "Right" + jointId.ToString();
            tipObj.SetActive(false);
            _rightFingerTips.Add(jointId, tipObj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Left Hand Tips
        if (_leftHand.IsHighConfidence) {
            foreach (var pair in _leftFingerTips) {
                pair.Value.SetActive(true);
                Pose tipPose;
                _leftHand.GetJointPose(pair.Key, out tipPose);
                pair.Value.transform.position = tipPose.position;
            }
        }
        else {
            foreach (var pair in _leftFingerTips) {
                pair.Value.SetActive(false);
            }
        }

        // Right Hand Tips
        if (_rightHand.IsHighConfidence) {
            foreach (var pair in _rightFingerTips) {
                pair.Value.SetActive(true);
                Pose tipPose;
                _rightHand.GetJointPose(pair.Key, out tipPose);
                pair.Value.transform.position = tipPose.position;
            }
        }
        else {
            foreach (var pair in _rightFingerTips) {
                pair.Value.SetActive(false);
            }
        }
    }


    void OnTriggerEnter(Collider collider) 
    {
        Debug.Log(collider.name);
        _touchPoints.Add(collider, Instantiate(_touchPointPrefab, this.gameObject.transform));
    }

    void OnTriggerStay(Collider collider) 
    {
        Vector3 contact = _touchCollider.ClosestPoint(collider.transform.position);
        _touchPoints[collider].transform.position = contact;
    }

    void OnTriggerExit(Collider collider) 
    {
        Destroy(_touchPoints[collider]);
        _touchPoints.Remove(collider);
    }

}
