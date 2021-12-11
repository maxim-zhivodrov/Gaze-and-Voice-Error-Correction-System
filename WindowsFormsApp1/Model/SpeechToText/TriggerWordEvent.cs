using System;

namespace EyeGaze.SpeechToText
{
    public delegate void TriggerWordHandler(object sender, TriggerWordEvent message);
    public class TriggerWordEvent : EventArgs
    {
        public string triggerWord { get; set; }
        public string[] content { get; set; }

    }
}
