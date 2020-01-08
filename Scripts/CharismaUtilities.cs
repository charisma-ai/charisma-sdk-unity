using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using Newtonsoft.Json;
using UnityEngine;

namespace CharismaSdk
{
    public static class CharismaUtilities
    {

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
