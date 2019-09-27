using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using Newtonsoft.Json;
using UnityEngine;

namespace CharismaSDK
{
    public static class CharismaUtilities
    {
        #region Audio

        public static AudioClip FromMp3Data(byte[] data)
        {
            // Load the data into a stream
            var mp3Stream = new MemoryStream(data);
            // Convert the data in the stream to WAV format
            var mp3Audio = new Mp3FileReader(mp3Stream);
            var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Audio);
            // Convert to WAV data
            var wav = new Wav(AudioMemStream(waveStream).ToArray());
            
            var audioClip = AudioClip.Create("CharismaSpeech", wav.SampleCount, 1,wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            // Return the clip
            return audioClip;
        }
        
        private static MemoryStream AudioMemStream(WaveStream waveStream)
        {
            var outputStream = new MemoryStream();
            
            using (var waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
            {
                var bytes = new byte[waveStream.Length];
                waveStream.Position = 0;
                waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
                waveFileWriter.Write(bytes, 0, bytes.Length);
                waveFileWriter.Flush();
            }
            return outputStream;
        }
        
        #endregion
        
        /// <summary>
        /// Convert token response to token.
        /// </summary>
        /// <param name="tokenResponse">JSON payload</param>
        /// <returns></returns>
        public static string TokenToString(string tokenResponse)
        {
            try
            {
                var responseParams = JsonConvert.DeserializeObject<TokenResponseParams>(tokenResponse);			
                return responseParams.Token;
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not deserialize token. Is your debug token up to date?: {e}");
                throw;
            }
        }

        /// <summary>
        /// Generate a conversation from the response received from Charisma.
        /// </summary>
        /// <param name="conversationResponse">JSON payload</param>
        /// <returns></returns>
        public static Conversation GenerateConversation(string conversationResponse)
        {
            try
            {
                var responseParams = JsonConvert.DeserializeObject<Conversation>(conversationResponse);
                return responseParams;
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not deserialize conversationResponse. Is your token valid?: {e}");
                throw;
            }
        }
        
        /// <summary>
        /// Generate a Charisma response from the response string.
        /// </summary>
        /// <param name="charismaResponse">JSON payload</param>
        /// <returns></returns>
        public static async Task<Response> GenerateResponse(string charismaResponse)
        {
            var modifiedString = charismaResponse.Remove(charismaResponse.Length-1, 1).Remove(0, 11);
            var message = await Task<Response>.Run(() => JsonConvert.DeserializeObject<Response>(modifiedString));

            return message;
        }

        public static string ToJson<T>(T obj)
        {
            var jObject = JsonConvert.SerializeObject(obj);
            
            return jObject;
        }
    }
}
