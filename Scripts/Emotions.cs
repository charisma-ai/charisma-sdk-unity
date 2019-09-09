using Newtonsoft.Json;
using UnityEngine;

namespace CharismaSDK
{
    public class Emotions
    {
        [Header("Low = sad, High = happy")]
        [SerializeField][Range(0,100)]private int happiness;
        [Header("Low = calm, High = angry")]
        [SerializeField][Range(0,100)]private int anger;
        [Header("Low = untrusting, High = trusting")]
        [SerializeField][Range(0,100)]private int trust;
        [Header("Low = impatient, High = patient")]
        [SerializeField][Range(0,100)]private int patience;
        [Header("Low = scared, High = fearless")]
        [SerializeField][Range(0,100)]private int fearlessness;
        
        [JsonConstructor]
        public Emotions(int happiness, int anger, int trust, int patience, int fearlessness)
        {
            this.happiness = happiness;
            this.anger = anger;
            this.trust = trust;
            this.patience = patience;
            this.fearlessness = fearlessness;
        }

        public int Happiness
        {
            get => happiness;
            set => happiness = value;
        }

        public int Anger
        {
            get => anger;
            set => anger = value;
        }

        public int Trust
        {
            get => trust;
            set => trust = value;
        }

        public int Patience
        {
            get => patience;
            set => patience = value;
        }

        public int Fearlessness
        {
            get => fearlessness;
            set => fearlessness = value;
        }    
    }
}
