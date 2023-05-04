using CharismaSDK;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionStateDisplay : MonoBehaviour
{
    private enum ConnectionStateResult
    {
        Inactive,
        NotConnected,
        ConnectedNotReady,
        ConnectedReady
    }

    private Playthrough _charisma;

    [SerializeField]
    private Text _currentConnectionState;

    public void AssignCharismaPlaythrough(Playthrough playthrough)
    {
        _charisma = playthrough;
    }

    private void Update()
    {
        if(_charisma != default)
        {
            if (_charisma.IsConnected)
            {
                if (_charisma.IsReadyToPlay)
                {
                    SetResultState(ConnectionStateResult.ConnectedReady);
                }
                else
                {
                    SetResultState(ConnectionStateResult.ConnectedNotReady);
                }
            }
            else
            {
                SetResultState(ConnectionStateResult.NotConnected);
            }
        }
        else
        {
            SetResultState(ConnectionStateResult.Inactive);
        }
    }

    private void SetResultState(ConnectionStateResult result)
    {
        switch (result)
        {
            case ConnectionStateResult.Inactive:
                SetDisplayText("Inactive");
                SetDisplayColor(Color.white);
                break;
            case ConnectionStateResult.NotConnected:
                SetDisplayText("Connecting...");
                SetDisplayColor(Color.white);
                break;
            case ConnectionStateResult.ConnectedNotReady:
                SetDisplayText("Connected - Not Ready");
                SetDisplayColor(Color.yellow);
                break;
            case ConnectionStateResult.ConnectedReady:
                SetDisplayText("Ready");
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