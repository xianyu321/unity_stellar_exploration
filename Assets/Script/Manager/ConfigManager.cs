using System.IO;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class ConfigManager{
    private static ConfigManager _instance;
    public static ConfigManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ConfigManager();
                _instance.Initialize(); // 初始化
            }
            return _instance;
        }
    }
    void Initialize(){
        // mouseSensitivity
    }

    T  LoadFromJson<T>(string filePath) where T : class, new(){
        string data = FileLoader.LoadJsonFile(filePath);
        if(data == null){
            T temp = new T();
            SaveToJson<T>(temp, filePath);
            return temp;
        }
        T json = JsonUtility.FromJson<T>(data);
        return json;
    }

        // 泛型方法，T是你要序列化并保存的对象类型
    public static void SaveToJson<T>(T data, string filePath) where T : class
    {
        if (data == null)
        {
            Debug.LogError("要保存的数据不能为空");
            return;
        }
        string jsonString = JsonUtility.ToJson(data, true); // 第二个参数为true表示格式化输出
        File.WriteAllText(filePath, jsonString);
    }

    
    static string appConfigDir = Path.Combine(PathLoader.GetAppConfigPath(), "config.json");
    public AppConfig GetAppConfig(){
        return LoadFromJson<AppConfig>(appConfigDir);
    }
    public void SaveAppConfig(AppConfig appConfig){
        SaveToJson<AppConfig>(appConfig, appConfigDir);
        if(appConfig.languageType == "en"){
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
        }else{
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        }
    }
    public SaveConfig GetSaveConfig(string savename){
        string faPath = PathLoader.GetSavePath(savename);
        string filePath = Path.Combine(faPath, "config.json");
        return LoadFromJson<SaveConfig>(filePath);
    }

    public void SaveSaveConfig(SaveConfig config, string savename){
        string faPath = PathLoader.GetSavePath(savename);
        string filePath = Path.Combine(faPath, "config.json");
        SaveToJson<SaveConfig>(config, filePath);
    }
}