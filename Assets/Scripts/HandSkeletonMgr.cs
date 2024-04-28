using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://developer.oculus.com/documentation/unity/unity-handtracking/?locale=fr_FR
using Oculus.Interaction.Input;


public class HandSkeleton 
{
    private Hand _hand;
    private string _handedness;
    private GameObject _handSkeletonObj;
    public GameObject GetHandSkeletonObj() {
        return _handSkeletonObj;
    }

    private Dictionary<HandJointId, GameObject> _handJoints;
    private Dictionary<HandJointId, GameObject> _handLinks;
    private Dictionary<HandJointId, GameObject> _handTips;


    readonly Dictionary<string, HandJointId[]> BONES = new Dictionary<string, HandJointId[]>() 
    {
        {"Thumb", new HandJointId[] {HandJointId.HandThumb1, HandJointId.HandThumb2, HandJointId.HandThumb3, HandJointId.HandThumbTip}},
        {"Index", new HandJointId[] {HandJointId.HandIndex1, HandJointId.HandIndex2, HandJointId.HandIndex3, HandJointId.HandIndexTip}},
        {"Middle", new HandJointId[] {HandJointId.HandMiddle1, HandJointId.HandMiddle2, HandJointId.HandMiddle3, HandJointId.HandMiddleTip}},
        {"Ring", new HandJointId[] {HandJointId.HandRing1, HandJointId.HandRing2, HandJointId.HandRing3, HandJointId.HandRingTip}},
        {"Pinky", new HandJointId[] {HandJointId.HandPinky1, HandJointId.HandPinky2, HandJointId.HandPinky3, HandJointId.HandPinkyTip}}
    };


    public HandSkeleton(Hand hand) {
        _hand = hand;

        if(hand.Handedness == Handedness.Left) {
            _handedness = "Left";
        }
        else {
            _handedness = "Right";
        }

        _handSkeletonObj = new GameObject();
        _handSkeletonObj.name = _handedness + "HandSkeleton";

        _handJoints = new Dictionary<HandJointId, GameObject>();
        _handLinks = new Dictionary<HandJointId, GameObject>();
        _handTips = new Dictionary<HandJointId, GameObject>();
    }


    public void SetSkeleton(GameObject jointObj, GameObject linkObj, GameObject tipObj) { 
        // WristRoot Object
        GameObject root = GameObject.Instantiate(jointObj, _handSkeletonObj.transform);
        root.name = "WristRoot";
        root.SetActive(false);
        _handJoints.Add(HandJointId.HandWristRoot, root);
        // Finger Objects
        foreach (var item in BONES) {
            GameObject finger = new GameObject();
            finger.name = item.Key;
            finger.transform.SetParent(_handSkeletonObj.transform);

            for (int i = 0; i < item.Value.Length - 1; i++) {
                // Finger Joints
                GameObject joint = GameObject.Instantiate(jointObj, finger.transform);
                joint.name = item.Key + "Joint" + (i + 1);
                joint.SetActive(false);
                _handJoints.Add(item.Value[i], joint);
                // Finger Links
                GameObject link = GameObject.Instantiate(linkObj, finger.transform);
                link.name = item.Key + "Link" + (i + 1);
                link.SetActive(false);
                _handLinks.Add(item.Value[i], link);
            }
            // Finger Tips
            GameObject tip = GameObject.Instantiate(tipObj, finger.transform);
            tip.name = item.Key + "Tip";
            tip.SetActive(false);
            _handJoints.Add(item.Value[item.Value.Length - 1], tip);
        }
    }


    public void ProcessSkeleton() {
        // WristRoot Object
        HandJointId rootId = HandJointId.HandWristRoot;
        Pose rootPose = Pose.identity;
        if (_hand.GetJointPose(rootId, out rootPose)) {
            _handJoints[rootId].SetActive(true);
            _handJoints[rootId].transform.position = rootPose.position;
            _handJoints[rootId].transform.rotation = rootPose.rotation;
        }
        else {
            _handJoints[rootId].SetActive(false);
        }
        // Finger Objects
        foreach (var item in BONES) {
            for (int i = 0; i < item.Value.Length - 1; i++) {
                HandJointId startId = item.Value[i];
                HandJointId endId = item.Value[i + 1];
                Pose startPose = Pose.identity;
                Pose endPose = Pose.identity;
                bool startValid = _hand.GetJointPose(startId, out startPose);
                bool endValid = _hand.GetJointPose(endId, out endPose);

                // Finger Joints
                if (startValid) {
                    _handJoints[startId].SetActive(true);
                    _handJoints[startId].transform.position = startPose.position;
                    _handJoints[startId].transform.rotation = startPose.rotation;
                }
                else {
                    _handJoints[startId].SetActive(false);
                }
                // Finger Links
                if (startValid && endValid) {
                    _handLinks[startId].SetActive(true);
                    _handLinks[startId].transform.position = 0.5f * (startPose.position + endPose.position);
                    Vector3 sub = endPose.position - startPose.position;
                    _handLinks[startId].transform.up = sub.normalized;
                    _handLinks[startId].transform.localScale = new Vector3(
                        _handLinks[startId].transform.localScale.x, 
                        0.5f * sub.magnitude,  
                        _handLinks[startId].transform.localScale.z
                    );
                    float scaleY = _handLinks[startId].transform.localScale.y / _handLinks[startId].transform.localScale.x;
                    var capsule = _handLinks[startId].GetComponent<CapsuleCollider>();
                    capsule.height = 
                        1f + 2f * capsule.radius + 1f / scaleY;
                }
                else {
                    _handLinks[startId].SetActive(false);
                }
            }
            // Finger Tips
            HandJointId tipId = item.Value[item.Value.Length - 1];
            Pose tipPose = Pose.identity;
            if (_hand.GetJointPose(tipId, out tipPose)) {
                _handJoints[tipId].SetActive(true);
                _handJoints[tipId].transform.position = tipPose.position;
                _handJoints[tipId].transform.rotation = tipPose.rotation;
            }
            else {
                _handJoints[tipId].SetActive(false);
            }
        }
    }

}


public class HandSkeletonMgr : MonoBehaviour
{
    [SerializeField] Hand _leftHand;
    [SerializeField] Hand _rightHand;

    private HandSkeleton _leftHandSkeleton;
    private HandSkeleton _rightHandSkeleton;

    private Dictionary<HandJointId, GameObject> _leftHandJoints;
    private Dictionary<HandJointId, GameObject> _rightHandJoints;

    public GameObject _jointPrefab;
    public GameObject _linkPrefab;
    public GameObject _tipPrefab;


    // Start is called before the first frame update
    void Start()
    {
        _leftHandSkeleton = new HandSkeleton(_leftHand);
        _leftHandSkeleton.SetSkeleton(_jointPrefab, _linkPrefab, _tipPrefab);

        _rightHandSkeleton = new HandSkeleton(_rightHand);
        _rightHandSkeleton.SetSkeleton(_jointPrefab, _linkPrefab, _tipPrefab);
    }

    // Update is called once per frame
    void Update()
    {        
        _leftHandSkeleton.ProcessSkeleton();
        _rightHandSkeleton.ProcessSkeleton();
    }

}
