public class HitStopSwich : AnimationRelay
{
    public delegate void PauseHitStopDelegate();
    public static PauseHitStopDelegate PauseHitStop;    
    public delegate void ResumeHitStopDelegate();
    public static ResumeHitStopDelegate ResumeHitStop;
    /// <summary>
    /// SetActive == trueで呼び出される
    /// </summary>
    void OnEnable()
    {
        PauseHitStop();
    }
    void OnDisable()
    {
        ResumeHitStop();
        gameObject.SetActive(false);
    }
}
