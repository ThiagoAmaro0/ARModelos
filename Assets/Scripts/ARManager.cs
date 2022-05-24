
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ARManager : MonoBehaviour
{
    [SerializeField] private ARCursor _arCursor;
    [SerializeField] private GameObject _loadingPanel;
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("NewARScene"));
        _loadingPanel.SetActive(true);
        LoadObject();
    }

    private void LoadObject()
    {
        //_arCursor.objectToPlace = _objLoader.Load(DownloadManager.instance.selectedModelURL);
        ObjectLoader loader = _arCursor.objectToPlace.AddComponent<ObjectLoader>();
        loader.Load(Application.persistentDataPath+"/", DownloadManager.instance.selectedModelAsset.name+".obj");
        foreach (Renderer renderer in _arCursor.objectToPlace.GetComponentsInChildren<Renderer>())
        {
            renderer.material.mainTexture = DownloadManager.instance.selectedTexture;
        }
        _arCursor.objectToPlace.SetActive(false);
        _loadingPanel.SetActive(false);
        _arCursor.ready = true;
    }
}
