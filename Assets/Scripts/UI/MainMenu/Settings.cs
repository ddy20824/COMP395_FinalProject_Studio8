using UnityEngine;
using UnityEngine.UI;

public class Settings : BasePanel<Settings>
{
    [SerializeField] private Toggle musicOn;
    [SerializeField] private Button btnBack;

    protected override void Awake()
    {
        base.Awake();
        HideMe();

    }
    void Start()
    {
        btnBack.onClick.AddListener(() => {
            HideMe();
            MainMenu.Instance.ShowMe();
        });
    }
}
