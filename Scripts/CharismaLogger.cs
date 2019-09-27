using UnityEngine;

public static class CharismaLogger
{
    public static bool IsActive { get; set; }
    
    public static void Log(string message)
    {
        if(IsActive)
            Debug.Log(message);
    }
}
