using UnityEngine;

// Ассет с настройками управления. Создаётся через
// Assets -> Create -> Settings -> Input Settings
[CreateAssetMenu(fileName = "InputSettings", menuName = "Settings/Input Settings")]
public class InputSettings : ScriptableObject
{
    [Header("Движение")]
    public KeyCode moveForward = KeyCode.W;
    public KeyCode moveBackward = KeyCode.S;
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;

    [Header("Действия")]
    public KeyCode jump = KeyCode.Space;
    public KeyCode sprint = KeyCode.LeftShift;
    public KeyCode respawn = KeyCode.R;

    [Header("Режим управления")]
    public KeyCode toggleMovementMode = KeyCode.Tab;

    [Header("Бой")]
    public KeyCode attack = KeyCode.Mouse0;
    public KeyCode block = KeyCode.Mouse1;

    [Header("Камера / мышь")]
    [Range(0.1f, 10f)] public float mouseSensitivityX = 3f;
    [Range(0.1f, 10f)] public float mouseSensitivityY = 3f;
    public bool invertY = false;

    private const string PrefsPrefix = "KeyBind_";
    private const string FloatPrefsPrefix = "Setting_";

    private void OnEnable()
    {
        LoadBindings();
    }

    public void LoadBindings()
    {
        moveForward = LoadKey(nameof(moveForward), moveForward);
        moveBackward = LoadKey(nameof(moveBackward), moveBackward);
        moveLeft = LoadKey(nameof(moveLeft), moveLeft);
        moveRight = LoadKey(nameof(moveRight), moveRight);
        jump = LoadKey(nameof(jump), jump);
        sprint = LoadKey(nameof(sprint), sprint);
        respawn = LoadKey(nameof(respawn), respawn);
        toggleMovementMode = LoadKey(nameof(toggleMovementMode), toggleMovementMode);
        attack = LoadKey(nameof(attack), attack);
        block = LoadKey(nameof(block), block);

        mouseSensitivityX = LoadFloat(nameof(mouseSensitivityX), mouseSensitivityX);
        mouseSensitivityY = LoadFloat(nameof(mouseSensitivityY), mouseSensitivityY);
        invertY = LoadFloat(nameof(invertY), invertY ? 1f : 0f) > 0.5f;
    }

    public void SetKey(string bindingName, KeyCode newKey)
    {
        PlayerPrefs.SetInt(PrefsPrefix + bindingName, (int)newKey);
        PlayerPrefs.Save();
        LoadBindings();
    }

    public void SetSensitivity(float x, float y)
    {
        PlayerPrefs.SetFloat(FloatPrefsPrefix + nameof(mouseSensitivityX), x);
        PlayerPrefs.SetFloat(FloatPrefsPrefix + nameof(mouseSensitivityY), y);
        PlayerPrefs.Save();
        LoadBindings();
    }

    public void SetInvertY(bool value)
    {
        PlayerPrefs.SetFloat(FloatPrefsPrefix + nameof(invertY), value ? 1f : 0f);
        PlayerPrefs.Save();
        LoadBindings();
    }

    private KeyCode LoadKey(string bindingName, KeyCode defaultKey)
    {
        int saved = PlayerPrefs.GetInt(PrefsPrefix + bindingName, (int)defaultKey);
        return (KeyCode)saved;
    }

    private float LoadFloat(string settingName, float defaultValue)
    {
        return PlayerPrefs.GetFloat(FloatPrefsPrefix + settingName, defaultValue);
    }
}