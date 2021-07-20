using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class ClickCatcher : MonoBehaviour
{
    bool mousePressed = false;
    RaycastHit2D hit;
    void Update()
    {
        if (GameManager.gameState == GameManager.GameState.play)
        {
            if (Input.GetMouseButtonDown(0) && !mousePressed)
            {
                mousePressed = true;
                if (HittingUI())
                {
                    return;
                }
                hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.tag == "Node")
                {
                    hit.collider.GetComponent<Node>().OnClick();
                }
                else
                {
                    UIManager.Instance.CloseActionPanel();
                }
            }
            if (!Input.GetMouseButtonDown(0) && mousePressed)
            {
                mousePressed = false;
            }
        }
    }

    bool HittingUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        if (results.Count > 0)
        {
            foreach (var go in results)
            {
                if (go.gameObject.layer == LayerMask.NameToLayer("UI"))
                    return true;
            }
        }
        return false;
    }
}
