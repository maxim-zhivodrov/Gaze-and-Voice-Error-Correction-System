
namespace EyeGaze.SpeechToText
{
    interface InterfaceSpeechToText
    {
        void connect(string key, string keyInfo);
        string listen();
        void disconnect();
    }
}
