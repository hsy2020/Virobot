using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class PlayerControls : MonoBehaviour {

    [SerializeField] Transform m_manipulator, m_ikTip, m_connectionPoint, m_topClaw, m_bottomClaw;
    [SerializeField] float m_acceptedError, m_vibrationStrength = 1, m_inBetweenShakeTime = 0.01f, m_manipulatorMinHeight = 0, m_pickupRadius = 2, m_clawMoveAmount = 0.01f;
    [SerializeField] BioIK.IKBody m_body;
    [SerializeField] int m_nShakes = 5;
    [SerializeField] Text m_playText, m_recordText, m_poseText, m_lockRotationText;
    [SerializeField] Color m_activeColor = Color.red, m_futureColor = Color.white, m_pastColor = Color.red, m_freeModeColor = Color.white, m_lockModeColor = Color.green, m_unactive = Color.gray;
    [SerializeField] string m_freeModeText = "Free Rotation", m_lockModeText = "Locked Rotation";
    [SerializeField] FixedJoint m_joint;

    [SerializeField] SteamVR_TrackedController m_left, m_right;
    Vector3? m_lastPos = null;
    List<Pose> m_recording = new List<Pose>();
    ModeState m_mode = ModeState.PoseMode;
    MovementState m_movementMode = MovementState.Free;
    int m_cursor = 0;
    Color m_disabledColor;
    LineRenderer m_recordingLine;
    bool m_active = false, m_grabbing = false;
    float m_grabbyness = 0;
    Vector3 m_topClawOrig, m_bottomClawOrig;
    Pose m_currentPose;

    enum ModeState {
        PoseMode,
        Recording,
        Playing
    }

    enum MovementState {
        Free,
        Locked
    }

    void Awake() {
        m_recordingLine = GetComponent<LineRenderer>();
        m_lastPos = m_right.transform.position;
        m_right.PadClicked += Pad;
        m_right.Gripped += ToggleGrab;
        m_disabledColor = m_playText.color;
        m_poseText.color = m_activeColor;
        m_lockRotationText.text = m_freeModeText;
        m_lockRotationText.color = m_freeModeColor;
        m_right.MenuButtonClicked += MenuToggle;
        m_topClawOrig = m_topClaw.transform.localPosition;
        m_bottomClawOrig = m_bottomClaw.transform.localPosition;
    }

    private void MenuToggle(object sender, ClickedEventArgs e) {
        if (m_mode != ModeState.Playing) {
            if (m_movementMode == MovementState.Free) {
                m_movementMode = MovementState.Locked;
                m_lockRotationText.text = m_lockModeText;
                RotateDown();
            } else {
                m_movementMode = MovementState.Free;
                m_lockRotationText.text = m_freeModeText;
            }
            UpdateLockRotationTextColor();
        }
    }

    void ToggleGrab(object sender, ClickedEventArgs e) {
        if (m_mode != ModeState.Playing) {
            m_grabbing = !m_grabbing;
            if (m_mode == ModeState.Recording)
                m_currentPose.Action = m_grabbing ? Pose.Effector.Close : Pose.Effector.Open;
            if (m_grabbing)
                Grab();
            else
                Release();
        }
    }

    void Grab() {
        var things = Physics.OverlapSphere(m_connectionPoint.transform.position, m_pickupRadius, 1 << LayerMask.NameToLayer("Pickup"));
        foreach (var thing in things) {
            var body = thing.GetComponent<Rigidbody>();
            if (!body && thing.transform.parent)
                body = thing.transform.parent.GetComponentInParent<Rigidbody>();
            if (body)
                m_joint.connectedBody = body;
        }
        m_topClaw.transform.localPosition += Vector3.forward * m_clawMoveAmount;
        m_bottomClaw.transform.localPosition += Vector3.back * m_clawMoveAmount;
    }

    void Release() {
        if(m_joint.connectedBody != null)
            m_joint.connectedBody.WakeUp();
        m_joint.connectedBody = null;
        m_topClaw.transform.localPosition = m_topClawOrig;
        m_bottomClaw.transform.localPosition = m_bottomClawOrig;
    }

    private void Pad(object sender, ClickedEventArgs e) {
        Gradient gradient = new Gradient();
        GradientColorKey[] colors = { new GradientColorKey(m_futureColor, 0) };
        GradientAlphaKey[] alphas = { new GradientAlphaKey(1, 0) };
        gradient.SetKeys(colors, alphas);
        m_recordingLine.colorGradient = gradient;
        var val = (e.padY + 1) / 2;
        if (val < 1f/3) {
            m_mode = ModeState.PoseMode;
            m_playText.color = m_disabledColor;
            m_recordText.color = m_disabledColor;
            m_poseText.color = m_activeColor;
        } else if(val < 2f/3) {
            m_mode = ModeState.Recording;
            m_cursor = 0;
            m_playText.color = m_disabledColor;
            m_recordText.color = m_activeColor;
            m_poseText.color = m_disabledColor;
            m_recording.Clear();
            m_recordingLine.positionCount = 0;
        } else {
            m_mode = ModeState.Playing;
            m_playText.color = m_activeColor;
            m_recordText.color = m_disabledColor;
            m_poseText.color = m_disabledColor;
        }
        UpdateLockRotationTextColor();
    }

    void UpdateLockRotationTextColor() {
        if (m_mode == ModeState.Playing) {
            m_lockRotationText.color = m_unactive;
        } else if (m_movementMode == MovementState.Free) {
            m_lockRotationText.color = m_freeModeColor;
        } else {
            m_lockRotationText.color = m_lockModeColor;
        }
    }

    void Update() {
        m_grabbyness = m_right.controllerState.rAxis1.x;
        m_active = m_grabbyness > 0;
        switch (m_mode) {
            case ModeState.PoseMode:
                MoveManipulator();
                break;
            case ModeState.Recording:
                MoveManipulator();
                if (m_active) {
                    m_currentPose = new Pose(m_manipulator.transform);
                }
                break;
            case ModeState.Playing:
                if (m_recording.Count > 0) {
                    Pose pose = m_recording[m_cursor];
                    m_manipulator.transform.position = pose.Position;
                    m_manipulator.transform.rotation = pose.Rotation;
                    switch (pose.Action) {
                        case Pose.Effector.Close:
                            m_grabbing = true;
                            Grab();
                            break;
                        case Pose.Effector.Open:
                            m_grabbing = false;
                            Release();
                            break;
                    }
                    m_cursor++;
                    if (m_cursor >= m_recording.Count)
                        m_cursor = 0;
                }
                break;
        }
    }

    void LateUpdate() {
        if (m_mode == ModeState.Recording && m_active) {
            m_recording.Add(m_currentPose);
            m_recordingLine.positionCount = m_recording.Count;
            m_recordingLine.SetPosition(m_recording.Count - 1, m_manipulator.transform.position);
        }
        if (m_mode != ModeState.Playing && m_active && !m_body.HasChanged) {
            if (Vector3.Distance(m_ikTip.position, m_manipulator.position) >= m_acceptedError) {
                m_manipulator.position = m_ikTip.position;
                StartCoroutine(Vibrate());
            }
        }
    }

    void MoveManipulator() {
        if (m_active) {
            m_manipulator.transform.position += m_grabbyness * (m_right.transform.position - m_lastPos.Value);
            if(m_manipulator.transform.position.y < m_manipulatorMinHeight) {
                var a = m_manipulator.transform.position;
                a.y = m_manipulatorMinHeight;
                m_manipulator.position = a;
            }
            switch (m_movementMode) {
                case MovementState.Free:
                    m_manipulator.transform.rotation = Quaternion.AngleAxis(90, m_right.transform.forward) * m_right.transform.rotation;
                    break;
                case MovementState.Locked:
                    RotateDown();
                    break;
            }

        }
        m_lastPos = m_right.transform.position;
    }

    void RotateDown() {
        m_manipulator.transform.rotation = Quaternion.Euler(0, m_right.transform.localEulerAngles.y , 90);
    }

    IEnumerator Vibrate() {
        for (int i = 0; i < m_nShakes; i++) {
            ushort power = (ushort)Mathf.FloorToInt(m_vibrationStrength * 3999);
            SteamVR_Controller.Input((int)m_right.controllerIndex).TriggerHapticPulse(power);
            yield return new WaitForSeconds(m_inBetweenShakeTime);
        }
    }

    struct Pose {

        public enum Effector {
            Nothing,
            Open,
            Close
        }

        public Vector3 Position;
        public Quaternion Rotation;
        public Effector Action;

        public Pose(Transform transform, Effector action = Effector.Nothing) {
            Position = transform.position;
            Rotation = transform.rotation;
            Action = action;
        }

    }

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_connectionPoint.position, m_pickupRadius);
    }

}