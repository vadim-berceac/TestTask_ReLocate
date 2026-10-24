using UnityEngine;

public class HintText : MonoBehaviour
{
    [TextArea(3, 5)]
    [SerializeField] private string text = "Подсказка";
    [SerializeField] private Vector2 position = new (20f, 20f);
    [SerializeField] private int fontSize = 20;
    [SerializeField] private Color color = Color.white;

    private void OnGUI()
    {
        if (!enabled) return;

        var previousColor = GUI.color;
        var previousAlignment = GUI.skin.label.alignment;
        var previousFontSize = GUI.skin.label.fontSize;

        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUI.skin.label.fontSize = fontSize;

        var rect = new Rect(position.x, position.y, Screen.width - position.x, Screen.height - position.y);
        
        GUI.color = Color.black;
        GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text);

        GUI.color = color;
        GUI.Label(rect, text);

        GUI.skin.label.fontSize = previousFontSize;
        GUI.skin.label.alignment = previousAlignment;
        GUI.color = previousColor;
    }
}