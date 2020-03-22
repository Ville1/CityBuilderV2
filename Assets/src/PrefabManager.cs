using UnityEngine;

public class PrefabManager : MonoBehaviour {
    public static PrefabManager Instance { get; private set; }

    public GameObject Tile;
    //TODO: Buildings have Text component
    public GameObject Building_1x1;
    public GameObject Building_2x2;
    public GameObject Building_3x3;
    public GameObject Building_Generic;
    public GameObject Road;
    public GameObject Alert;
    public GameObject Entity;

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

    }
}
