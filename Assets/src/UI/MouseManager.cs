using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    private Vector3 last_position;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Warning(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        Vector3 current_position = CameraManager.Instance.Camera.ScreenToWorldPoint(Mouse_Position_Relative_To_Camera);
        if (last_position != null && Input.GetMouseButton(2)) {
            //Move camera
            Vector3 difference = last_position - current_position;
            CameraManager.Instance.Move_Camera(-1.0f * difference);
            //Close stuff
            MasterUIManager.Instance.Close_Others(string.Empty);
        }

        //Buttons
        if (Input.GetMouseButtonDown(0)) {
            if (!EventSystem.current.IsPointerOverGameObject()) {
                Tile t = Tile_Under_Cursor;
                MasterUIManager.Instance.Close_Others(string.Empty);
            }
        }
        //Scrolling
        if (Input.GetAxis("Mouse ScrollWheel") > 0.0f) {
            CameraManager.Instance.Zoom_Camera(CameraManager.Zoom.Out);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0.0f) {
            CameraManager.Instance.Zoom_Camera(CameraManager.Zoom.In);
        }

        last_position = CameraManager.Instance.Camera.ScreenToWorldPoint(Mouse_Position_Relative_To_Camera);
    }

    public Vector3 Mouse_Position_Relative_To_Camera
    {
        get {
            Vector3 position = Input.mousePosition;
            position.z = CameraManager.Instance.Camera.transform.position.z;
            return position;
        }
    }

    public Tile Tile_Under_Cursor
    {
        get {
            if(!Map.Instance.Active) {
                return null;
            }
            RaycastHit hit;
            if (Physics.Raycast(CameraManager.Instance.Camera.ScreenPointToRay(Input.mousePosition), out hit)) {
                Coordinates coordinates = Tile.Parse_Coordinates_From_GameObject_Name(hit.transform.gameObject.name);
                if(coordinates != null) {
                    return Map.Instance.Get_Tile_At(coordinates);
                } else {
                    return null;
                }
            }
            return null;
        }
    }
}
