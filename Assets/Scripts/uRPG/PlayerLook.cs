using UnityEngine;
using Photon.Pun;

public class PlayerLook : MonoBehaviourPun
{
    [Header("Components")]
#pragma warning disable CS0109 // member does not hide accessible member
    new Camera camera;
#pragma warning restore CS0109 // member does not hide accessible member

    [Header("Camera")]
    public float XSensitivity = 2;
    public float YSensitivity = 2;
    public float MinimumX = -90;
    public float MaximumX = 90;

    // head position is useful for raycasting etc.
    public Transform firstPersonParent;
    public Transform thirdPersonParent;
    public Vector3 headPosition => thirdPersonParent.position;
    public CharacterController playerCharacterController;

    Vector3 originalCameraPosition, oldPosition;

    // the layer mask to use when trying to detect view blocking
    // (this way we dont zoom in all the way when standing in another entity)
    // (-> create a entity layer for them if needed)
    public LayerMask viewBlockingLayers;
    public float zoomSpeed = 0.5f;
    public float distance = 4;
    public float minDistance = 0;
    public float maxDistance = 7;

    [Header("Physical Interaction")]
    [Tooltip("Layers to use for raycasting. Check Default, Walls, Player, Zombie, Doors, Interactables, Item, etc. Uncheck IgnoreRaycast, AggroArea, Water, UI, etc.")]
    public LayerMask raycastLayers = Physics.DefaultRaycastLayers;

    // camera offsets. Vector2 because we only want X (left/right) and Y (up/down)
    // to be modified. Z (forward/backward) should NEVER be modified because
    // then we could look through walls when tilting our head forward to look
    // downwards, etc. This can be avoided in the camera positioning logic, but
    // is way to complex and not worth it at all.
    [Header("Offsets - Standing")]
    public Vector3 firstPersonOffsetStanding = Vector3.zero;
    public Vector3 thirdPersonOffsetStanding = Vector3.up;
    public Vector3 thirdPersonOffsetStandingMultiplier = Vector3.zero;


    private bool isFirstPerson = false;

    // look directions /////////////////////////////////////////////////////////
    // * for first person, all we need is the camera.forward
    //
    // * for third person, we need to raycast where the camera looks and then
    //   calculate the direction from the eyes.
    //   BUT for animations we actually only want camera.forward because it
    //   looks strange if we stand right in front of a wall, camera aiming above
    //   a player's head (because of head offset) and then the players arms
    //   aiming at that point above his head (on the wall) too.
    //     => he should always appear to aim into the far direction
    //     => he should always fire at the raycasted point
    //   in other words, if we want 1st and 3rd person WITH camera offsets, then
    //   we need both the FAR direction and the RAYCASTED direction
    //
    // * we also need to sync it over the network to animate other players.
    //   => we compress it as far as possible to save bandwidth. syncing it via
    //      rotation bytes X and Y uses 2 instead of 12 bytes per observer(!)
    //
    // * and we can't only calculate and store the values in Update because
    //   ShoulderLookAt needs them live in LateUpdate, Update is too far behind
    //   and would cause the arms to be lag behind a bit.
    //
    public Vector3 lookDirectionFar
    {
        get
        {
            return camera.transform.forward;
        }
    }

    //[SyncVar, HideInInspector] Vector3 syncedLookDirectionRaycasted; not needed atm, see lookPositionRaycasted comment
    public Vector3 lookDirectionRaycasted
    {
        get
        {
            // same for local and other players
            // (positionRaycasted uses camera || syncedDirectionRaycasted anyway)
            return (lookPositionRaycasted - headPosition).normalized;
        }
    }

    // the far position, directionFar projected into nirvana
    public Vector3 lookPositionFar
    {
        get
        {
            Vector3 position = camera.transform.position;
            return position + lookDirectionFar * 9999f;
        }
    }

    // the raycasted position is needed for lookDirectionRaycasted calculation
    // and for firing, so we might as well reuse it here
    public Vector3 lookPositionRaycasted
    {
        get
        {
            // raycast based on position and direction, project into nirvana if nothing hit
            // (not * infinity because might overflow depending on position)
            RaycastHit hit;
            return Utils.RaycastWithout(camera.transform.position, camera.transform.forward, out hit, Mathf.Infinity, gameObject, raycastLayers)
                   ? hit.point
                   : lookPositionFar;
       }
    }

    void Awake()
    {
        if (!photonView.IsMine) return;
        camera = Camera.main;
    }

    void Start()
    {
        if (!photonView.IsMine) return;
        // set camera parent to player
        camera.transform.SetParent(transform, false);

        // look into player forward direction, which was loaded from the db
        camera.transform.rotation = transform.rotation;

        // set camera to head position
        camera.transform.position = headPosition;

        // remember original camera position
        originalCameraPosition = camera.transform.localPosition;
    }

    ////////////////////////////////////////////////////////////////////////////
    void Update() {
        if (!photonView.IsMine || Input.GetButton("Fire2") || Cursor.lockState != CursorLockMode.Locked) return;
        // calculate horizontal and vertical rotation steps
        float xExtra = Input.GetAxis("Mouse X") * XSensitivity;
        float yExtra = Input.GetAxis("Mouse Y") * YSensitivity;

        // use mouse to rotate character
        // set to player parent already?
        if (camera.transform.parent != transform)
            InitializeForcedLook();

        // rotate character for horizontal, rotate camera for vertical
        transform.Rotate(new Vector3(0, xExtra, 0));
        camera.transform.Rotate(new Vector3(-yExtra, 0, 0));

        float forwardVelocity = (Vector3.Project(playerCharacterController.velocity, transform.forward)).magnitude;
        if (forwardVelocity > 15f) {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 75f, (forwardVelocity / 15f) * Time.deltaTime);
        } else if (forwardVelocity > 7f) {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 70f, (forwardVelocity / 7f) * Time.deltaTime);
        } else {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 65f, 7f * Time.deltaTime);
        }
    }

    // Update camera position after everything else was updated
    void LateUpdate() {
        if (!photonView.IsMine || Input.GetButton("Fire2") || Cursor.lockState != CursorLockMode.Locked) return;
        // clamp camera rotation automatically. this way we can rotate it to
        // whatever we like in Update, and LateUpdate will correct it.
        camera.transform.localRotation = Utils.ClampRotationAroundXAxis(camera.transform.localRotation, MinimumX, MaximumX);

        // Zoom changing
        float step = Utils.GetZoomUniversal() * zoomSpeed;
        distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
        if (distance == 0f && !isFirstPerson) {
            // set camera to head position
            camera.transform.position = firstPersonParent.position;
            // remember original camera position
            originalCameraPosition = camera.transform.localPosition;
            isFirstPerson = true;
        } else if (distance > 0f && isFirstPerson) {
            // set camera to head position
            camera.transform.position = thirdPersonParent.position;
            // remember original camera position
            originalCameraPosition = camera.transform.localPosition;
            isFirstPerson = false;
        }

        // calculate target and zoomed position
        Vector3 origin = isFirstPerson ? transform.InverseTransformPoint(headPosition) : originalCameraPosition;
        Vector3 offsetBase = isFirstPerson ? firstPersonOffsetStanding : thirdPersonOffsetStanding;
        Vector3 offsetMult = isFirstPerson ? Vector3.zero : thirdPersonOffsetStanding;

        origin = originalCameraPosition;

        Vector3 target = transform.TransformPoint(origin + offsetBase + offsetMult * distance);
        Vector3 newPosition = target - (camera.transform.rotation * Vector3.forward * distance);

        // avoid view blocking (only third person, pointless in first person)
        // -> always based on original distance and only overwrite if necessary
        //    so that we dont have to zoom out again after view block disappears
        // -> we cast exactly from cam to target, which is the crosshair position.
        //    if anything is inbetween then view blocking changes the distance.
        //    this works perfectly.
        float finalDistance = distance;
        RaycastHit hit;
        Debug.DrawLine(target, camera.transform.position, Color.white);
        if (Physics.Linecast(target, newPosition, out hit, viewBlockingLayers))
        {
            // calculate a better distance (with some space between it)
            finalDistance = Vector3.Distance(target, hit.point) - 0.1f;
            Debug.DrawLine(target, hit.point, Color.red);
        }
        else Debug.DrawLine(target, newPosition, Color.green);

        // set final position
        camera.transform.position = target - (camera.transform.rotation * Vector3.forward * finalDistance);
        
    }

    public void InitializeForcedLook()
    {
        if (!photonView.IsMine) return;
        camera.transform.SetParent(transform, false);
    }

    // debugging ///////////////////////////////////////////////////////////////
    void OnDrawGizmos()
    {
        if (camera == null) return;

        // draw camera forward
        Gizmos.color = Color.white;
        Gizmos.DrawLine(headPosition, camera.transform.position + camera.transform.forward * 9999f);

        // draw all the different look positions
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(headPosition, lookPositionFar);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(headPosition, lookPositionRaycasted);
    }
}
