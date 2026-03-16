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
        //button onclick: back to main
        btnBack.onClick.AddListener(() => {
            this.HideMe();
            MainMenu.Instance.ShowMe();
        });

        // TODO: SFX - Maybe a checkbox boolean - MKP:2026/03/15
    }
}
