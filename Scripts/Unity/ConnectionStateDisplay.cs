using static CharismaSDK.Playthrough;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionStateDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _currentConnectionState;

    public void SetResultState(PlaythroughConnectionState result)
    {
        switch (result)
        {
            case PlaythroughConnectionState.NotConnected:
                SetDisplayText("Disconnected");
                SetDisplayColor(Color.white);
                break;
            case PlaythroughConnectionState.Connecting:
                SetDisplayText("Connecting...");
                SetDisplayColor(Color.yellow);
                break;
            case PlaythroughConnectionState.Connected:
                SetDisplayText("Connected");
                SetDisplayColor(Color.green);
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
