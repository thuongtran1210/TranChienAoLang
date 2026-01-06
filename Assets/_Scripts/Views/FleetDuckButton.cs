using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FleetDuckButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image duckIcon;
    [SerializeField] private TextMeshProUGUI nameText;

    private DuckDataSO _duckData;
    private Action<DuckDataSO> _onSelectAction;

    public void Setup(DuckDataSO data, Action<DuckDataSO> onSelect)
    {
        _duckData = data;
        _onSelectAction = onSelect;

        // Visual setup từ DuckDataSO
        if (duckIcon != null) duckIcon.sprite = data.icon;

        // Đăng ký sự kiện click
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClicked);
    }
    private void OnButtonClicked()
    {
        // Báo ngược lại cho Manager biết nút này vừa được bấm
        _onSelectAction?.Invoke(_duckData );
    }
}
