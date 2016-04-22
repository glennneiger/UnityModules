﻿using UnityEngine;

namespace Leap.Unity.PinchUtility {

  /// <summary>
  /// Use this component on a Game Object to allow it to be manipulated by a pinch gesture.  The component
  /// allows rotation, translation, and scale of the object (RTS).
  /// </summary>
  public class LeapRTS : MonoBehaviour {

    public enum RotationMethod {
      None,
      Single,
      Full
    }

    [SerializeField]
    private LeapPinchDetector _pinchDetectorA;
    public LeapPinchDetector PinchDetectorA {
      get {
        return _pinchDetectorA;
      }
      set {
        _pinchDetectorA = value;
      }
    }

    [SerializeField]
    private LeapPinchDetector _pinchDetectorB;
    public LeapPinchDetector PinchDetectorB {
      get {
        return _pinchDetectorB;
      }
      set {
        _pinchDetectorB = value;
      }
    }

    [SerializeField]
    private RotationMethod _oneHandedRotationMethod;

    [SerializeField]
    private RotationMethod _twoHandedRotationMethod;

    [SerializeField]
    private bool _allowScale = true;

    [SerializeField]
    private float _pinchRadius = 0.08f;

    [Header("GUI Options")]
    [SerializeField]
    private KeyCode _toggleGuiState = KeyCode.None;

    [SerializeField]
    private bool _showGUI = true;

    private Transform _anchor;

    private float _defaultNearClip;

    private bool isPinchingA = false;
    private bool isPinchingB = false;

    void Start() {
      if (_pinchDetectorA == null || _pinchDetectorB == null) {
        Debug.LogWarning("Both Pinch Detectors of the LeapRTS component must be assigned. This component has been disabled.");
        enabled = false;
      }

      GameObject pinchControl = new GameObject("RTS Anchor");
      _anchor = pinchControl.transform;
      _anchor.transform.parent = transform.parent;
      transform.parent = _anchor;
    }

    void Update() {
      if (Input.GetKeyDown(_toggleGuiState)) {
        _showGUI = !_showGUI;
      }

      bool didUpdate = false;

      if (_pinchDetectorA.IsPinching != isPinchingA && Vector3.Distance(_pinchDetectorA.gameObject.transform.position, transform.position) < _pinchRadius * transform.lossyScale.x)
      {
          didUpdate = true;
          isPinchingA = _pinchDetectorA.IsPinching;
      }
      if (_pinchDetectorB.IsPinching != isPinchingB && Vector3.Distance(_pinchDetectorB.gameObject.transform.position, transform.position) < _pinchRadius * transform.lossyScale.x)
      {
          didUpdate = true;
          isPinchingB = _pinchDetectorB.IsPinching;
      }

      if (didUpdate) {
        transform.SetParent(null, true);
      }

      if ((isPinchingA && Vector3.Distance(_pinchDetectorA.gameObject.transform.position, transform.position) < _pinchRadius * transform.lossyScale.x) && (isPinchingB && Vector3.Distance(_pinchDetectorB.gameObject.transform.position, transform.position) < _pinchRadius * transform.lossyScale.x))
      {
        transformDoubleAnchor();
      }
      else if (isPinchingA && Vector3.Distance(_pinchDetectorA.gameObject.transform.position, transform.position) < _pinchRadius * transform.lossyScale.x)
      {
        transformSingleAnchor(_pinchDetectorA);
      }
      else if (isPinchingB && Vector3.Distance(_pinchDetectorB.gameObject.transform.position, transform.position) < _pinchRadius * transform.lossyScale.x)
      {
        transformSingleAnchor(_pinchDetectorB);
      }

      if (didUpdate) {
        transform.SetParent(_anchor, true);
      }
    }

    void OnGUI() {
      if (_showGUI) {
        GUILayout.Label("One Handed Settings");
        doRotationMethodGUI(ref _oneHandedRotationMethod);
        GUILayout.Label("Two Handed Settings");
        doRotationMethodGUI(ref _twoHandedRotationMethod);
        _allowScale = GUILayout.Toggle(_allowScale, "Allow Two Handed Scale");
      }
    }

    private void doRotationMethodGUI(ref RotationMethod rotationMethod) {
      GUILayout.BeginHorizontal();

      GUI.color = rotationMethod == RotationMethod.None ? Color.green : Color.white;
      if (GUILayout.Button("No Rotation")) {
        rotationMethod = RotationMethod.None;
      }

      GUI.color = rotationMethod == RotationMethod.Single ? Color.green : Color.white;
      if (GUILayout.Button("Single Axis")) {
        rotationMethod = RotationMethod.Single;
      }

      GUI.color = rotationMethod == RotationMethod.Full ? Color.green : Color.white;
      if (GUILayout.Button("Full Rotation")) {
        rotationMethod = RotationMethod.Full;
      }

      GUI.color = Color.white;

      GUILayout.EndHorizontal();
    }

    private void transformDoubleAnchor() {
      _anchor.position = (_pinchDetectorA.Position + _pinchDetectorB.Position) / 2.0f;

      switch (_twoHandedRotationMethod) {
        case RotationMethod.None:
          break;
        case RotationMethod.Single:
          Vector3 p = _pinchDetectorA.Position;
          p.y = _anchor.position.y;
          _anchor.LookAt(p);
          break;
        case RotationMethod.Full:
          Quaternion pp = Quaternion.Lerp(_pinchDetectorA.Rotation, _pinchDetectorB.Rotation, 0.5f);
          Vector3 u = pp * Vector3.up;
          _anchor.LookAt(_pinchDetectorA.Position, u);
          break;
      }

      if (_allowScale) {
        _anchor.localScale = Vector3.one * Vector3.Distance(_pinchDetectorA.Position, _pinchDetectorB.Position);
      }
    }

    private void transformSingleAnchor(LeapPinchDetector singlePinch) {
      _anchor.position = singlePinch.Position;

      switch (_oneHandedRotationMethod) {
        case RotationMethod.None:
          break;
        case RotationMethod.Single:
          Vector3 p = singlePinch.Rotation * Vector3.right;
          p.y = _anchor.position.y;
          _anchor.LookAt(p);
          break;
        case RotationMethod.Full:
          _anchor.rotation = singlePinch.Rotation;
          break;
      }

      _anchor.localScale = Vector3.one;
    }
  }
}
