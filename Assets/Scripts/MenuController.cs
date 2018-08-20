using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject MusicController;
    public Text StarText;
    public Text StarTextShadow;

    void Start()
    {
        GameObject.DontDestroyOnLoad(MusicController);

#if UNITY_ANDROID
        StarText.text = "Touch to start";
        StarTextShadow.text = "Touch to start";
#else
        StarText.text = "Press Enter to start";
        StarTextShadow.text = "Press Enter to start";
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || 
            Input.GetKeyDown(KeyCode.KeypadEnter) || 
            Input.GetKeyDown(KeyCode.Space) ||
            Input.touchCount > 0)
        {
            SceneManager.LoadScene("Game");
        }
    }
}
