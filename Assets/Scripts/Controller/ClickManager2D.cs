using UnityEngine;

public class ClickManager2D : MonoBehaviour
{
    public UIManager uiManager;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject clickedObject = hit.collider.gameObject;
                Debug.Log("Clicked on point: " + clickedObject);
                if (clickedObject.transform.tag == "Point")
                {
                    uiManager.OpenPointMenu(clickedObject);
                }
            }
        }
    }
}
