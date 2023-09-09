using System;
using System.Collections;
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
    private SimplePlaythrough _simplePlaythrough;

    private void Awake()
    {
        StartCoroutine(Bind());
    }

    private IEnumerator Bind()
    {
        var instance = SimplePlaythrough.Instance;

        if (instance != default)
        {
            _simplePlaythrough = instance;
            _replyButton.onClick.AddListener(SendPlayerMessage);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
            yield return Bind();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
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

        _simplePlaythrough.SendReply(_input.text);

        _input.text = string.Empty;
    }

    internal void SetOnReplyCallback(Action<string> sendReply)
    {
        _sendTextEvent = sendReply;
    }
}
