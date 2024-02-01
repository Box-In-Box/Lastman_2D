using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseCursor : MonoBehaviour
{
    public Texture2D cursorIcon;
    public Texture2D handCursorIcon;
    public Texture2D lobbyCursorIcon;
    public Texture2D gameCursorIcon;
    
    void Start()
    {
        
        handCursorIcon = Resources.Load<Texture2D>("handCursorIcon");
        lobbyCursorIcon = Resources.Load<Texture2D>("lobbyCursorIcon");
        gameCursorIcon = Resources.Load<Texture2D>("gameCursorIcon");

        if (SceneManager.GetActiveScene().name == "Lobby")
            cursorIcon = lobbyCursorIcon;
        else
            cursorIcon = gameCursorIcon;

        Cursor.SetCursor(cursorIcon, new Vector2(0, 0), CursorMode.Auto);
    }

    public void OnMouseOver()
    {
        Cursor.SetCursor(handCursorIcon, new Vector2(handCursorIcon.width / 3, 0), CursorMode.Auto);
    }
    
    public void OnMouseExit()
    {
        Cursor.SetCursor(cursorIcon, new Vector2(0, 0), CursorMode.Auto);
    }

    public void ManualOnMouseOver()
    {
        Cursor.SetCursor(gameCursorIcon, new Vector2(gameCursorIcon.width / 3, 0), CursorMode.Auto);
    }
    
    public void ManualOnMouseExit()
    {
        Cursor.SetCursor(cursorIcon, new Vector2(0, 0), CursorMode.Auto);
    }
}
