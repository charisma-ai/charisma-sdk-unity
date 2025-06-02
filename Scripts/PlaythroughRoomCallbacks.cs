using System;
using Newtonsoft.Json;
using UnityEngine;
using Colyseus;
using CharismaSDK.Audio;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

namespace CharismaSDK
{
    public partial class Playthrough
    {
        private bool _assignedCallbacks;

        private void AssignRoomCallbacks(ColyseusRoom<object> room)
        {
            if (room == null)
            {
                Logger.LogError("Cannot assign room callbacks - Room is null");
                return;
            }

            if (_assignedCallbacks)
            {
                Logger.LogError("Cannot assign room callbacks - already assigned.");
                return;
            }

            room.OnJoin += OnRoomJoin;
            room.OnError += OnRoomError;
            room.OnLeave += OnRoomLeave;
            room.OnMessage<string>("status", OnStatusMessageReceived);
            room.OnMessage<Events.MessageEvent>("message", OnMessageReceived);
            room.OnMessage<Events.StartTypingEvent>("start-typing", OnStartTypingReceived);
            room.OnMessage<Events.StopTypingEvent>("stop-typing", OnStopTypingReceived);
            room.OnMessage<Events.SpeechRecognitionResult>("speech-recognition-result", OnSpeechRecognitionResultReceived);
            room.OnMessage<Events.ProblemEvent>("problem", OnProblemReceived);
            room.OnMessage<string>("pong", OnPongReceived);

        }

        private void OnRoomJoin()
        {
            Logger.Log("Successfully connected to playthrough");
        }

        private void OnRoomError(int code, string message)
        {
            Logger.LogError($"There was an error connecting to the playthrough. Code: {code}. Message: {message}");
        }

        private void OnStatusMessageReceived(string status)
        {
            Logger.Log($"Received `status` event: {JsonConvert.SerializeObject(status)}");
            if (status == "ready")
            {
                Logger.Log("Ready to begin play");
                OnReady();
            }
        }

        private void OnMessageReceived(Events.MessageEvent message)
        {
            Logger.Log($"Received `message` event: {JsonConvert.SerializeObject(message)}");
            OnMessage?.Invoke(message);
        }

        private void OnStartTypingReceived(Events.StartTypingEvent message)
        {
            Logger.Log($"Received `start-typing` event: {JsonConvert.SerializeObject(message)}");
            OnStartTyping?.Invoke(message);
        }

        private void OnStopTypingReceived(Events.StopTypingEvent message)
        {
            Logger.Log($"Received `stop-typing` event: {JsonConvert.SerializeObject(message)}");
            OnStopTyping?.Invoke(message);
        }

        private void OnSpeechRecognitionResultReceived(Events.SpeechRecognitionResult message)
        {
            Logger.Log($"Received `speech-recognition-result` event: {JsonConvert.SerializeObject(message)}");
            OnSpeechRecognitionResult?.Invoke(message);
        }

        private void OnProblemReceived(Events.ProblemEvent message)
        {
            Logger.LogWarning($"Received `problem` event: {JsonConvert.SerializeObject(message)}");
        }

        private void OnPongReceived(string message)
        {
            _pingCount = 0;
            OnPingSuccess?.Invoke();
        }

        private void OnRoomLeave(int code)
        {
            Logger.Log("Connection closed.");

            if (_calledByDisconnect)
            {
                return;
            }

            Logger.Log("Attempting to reconnect to Playthrough.");
            MainThreadDispatcher.Instance.Consume(TryToReconnect());
        }
    }
}
