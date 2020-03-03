using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject Tooltip_Panel;
    public Text Tooltip_Text;

    public float Tooltip_Wait_Time { get; set; }//s
    public float Tooltip_Sensitivity { get; set; }

    private Dictionary<GameObject, Tooltip> tooltips;
    private Vector3 last_mouse_position;
    private float mouse_unmoved_time;
    private float line_height;
    private float font_widht;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
        Tooltip_Wait_Time = 1.0f;
        Tooltip_Sensitivity = 1.0f;
        mouse_unmoved_time = 0.0f;
        line_height = 20.0f;
        font_widht = 7.5f;
        tooltips = new Dictionary<GameObject, Tooltip>();
        Tooltip_Panel.SetActive(false);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        Vector3 current_mouse_position = Input.mousePosition;

        if (Mathf.Abs(current_mouse_position.x - last_mouse_position.x) <= Tooltip_Sensitivity && Mathf.Abs(current_mouse_position.y - last_mouse_position.y) <= Tooltip_Sensitivity) {
            mouse_unmoved_time += Time.deltaTime;
            if (mouse_unmoved_time >= Tooltip_Wait_Time) {
                GameObject gameObject_under_cursor = null;

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(new PointerEventData(null) { position = current_mouse_position }, results);
                foreach (RaycastResult result in results) {
                    if (tooltips.ContainsKey(result.gameObject)) {
                        gameObject_under_cursor = result.gameObject;
                        break;
                    }
                }

                if (gameObject_under_cursor == null) {
                    Tooltip_Panel.SetActive(false);
                } else {
                    Tooltip_Panel.SetActive(true);

                    string tooltip = tooltips[gameObject_under_cursor].Text;
                    MatchCollection line_matches = Regex.Matches(tooltip, "(.*)" + Environment.NewLine);
                    int lines = line_matches.Count == 0 ? 1 : line_matches.Count;
                    int max_line_lenght = lines == 1 ? tooltip.Length : 0;
                    foreach (Match line_match in line_matches) {
                        if (line_match.Value.Length > max_line_lenght) {
                            max_line_lenght = line_match.Value.Length;
                        }
                    }
                    Tooltip_Text.text = tooltip;
                    Tooltip_Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(5.0f + (max_line_lenght * font_widht), 7.5f + (lines * line_height));
                    Tooltip_Text.GetComponent<RectTransform>().sizeDelta = new Vector2(max_line_lenght * font_widht, lines * line_height);

                    Tooltip_Panel.transform.position = current_mouse_position;
                }
            }
        } else {
            mouse_unmoved_time = 0.0f;
            Tooltip_Panel.SetActive(false);
        }
        last_mouse_position = current_mouse_position;
    }

    public void Register_Tooltip(GameObject gameObject, string tooltip, GameObject owner)
    {
        if (string.IsNullOrEmpty(tooltip.Trim())) {
            return;
        }
        if (tooltips.ContainsKey(gameObject)) {
            tooltips[gameObject] = new Tooltip(tooltip, owner);
        } else {
            tooltips.Add(gameObject, new Tooltip(tooltip, owner));
        }
    }

    public void Unregister_Tooltip(GameObject gameObject)
    {
        if (!tooltips.ContainsKey(gameObject)) {
            return;
        }
        tooltips.Remove(gameObject);
    }

    public void Unregister_Tooltips_By_Owner(GameObject gameObject)
    {
        List<GameObject> owned_tooltips = new List<GameObject>();
        foreach (KeyValuePair<GameObject, Tooltip> pair in tooltips) {
            if (pair.Value.Owner == gameObject) {
                owned_tooltips.Add(pair.Key);
            }
        }
        foreach (GameObject tooltip_gameObject in owned_tooltips) {
            tooltips.Remove(tooltip_gameObject);
        }
    }

    private class Tooltip
    {
        public string Text { get; set; }
        public GameObject Owner { get; set; }

        public Tooltip(string text, GameObject owner)
        {
            Text = text;
            if (Text.Contains(Environment.NewLine) && !Text.EndsWith(Environment.NewLine)) {
                Text += Environment.NewLine;
            }
            Owner = owner;
        }
    }
}
