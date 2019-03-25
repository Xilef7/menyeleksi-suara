using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleInputHandler : MonoBehaviour
{
    [SerializeField] Transform canvas = null;

    private int shownChildIndex = 0;

    void Awake()
    {
        shownChildIndex = 0;
        canvas.GetChild(0).gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int newShownChildIndex = shownChildIndex + 1;
            if (newShownChildIndex < canvas.childCount)
            {
                ChangeUI(newShownChildIndex);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            int newShownChildIndex = shownChildIndex - 1;
            if (newShownChildIndex >= 0)
            {
                ChangeUI(newShownChildIndex);
            }
            else
            {
                SceneManager.LoadScene("Menu");
            }
        }
    }

    void ChangeUI(int newShownChildIndex)
    {
        canvas.GetChild(shownChildIndex).gameObject.SetActive(false);
        canvas.GetChild(newShownChildIndex).gameObject.SetActive(true);
        shownChildIndex = newShownChildIndex;
    }
}
