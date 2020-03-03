using System.Collections.Generic;
using UnityEngine;

public class CameraManager
{
    public enum Zoom { In, Out }

    private static CameraManager instance;

    private Camera camera;
    private float speed;
    private float zoom_speed;
    private float current_zoom;
    private float default_zoom;
    private float min_zoom;
    private float max_zoom;
    private float original_z;
    private GameObject canvas;

    private CameraManager()
    {
        camera = Camera.main;
        speed = 0.25f;
        zoom_speed = 1.0f;
        default_zoom = 0.0f;
        current_zoom = 0.0f;
        min_zoom = -5.0f;
        max_zoom = 50.0f;
        original_z = camera.transform.position.z;
        camera.orthographicSize = default_zoom;
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    /// <returns></returns>
    public static CameraManager Instance
    {
        get {
            if (instance == null) {
                instance = new CameraManager();
            }
            return instance;
        }
    }

    public void Zoom_Camera(Zoom zoom)
    {
        if(zoom == Zoom.In) {
            Set_Zoom(current_zoom + zoom_speed);
        } else {
            Set_Zoom(current_zoom - zoom_speed);
        }
    }

    public void Set_Zoom(float zoom)
    {
        current_zoom = Mathf.Clamp(zoom, min_zoom, max_zoom);
        camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, original_z - current_zoom);
    }

    public void Reset_Zoom()
    {
        CustomLogger.Instance.Warning("Not implemented");
    }

    /// <summary>
    /// Moves the camera
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    public void Move_Camera(Vector2 delta)
    {
        camera.transform.Translate(delta);
    }

    /// <summary>
    /// Moves the camera
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    public void Move_Camera(Coordinates.Direction direction)
    {
        switch (direction) {
            case Coordinates.Direction.North:
                camera.transform.Translate(new Vector3(0.0f, speed, 0.0f));
                break;
            case Coordinates.Direction.North_East:
                camera.transform.Translate(new Vector3(speed, speed, 0.0f));
                break;
            case Coordinates.Direction.East:
                camera.transform.Translate(new Vector3(speed, 0.0f, 0.0f));
                break;
            case Coordinates.Direction.South_East:
                camera.transform.Translate(new Vector3(speed, -speed, 0.0f));
                break;
            case Coordinates.Direction.South:
                camera.transform.Translate(new Vector3(0.0f, -speed, 0.0f));
                break;
            case Coordinates.Direction.South_West:
                camera.transform.Translate(new Vector3(-speed, -speed, 0.0f));
                break;
            case Coordinates.Direction.West:
                camera.transform.Translate(new Vector3(-speed, 0.0f, 0.0f));
                break;
            case Coordinates.Direction.North_West:
                camera.transform.Translate(new Vector3(-speed, speed, 0.0f));
                break;
        }
    }

    /// <summary>
    /// Sets the camera to a new location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public void Set_Camera_Location(Vector2 location)
    {
        camera.transform.position = new Vector3(location.x, location.y, camera.transform.position.z);
    }

    /// <summary>
    /// Returns world coordinate points that make screen
    /// </summary>
    /// <returns></returns>
    public List<Vector2> Get_Screen_Location()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(camera.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f)));
        points.Add(camera.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, 0.0f)));
        points.Add(camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0.0f)));
        points.Add(camera.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, 0.0f)));
        return points;
    }

    public Camera Camera
    {
        get {
            return camera;
        }
        private set {
            camera = value;
        }
    }

    private GameObject Canvas
    {
        get {
            if (canvas != null) {
                return canvas;
            }
            canvas = GameObject.Find("Canvas");
            return canvas;
        }
    }

    /// <summary>
    /// https://answers.unity.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
    /// </summary>
    /// <param name="ui_element"></param>
    /// <param name="world_go"></param>
    public void Set_UI_Element_On_World_GO(RectTransform ui_element_rect_transform, GameObject world_go)
    {
        //first you need the RectTransform component of your canvas
        RectTransform CanvasRect = Canvas.GetComponent<RectTransform>();

        //then you calculate the position of the UI element
        //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
        Vector2 ViewportPosition = CameraManager.Instance.Camera.WorldToViewportPoint(new Vector3(world_go.transform.position.x + 0.5f, world_go.transform.position.y - 0.5f, world_go.transform.position.z));
        Vector2 WorldObject_ScreenPosition = new Vector2(
        ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

        //now you can set the position of the ui element
        ui_element_rect_transform.anchoredPosition = WorldObject_ScreenPosition;
    }
}
