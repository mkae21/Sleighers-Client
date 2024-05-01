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
        // jsonData 맨 뒤에 패킷 간 구분자인 개행문자 추가
        jsonData += "\n";
        return Encoding.UTF8.GetBytes(jsonData);
    }
}
