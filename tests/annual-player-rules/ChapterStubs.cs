// Pure adapter logic only. These stubs do not establish Unity/JsonUtility compatibility.
using System.Text.Json;
namespace UnityEngine {
 public class MonoBehaviour { public GameObject gameObject = null; }
 public class GameObject { public string name; public GameObject(string value) { name=value; } public T AddComponent<T>() where T : MonoBehaviour,new() => new T {gameObject=this}; }
 public static class Debug { public static void Log(object value) {} public static void LogWarning(object value) {} public static void LogException(Exception e, object context) {} }
 public static class Time { public static float realtimeSinceStartup; public static double realtimeSinceStartupAsDouble; }
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

namespace UnityEngine.Networking {
 public class UploadHandlerRaw { public byte[] data; public UploadHandlerRaw(byte[] value){data=value;} }
 public class DownloadHandlerBuffer { public string text; }
 public class UnityWebRequestAsyncOperation { public bool isDone; }
 public class UnityWebRequest : IDisposable {
  public enum Result { InProgress,Success,ConnectionError,ProtocolError }
  public static UnityWebRequest Last; public static Action OnSend;
  public UploadHandlerRaw uploadHandler; public DownloadHandlerBuffer downloadHandler; public int timeout; public long responseCode; public Result result;
  public UnityWebRequestAsyncOperation operation=new();
  public UnityWebRequest(string url,string method){Last=this;}
  public void SetRequestHeader(string name,string value){}
  public UnityWebRequestAsyncOperation SendWebRequest(){OnSend?.Invoke();return operation;}
  public void Abort(){} public void Dispose(){}
 }
}
