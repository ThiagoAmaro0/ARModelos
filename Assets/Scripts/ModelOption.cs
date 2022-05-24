using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModelOption : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    public ModelAsset data;
    private Sprite _icon;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(Select);
    }

    private async void Select()
    {
       bool success = await DownloadManager.instance.DownloadModel(data);
    }

    public void SetIcon(Sprite _sprite)
    {
        _icon = _sprite;
        icon.sprite = _icon;
    }
}
