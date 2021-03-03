// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraWork.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the Camera work to follow the player
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Pun;

/// <summary>
/// Camera work. Follow a target
/// </summary>
public class CustomCameraWork : MonoBehaviourPun
{
  #region Private Fields

  [Tooltip("The distance in the local x-z plane to the target")]
  [SerializeField]
  private float distance = 7.0f;

  [Tooltip("The height we want the camera to be above the target")]
  [SerializeField]
  private float height = 3.0f;
  private float initialHeight;
  private Quaternion initialDirection;

  [Tooltip("The amount right to left offcenter the camera is")]
  [SerializeField]
  private float offCenter = 1.0f;

  [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
  [SerializeField]
  private Vector3 centerOffset = Vector3.zero;

  [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
  [SerializeField]
  private bool followOnStart = false;

  [Tooltip("The Smoothing for the camera to follow the target")]
  [SerializeField]
  private float smoothSpeed = 0.125f;

  private float shakeFactor, shakeDecayFactor;

  // cached transform of the target
  Transform cameraTransform;

  // maintain a flag internally to reconnect if target is lost or camera is switched
  bool isFollowing;

  // Cache for camera offset
  Vector3 cameraOffset = Vector3.zero;


  #endregion

  #region MonoBehaviour Callbacks

  /// <summary>
  /// MonoBehaviour method called on GameObject by Unity during initialization phase
  /// </summary>
  void Start()
  {
    // Start following the target if wanted.
    if (followOnStart)
    {
      OnStartFollowing();
    }

    initialHeight = height;
    initialDirection = transform.rotation;
  }

  void Update() {
    if (!photonView.IsMine) return;
    if (shakeFactor >= 0.001f) {
        cameraTransform.position += (Vector3)(Random.insideUnitSphere * shakeFactor);
    }
    shakeFactor *= shakeDecayFactor;
  }


  void LateUpdate() {
    if (!photonView.IsMine) return;
    
    // The transform target may not destroy on level load, 
    // so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
    if (cameraTransform == null && isFollowing) {
      OnStartFollowing();
    }

    // only follow is explicitly declared
    if (isFollowing) {
      Follow();
    }

    if (Input.GetButton("Fire2")) {
      Cursor.lockState = CursorLockMode.Confined;
      Rotate();
    } else {
      ResetVerticalRotation();
      Cursor.lockState = CursorLockMode.None;
    }
  }

  #endregion

  #region Public Methods

  /// <summary>
  /// Raises the start following event. 
  /// Use this when you don't know at the time of editing what to follow, typically instances managed by the photon network.
  /// </summary>
  public void OnStartFollowing()
  {
    cameraTransform = Camera.main.transform;
    isFollowing = true;
    // we don't smooth anything, we go straight to the right camera shot
    Cut();
  }

  #endregion

  #region Private Methods

  /// <summary>
  /// Follow the target smoothly
  /// </summary>
  void Follow()
  {
    cameraOffset.x = offCenter;
    cameraOffset.z = -distance;
    cameraOffset.y = height;

    cameraTransform.position = Vector3.Lerp(cameraTransform.position, this.transform.position + this.transform.TransformVector(cameraOffset), smoothSpeed * Time.deltaTime);
  }

  void Rotate()
  {
    if (cameraTransform == null) cameraTransform = Camera.main.transform;

    float x = Input.GetAxis("Mouse X");
    float y = Input.GetAxis("Mouse Y");
    height = Mathf.Clamp(height - (y * 5f * Time.deltaTime), initialHeight, 20f);

    cameraTransform.RotateAround(transform.position, Vector3.up, x * 80f * Time.deltaTime);
    cameraTransform.RotateAround(transform.position, transform.right, -y * 20f * Time.deltaTime);
    
    //cameraTransform.Rotate(new Vector3(-y, x, 0));

    // Lock Z rotation
    float z = cameraTransform.rotation.eulerAngles.z;
    cameraTransform.Rotate(0, 0, -z);
  }

  void ResetVerticalRotation() {
    height = Mathf.Lerp(height, initialHeight, 0.65f * Time.deltaTime);
    cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, transform.rotation, 0.65f * Time.deltaTime);
  }


  void Cut()
  {
    cameraOffset.z = -distance;
    cameraOffset.y = height;

    cameraTransform.position = this.transform.position + this.transform.TransformVector(cameraOffset);

    cameraTransform.LookAt(this.transform.position + centerOffset);

  }

  public void Shake(float factor, float decay) {
    shakeFactor += factor;
    shakeDecayFactor = decay;
  }
  #endregion
}