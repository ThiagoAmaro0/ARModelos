using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class DownloadManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Transform buttonsContent;
    [SerializeField] public GameObject root;

    private ModelData modelAssets;
    private FirebaseStorage storage;
    private FirebaseDatabase _dbReference;

    public string selectedModelURL;
    public Texture2D selectedTexture;
    public ModelAsset selectedModelAsset;

    public static DownloadManager instance;
    private int downloads;

    void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            storage = FirebaseStorage.DefaultInstance;
            _dbReference = FirebaseDatabase.GetInstance("https://meta4chain-ar-default-rtdb.firebaseio.com/");
            GetData();
        }
        else
        {
            //Duplicate GameManager created every time the scene is loaded
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath);
        foreach (string filePath in filePaths)
            File.Delete(filePath);

    }

    private void GetData()
    {
        loadingPanel.SetActive(true);
        _dbReference.GetReference("ModelAssets").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.LogError(task.Exception.Message);
                loadingPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Falha ao carregar. \nTente novamente mais tarde.";
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                modelAssets = JsonUtility.FromJson<ModelData>(snapshot.GetRawJsonValue());
                print(snapshot.GetRawJsonValue());
                SetupButtons();
            }
        });
    }

    private async void SetupButtons()
    {
        List<ModelAsset> oldList = modelAssets.modelAssets;
        modelAssets.modelAssets = new List<ModelAsset>();

        foreach (ModelAsset model in oldList)
        {
            if (model.name != null || model.name != "")
            {
                modelAssets.modelAssets.Add(model);
            }
        }

        foreach (ModelAsset model in modelAssets.modelAssets)
        {
            
                GameObject modelGO = Instantiate(buttonPrefab, buttonsContent);
                modelGO.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = model.name;
                modelGO.transform.Find("DescText").GetComponent<TextMeshProUGUI>().text = model.description;
                ModelOption modelOption = modelGO.GetComponent<ModelOption>();
                modelOption.data = model;
                string iconURL = await GetURL($"Models/{model.name}/{model.name}_Icon.jpeg");
                DownloadIcon(iconURL, modelOption);

            
        }
    }

    private async Task<string> GetURL(string path)
    {
        StorageReference pathReference = storage.GetReference(path);


        Task<Uri> task = pathReference.GetDownloadUrlAsync();
        await task;

        return task.Result.ToString();
    }

    private void DownloadModel(string path)
    {
        StorageReference pathReference = storage.GetReference(path);

        pathReference.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Download URL: " + task.Result);
                StartCoroutine(DownloadOBJ(task.Result.ToString()));
            }
        });
    }

    private void DownloadIcon(string MediaUrl, ModelOption model)
    {
        StartCoroutine(ImageRequest(MediaUrl, (UnityWebRequest req) =>
        {
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            else
            {
                // Get the texture out using a helper downloadhandler
                Texture2D texture = DownloadHandlerTexture.GetContent(req);
                model.SetIcon(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
                downloads++;

                loadingPanel.SetActive(downloads != modelAssets.modelAssets.Count);
            }
        }));
    }

    public async Task<bool> DownloadModel(ModelAsset model)
    {
        selectedModelAsset = model;
        loadingPanel.SetActive(true);
        print("Start Texture Download");
        string texURL = await GetURL($"Models/{model.name}/{model.name}_Texture.jpeg");
        StartCoroutine(DownloadTexture(texURL));
        print("Done Download");



        string path = Path.Combine(Application.persistentDataPath, selectedModelAsset.name + ".obj");
        selectedModelURL = path;
        if (!File.Exists(path))
        {
            DownloadModel($"Models/{model.name}/{model.name}_Model.obj");
        }
        else
        {
            print("Done");
            loadingPanel.SetActive(false);
            root.SetActive(false);
            SceneManager.LoadSceneAsync("ARScene", LoadSceneMode.Additive);
        }



        return true;
    }

    private IEnumerator DownloadTexture(string MediaUrl)
    {
        yield return StartCoroutine(ImageRequest(MediaUrl, (UnityWebRequest req) =>
        {
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            else
            {
                // Get the texture out using a helper downloadhandler
                selectedTexture = DownloadHandlerTexture.GetContent(req);
            }
        }));
    }

    private IEnumerator DownloadOBJ(string MediaUrl)
    {
        print("Start Model Download");
        var www = new WWW(MediaUrl);
        yield return www;
        byte[] bytes = www.bytes;

        string path = Path.Combine(Application.persistentDataPath, selectedModelAsset.name+".obj");
        //File.Create(path);
        File.WriteAllBytes(path, bytes);

        print("Done");
        loadingPanel.SetActive(false);
        root.SetActive(false);
        SceneManager.LoadSceneAsync("ARScene", LoadSceneMode.Additive);

    }

    IEnumerator ImageRequest(string url, Action<UnityWebRequest> callback)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();
            callback(req);
        }
    }
}
