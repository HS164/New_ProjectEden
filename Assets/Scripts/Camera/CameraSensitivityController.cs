using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Cinemachineカメラの感度を外部から制御する。
/// Third Person Aim Camera にアタッチして使用する
/// </summary>
public class CameraSensitivityController : MonoBehaviour
{
    [SerializeField, Range(1, 10)] private int sensitivity = 5;

    private CinemachineInputAxisController axisController;

    private void Awake()
    {
        axisController = GetComponent<CinemachineInputAxisController>();
        ApplySensitivity();
    }

    /// <summary>
    /// オプション画面から感度を設定する（1〜10）
    /// </summary>
    public void SetSensitivity(int value)
    {
        sensitivity = Mathf.Clamp(value, 1, 10);
        ApplySensitivity();
    }

    /// <summary>
    /// sensitivityの値をCinemachineのGainに反映する
    /// </summary>
    private void ApplySensitivity()
    {
        if (axisController == null)
        {
            return;
        }

        foreach (InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller controller in axisController.Controllers)
        {
            controller.Input.Gain = sensitivity;
        }
    }
}
