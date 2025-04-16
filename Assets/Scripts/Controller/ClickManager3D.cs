using UnityEngine;

public class ClickManager3D : MonoBehaviour
{
    public UIManager uiManager;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Créer un rayon à partir de la position de la souris
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                // Debug.Log("Clicked on: " + clickedObject.name);
                // Vérifier la présence d'un tag "Point" sur l'objet cliqué
                if (clickedObject.CompareTag("Point"))
                {
                    uiManager.OpenPointMenu(clickedObject);
                }
            }
        }
    }
}
