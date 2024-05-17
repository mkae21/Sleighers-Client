using UnityEngine;
/* SettingManager.cs
 * - 게임 설정을 관리
 */
public class SettingManager : MonoBehaviour
{
#region PublicVariables
    public static bool backgroundPostProcessing { get; private set; } = true;
    public static bool speedPostProcessing { get; private set; } = true;
#endregion

#region PrivateVariables
    private static float soundValue = 0.5f;
#endregion

#region PublicMethod
    public static void ToggleBackgroundPostProcessing() => backgroundPostProcessing = !backgroundPostProcessing;
    public static void ToggleSpeedPostProcessing() => speedPostProcessing = !speedPostProcessing;
    public static void VolumeToggle(bool _value) => AudioListener.volume = _value ? soundValue : 0;
    public static void VolumeSlide(float _value) 
    {
        soundValue = _value;
        AudioListener.volume = soundValue;
    }
#endregion
}
