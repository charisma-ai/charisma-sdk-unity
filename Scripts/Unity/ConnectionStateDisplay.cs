using static CharismaSDK.Playthrough;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionStateDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _currentConnectionState;

    public void SetResultState(ConnectionState result)
    {
        switch (result)
        {
            case ConnectionState.Disconnected:
                SetDisplayText("Disconnected");
                SetDisplayColor(Color.white);
                break;
            case ConnectionState.Connecting:
                SetDisplayText("Connecting...");
                SetDisplayColor(Color.yellow);
                break;
            case ConnectionState.Connected:
                SetDisplayText("Connected");
                SetDisplayColor(Color.green);
                break;
            case ConnectionState.Reconnecting:
                SetDisplayText("Reconnecting...");
                SetDisplayColor(Color.yellow);
                break;
        }
    }

    private void SetDisplayColor(Color color)
    {
        _currentConnectionState.color = color;
    }

    private void SetDisplayText(string text)
    {
        _currentConnectionState.text = text;
    }
}
