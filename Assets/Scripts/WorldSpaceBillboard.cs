using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceBillboard : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private IntTypeEventChannel respawnItemEvent;
    [SerializeField] private IntTypeEventChannel onRespawnBtnActiveEvent;
    [SerializeField] private Button btnRespawn;

    private Transform mainCameraTransform;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
        btnRespawn.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        btnRespawn.onClick.AddListener(onButtonClick);
        onRespawnBtnActiveEvent.Subscribe(enableButton);
    }

    private void OnDisable()
    {
        onRespawnBtnActiveEvent.Unsubscribe(enableButton);
        btnRespawn.onClick.RemoveListener(onButtonClick);
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                         mainCameraTransform.rotation * Vector3.up);
    }

    void onButtonClick()
    {
        btnRespawn.gameObject.SetActive(false);
        respawnItemEvent.Raise(index);
    }


    void enableButton(int boxIndex)
    {
        if (boxIndex == index)
        {
            btnRespawn.gameObject.SetActive(true);
        }
    }
}
