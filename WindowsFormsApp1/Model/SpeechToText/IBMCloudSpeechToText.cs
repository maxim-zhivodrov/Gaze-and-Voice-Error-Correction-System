using System;
using System.Net.WebSockets;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using NAudio.Wave;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using EyeGaze.SpeechToText;
using EyeGaze.Logger;

namespace EyeGaze.SpeechToText
{
    class IBMCloudSpeechToText : InterfaceSpeechToText
    {

        private WaveInEvent waveIn;
        private WaveFormat format;
        private ClientWebSocket ws;
        private ArraySegment<byte> openingMessage;
        private ArraySegment<byte> closingMessage;

        public IBMCloudSpeechToText()
        {
            format = new WaveFormat(16000, 16, 1);
            waveIn = new WaveInEvent
            {
                BufferMilliseconds = 50,
                DeviceNumber = 0,
                WaveFormat = format
            };

            ws = new ClientWebSocket();
            openingMessage = new ArraySegment<byte>(Encoding.UTF8.GetBytes(
            "{\"action\": \"start\", \"content-type\": \"audio/l16;rate=16000\", \"interim_results\": true, \"timestamps\": true }"));
            closingMessage = new ArraySegment<byte>(Encoding.UTF8.GetBytes(
            "{\"action\": \"stop\"}"));
        }
        public void connect(string key, string keyInfo)
        {
            try
            {
                waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(WaveIn_DataAvailable);
                ws.Options.Credentials = new NetworkCredential("apikey", key);
                ws.ConnectAsync(new Uri(keyInfo), CancellationToken.None).Wait();
                ws.SendAsync(openingMessage, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                Task.WaitAll(
                    HandleResults(ws)
                );
                waveIn.StartRecording();
                SystemLogger.getEventLog().Info("IBM speech to text - connect successfully");
            }
            catch (AggregateException e)
            {
                foreach (var x in e.Flatten().InnerExceptions)
                {
                    if (x.InnerException is WebException)
                    {
                        System.Net.WebException innerException = (System.Net.WebException)x.InnerException;
                        System.Net.WebExceptionStatus status = innerException.Status;
                        if (status == WebExceptionStatus.ProtocolError)
                        {
                            SystemLogger.getErrorLog().Error("IBM cloud authentication is wrong");
                            throw new WrongAuthenticationException("IBM cloud authentication is wrong");
                        }
                        else if (status == WebExceptionStatus.NameResolutionFailure || status == WebExceptionStatus.ConnectFailure)
                        {
                            SystemLogger.getErrorLog().Error("IBM speech to text - connection to Internet faild");
                            throw new ConnectionFailedException("IBM speech to text - connection to Internet faild");
                        }
                    }
                    else
                    {
                        SystemLogger.getErrorLog().Error("IBM speech to text - a problem occurred while truing to connect IBM");
                    }
                }

            }



        }

        public string listen()
        {
            Task<string> task = null;
            try
            {
                Task.WaitAll(task = HandleResults(ws));
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Error("IBM speech to text - connection to Internet faild");
                throw new ConnectionFailedException("IBM speech to text - connection to Internet faild");
            }
            SystemLogger.getEventLog().Info("IBM cloud found : " + task.Result);
            return task.Result;
        }

        public async void disconnect()
        {
            if (waveIn != null)
                waveIn.Dispose();
            waveIn = null;

            if (ws.State == WebSocketState.Open)
            {
                await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
            }
            SystemLogger.getEventLog().Info("IBM speech to text - disconnect successfully");
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (ws.State == WebSocketState.Open)
                    ws.SendAsync(new ArraySegment<byte>(e.Buffer), WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
            }
            catch (Exception exception)
            {
                throw exception.InnerException;
            }
        }

        private async Task<string> HandleResults(ClientWebSocket ws)
        {
            var buffer = new byte[2048];

            while (true)
            {
                ArraySegment<byte> segment;
                WebSocketReceiveResult result;

                try
                {
                    segment = new ArraySegment<byte>(buffer);
                    result = await ws.ReceiveAsync(segment, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    throw exception.InnerException;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return "";
                }

                int count = result.Count;

                while (!result.EndOfMessage)
                {
                    if (count >= buffer.Length)
                    {
                        count = buffer.Length - 1;
                    }

                    segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                    result = await ws.ReceiveAsync(segment, CancellationToken.None);
                    count += result.Count;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, count);
                Debug.WriteLine(message);
                if (!IsDelimeter(message))
                {
                    try
                    {
                        JObject json = JObject.Parse(message);
                        Boolean final = (Boolean)json["results"][0]["final"];
                        string wordsResult = "";
                        if (final)
                        {
                            wordsResult = (string)json["results"][0]["alternatives"][0]["transcript"];
                            Debug.WriteLine(wordsResult);
                            return wordsResult;
                        }
                    }
                    catch { }
                }
                else
                    return "";
            }

        }

        [DataContract]
        internal class ServiceState
        {
            [DataMember]
            public string state = "";
        }
        static bool IsDelimeter(String json)
        {
            try
            {
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ServiceState));
                ServiceState obj = (ServiceState)ser.ReadObject(stream);
                return obj.state == "listening";
            }
            catch (Exception exception)
            {
                return false;
            }
        }
    }
}