using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARCursor : MonoBehaviour
{
    public GameObject cursorChildObject;
    public GameObject objectToPlace;
    public GameObject spawnedObject;
    public ARRaycastManager raycastManager;

    public bool useCursor = false;
    public bool ready;

    void Start()
    {
        cursorChildObject.SetActive(useCursor);
    }

    void Update()
    {
        if (ready)
        {
            if (useCursor)
            {
                UpdateCursor();
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (useCursor)
                {
                    if (spawnedObject)
                    {
                        spawnedObject.transform.position = transform.position;
                        spawnedObject.transform.rotation = transform.rotation;
                    }
                    else
                    {
                        spawnedObject = Instantiate(objectToPlace, transform.position, transform.rotation);
                        spawnedObject.SetActive(true);
                    }
                }
                else
                {
                    List<ARRaycastHit> hits = new List<ARRaycastHit>();
                    raycastManager.Raycast(Input.GetTouch(0).position, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);
                    if (hits.Count > 0)
                    {
                        if (spawnedObject)
                        {
                            spawnedObject.transform.position = hits[0].pose.position;
                            spawnedObject.transform.rotation = hits[0].pose.rotation;
                        }
                        else
                        {
                            spawnedObject = Instantiate(objectToPlace, hits[0].pose.position, hits[0].pose.rotation);
                            spawnedObject.SetActive(true);
                        }
                    }
                }
            }
        }

        void UpdateCursor()
        {
            Vector2 screenPosition = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            raycastManager.Raycast(screenPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

            if (hits.Count > 0)
            {
                transform.position = hits[0].pose.position;
                transform.rotation = hits[0].pose.rotation;
            }
        }
    }
}