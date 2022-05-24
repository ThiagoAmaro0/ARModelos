using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class TapToPlace : MonoBehaviour
{
    [SerializeField] ARSession m_Session;
    [SerializeField] Material standardMat;
    [SerializeField] private GameObject objectParent;
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private Slider rotationSlider;
    private GameObject spawndedObject;
    private Vector2 touchPosition;
    private ARRaycastManager arRaycastManager;
    public TMP_Text text;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();


    private void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    IEnumerator Start()
    {
        if ((ARSession.state == ARSessionState.None) || (ARSession.state == ARSessionState.CheckingAvailability))
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            // Start some fallback experience for unsupported devices
            text.gameObject.transform.parent.gameObject.SetActive(true);
            text.text = "AR Core not Supported!";
        }
        else
        {
            // Start the AR session
            //text.gameObject.transform.parent.gameObject.SetActive(true);
            //text.text = "AR Core Supported!";
            m_Session.enabled = true;

        }
        //GameObject prefab = objLoader.Load(DownloadManager.instance.selectedStream);
        //spawndedObject = Instantiate(prefab, new Vector3(), Quaternion.identity);
        //Destroy(prefab);
        //spawndedObject.transform.parent = objectParent.transform;
        //spawndedObject.transform.localPosition = new Vector3();
        //foreach (Renderer renderer in spawndedObject.GetComponentsInChildren<Renderer>())
        //{
        //    renderer.material.mainTexture = DownloadManager.instance.selectedTexture;
        //}
        //InstantiateObj();

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("ARScene"));

    }

    public void UpdateRotation()
    {
        objectParent.transform.rotation = Quaternion.Euler(0, 360 * rotationSlider.value, 0);
    }

    public void UpdateScale()
    {
        objectParent.transform.localScale = new Vector3(1, 1, 1) * scaleSlider.value;
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;

    }

    // Update is called once per frame
    void Update()
    {       
        if (!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitpos = hits[0].pose;

            if (spawndedObject == null)
            {
                InstantiateObj();
            }
            objectParent.transform.position = hitpos.position;
            objectParent.transform.rotation = hitpos.rotation;
        }
    }

    private void InstantiateObj()
    {
        while (objectParent.transform.childCount > 0)
        {
            Destroy(objectParent.transform.GetChild(0).gameObject);
        }

        spawndedObject = ObjReader.use.ConvertFile(DownloadManager.instance.selectedModelURL, false, standardMat)[0];
        spawndedObject.GetComponent<Renderer>().material.mainTexture = DownloadManager.instance.selectedTexture;
        spawndedObject.transform.parent = objectParent.transform;
        spawndedObject.transform.localPosition = Vector3.zero;
    }

    public void Back()
    {
        DownloadManager.instance.root.SetActive(true);
        SceneManager.UnloadSceneAsync("ARScene");
    }

}
