using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;


public class TouchHand : MonoBehaviour
{
    public Hand _hand;
    public FingerFeatureStateProvider _fingerFeatures;

    public GameObject _fingerTipPrefab;
    private Dictionary<HandFinger, GameObject> _fingerTouches;


    // index: 0->wrist / 1->joint1 / 2->joint2 / 3->joint3 / 4->tip / 5->pad
    public enum FingerJointIndex {
        Root = 0,
        Proximal = 1,
        Middle = 2,
        Distal = 3,
        Tip = 4, 
        Pad = 5
    }
    private readonly Dictionary<HandFinger, HandJointId[]> FINGER_JOINTS = new Dictionary<HandFinger, HandJointId[]> {
        {HandFinger.Thumb, new HandJointId[] {HandJointId.HandWristRoot, HandJointId.HandThumb1, HandJointId.HandThumb2, HandJointId.HandThumb3, HandJointId.HandThumbTip}},
        {HandFinger.Index, new HandJointId[] {HandJointId.HandWristRoot, HandJointId.HandIndex1, HandJointId.HandIndex2, HandJointId.HandIndex3, HandJointId.HandIndexTip}},
        {HandFinger.Middle, new HandJointId[] {HandJointId.HandWristRoot, HandJointId.HandMiddle1, HandJointId.HandMiddle2, HandJointId.HandMiddle3, HandJointId.HandMiddleTip}},
        {HandFinger.Ring, new HandJointId[] {HandJointId.HandWristRoot, HandJointId.HandRing1, HandJointId.HandRing2, HandJointId.HandRing3, HandJointId.HandRingTip}},
        {HandFinger.Pinky, new HandJointId[] {HandJointId.HandWristRoot, HandJointId.HandPinky1, HandJointId.HandPinky2, HandJointId.HandPinky3, HandJointId.HandPinkyTip}}
    };
    public Transform[] _fingerPads;

    private Dictionary<HandFinger, Pose[]> _currentHandJointPoses;
    public Dictionary<HandFinger, Pose[]> GetCurrentHandJointPoses() {
        return _currentHandJointPoses;
    }

    private Dictionary<HandFinger, Pose[]> _previousHandJointPoses;
    public Dictionary<HandFinger, Pose[]> GetPreviousHandJointPoses() {
        return _previousHandJointPoses;
    }


    // Start is called before the first frame update
    void Start()
    {
        _fingerTouches = new Dictionary<HandFinger, GameObject>();
        foreach (var finger in FINGER_JOINTS) {
            GameObject touchObj = Instantiate(_fingerTipPrefab, this.gameObject.transform);
            touchObj.GetComponent<TouchColliderInfo>().touchFinger = 
                new TouchFinger(_hand.Handedness, finger.Key);
            touchObj.name = _hand.Handedness.ToString() + finger.Key.ToString() + "Touch";
            _fingerTouches.Add(finger.Key, touchObj);
            touchObj.SetActive(false);
        }

        _currentHandJointPoses = new Dictionary<HandFinger, Pose[]>();
        _previousHandJointPoses = new Dictionary<HandFinger, Pose[]>();
    }


    // Update is called once per frame
    void Update()
    {
        _previousHandJointPoses = _currentHandJointPoses;

        // Check confidence
        if (!_hand.IsHighConfidence) {
            foreach (var touch in _fingerTouches) {
                touch.Value.SetActive(false);
            }
            return;
        }

        _currentHandJointPoses = GetFingerJointPoses();

        // Update Colliders
        foreach (var touch in _fingerTouches) {
            if (CheckFingerSpread(touch.Key)) {
                touch.Value.transform.position = _currentHandJointPoses[touch.Key][(int) FingerJointIndex.Pad].position;
                touch.Value.SetActive(true);
            }
            else {
                touch.Value.SetActive(false);
            }
        }
    }


    private bool CheckFingerSpread(HandFinger finger) {
        float curl, flextion;
        try {
            curl = (float) _fingerFeatures.GetFeatureValue(finger, FingerFeature.Curl);
            flextion = (float) _fingerFeatures.GetFeatureValue(finger, FingerFeature.Flexion);
            // if (curl < 225f && flextion < 225f) {
            if (curl < 225f) {   
                return true;
            }
            else {
                return false;
            }
        } catch {
            return false;
        }
    }


    private Dictionary<HandFinger, Pose[]> GetFingerJointPoses() {
        Dictionary<HandFinger, Pose[]> fingerPoses = new Dictionary<HandFinger, Pose[]>();
        foreach (var finger in FINGER_JOINTS) {
            Pose[] jointPoses = new Pose[finger.Value.Length + 1];
            for (int i = 0; i < finger.Value.Length; i++) {
                jointPoses[i] = GetJointPose(finger.Value[i]);
            }
            jointPoses[finger.Value.Length] = 
                new Pose(_fingerPads[(int) finger.Key].position, _fingerPads[(int) finger.Key].rotation);
            fingerPoses.Add(finger.Key, jointPoses);
        }
        return fingerPoses;
    }

    private Pose GetJointPose(HandJointId jointId) {
        Pose jointPose = Pose.identity;
        _hand.GetJointPose(jointId, out jointPose);
        return jointPose;
    }
}
