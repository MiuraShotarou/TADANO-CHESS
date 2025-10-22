using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    void Shot()
    {
        string fileName = $"Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        
        ScreenCapture.CaptureScreenshot(fileName);
        
        // ✅ 保存場所を表示
        Debug.Log($"Screenshot saved: {Application.dataPath}/../{fileName}");
    }
}