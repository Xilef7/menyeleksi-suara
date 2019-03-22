using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class NotificationUI : MonoBehaviour
{
    [SerializeField] private Sprite hitSprite = null;
    [SerializeField] private Sprite commissionSprite = null;
    [SerializeField] private float duration = 1;

    private Image image;
    private GameManager gameManager;

    void Awake()
    {
        image = GetComponent<Image>();

        image.enabled = false;
    }

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.Hit += OnHit;
        gameManager.Commission += OnCommission;
        gameManager.Omission += OnOmission;
    }

    void OnDestroy()
    {
        gameManager.Hit -= OnHit;
        gameManager.Commission -= OnCommission;
        gameManager.Omission -= OnOmission;
    }

    void OnHit(ResponseInfo responseInfo)
    {
        CancelInvoke("Hide");
        image.sprite = responseInfo.HasRespondedBefore ? commissionSprite : hitSprite;
        Show();
    }

    void OnCommission(ResponseInfo responseInfo)
    {
        CancelInvoke("Hide");
        image.sprite = commissionSprite;
        Show();
    }

    void OnOmission(int currentLevel, int lastStimulusIndex)
    {
        CancelInvoke("Hide");
        image.sprite = commissionSprite;
        Show();
    }

    void Show()
    {
        image.enabled = true;
        Invoke("Hide", duration);
    }

    void Hide()
    {
        image.enabled = false;
    }
}
