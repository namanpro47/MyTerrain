// Pure adapter logic only. These stubs do not establish Unity/JsonUtility compatibility.
using System.Text.Json;
namespace UnityEngine {
 public class MonoBehaviour { public GameObject gameObject = null; }
 public class GameObject { public string name; public GameObject(string value) { name=value; } public T AddComponent<T>() where T : MonoBehaviour,new() => new T {gameObject=this}; }
 public static class Debug { public static void LogException(Exception e, object context) {} }
 public static class Time { public static float realtimeSinceStartup; }
 public static class JsonUtility {
 static readonly JsonSerializerOptions Options = new JsonSerializerOptions {IncludeFields=true};
 public static string ToJson(object value, bool pretty=false) => JsonSerializer.Serialize(value,Options);
 public static T FromJson<T>(string value) => JsonSerializer.Deserialize<T>(value,Options);
 }
 public static class PlayerPrefs {
 public static readonly Dictionary<string,string> Values = new();
 public static bool HasKey(string key) => Values.ContainsKey(key);
 public static string GetString(string key) => Values[key];
 public static void SetString(string key,string value) => Values[key]=value;
 public static void Save() {}
 }
}

namespace UnityEngine.Scripting { public sealed class PreserveAttribute : Attribute {} }
