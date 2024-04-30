using System.Text;
using UnityEngine;

public class DataParser : MonoBehaviour
{
    // byte 배열을 string으로 변환
    public static T ReadJsonData<T>(byte[] buf)
    {
        var strByte = Encoding.Default.GetString(buf);
        return JsonUtility.FromJson<T>(strByte);
    }

    // string을 byte 배열로 변환
    public static byte[] DataToJsonData<T>(T obj)
    {
        var jsonData = JsonUtility.ToJson(obj);
        return Encoding.UTF8.GetBytes(jsonData);
    }
}
