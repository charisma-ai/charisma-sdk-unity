using System;
using UnityEngine;
using UnityEngine.UI;

public class SimpleCharismaPlayer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Text source, used to get")]
    private InputField _input;

    [SerializeField]
    [Tooltip("Reply button, triggers the reply event on click.")]
    private Button _replyButton;

    private Action<string> _sendTextEvent;

    // Start is called before the first frame update
    void Start()
    {
        _replyButton.onClick.AddListener(SendPlayerMessage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(key: KeyCode.Return))
        {
            SendPlayerMessage();
        }
    }

    private void SendPlayerMessage()
    {
        if (string.IsNullOrEmpty(value: _input.text))
        { 
            return; 
        }

        _sendTextEvent?.Invoke(_input.text);

        _input.text = string.Empty;
    }

    internal void SetOnReplyCallback(Action<string> sendReply)
    {
        _sendTextEvent = sendReply;
    }
}
