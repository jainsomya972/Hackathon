﻿using Newtonsoft;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SpeechToText
{
    public class ResponseEventArgs : EventArgs
    {
        public string interimResponse;
        public ResponseEventArgs(string result)
        {
            interimResponse = result;
        }
    }

    public delegate void ResponseEventHandler(object sender, ResponseEventArgs e);
    class speechtotext
    {
        public event ResponseEventHandler ResponseReceived;

        protected virtual void OnResponseRecieved(ResponseEventArgs e)
        {
            if (ResponseReceived != null) ResponseReceived(this, e);
        }
        string res = "";
        public static Rootobject parse(string json)
        {
            Rootobject root = new Rootobject();
            //Console.WriteLine(".........................");
            Newtonsoft.Json.JsonConvert.PopulateObject(json, root);
            return root;
        }
        public static string GetSummaryFromJSON(string json)
        {
            Rootobject rootobject = speechtotext.parse(json);
            string transcript = "";
            for (int i = 0; i < rootobject.results.Length; i++)
            {
                transcript += rootobject.results[i].alternatives[0].transcript.ToString() + ".";                
            }
            return transcript;
        }
        public static string fromWaveFile(string path)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(
                           "c3c96c75-0fec-4020-a1af-59857ab28bdc:qXlbSWgM2J5T")));

                var content = new StreamContent(new FileStream(path, FileMode.Open));
                content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                var response = client.PostAsync("https://stream.watsonplatform.net/speech-to-text/api/v1/recognize?interim_results=false&model=en-US_NarrowbandModel", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    //Console.WriteLine(res);
                }
                string json = response.Content.ReadAsStringAsync().Result;
                return json;
            }
        }
        public static string fromflacFile(string path)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(
                           "c3c96c75-0fec-4020-a1af-59857ab28bdc:qXlbSWgM2J5T")));

                var content = new StreamContent(new FileStream(path, FileMode.Open));
                content.Headers.ContentType = new MediaTypeHeaderValue("audio/flac");
                var response = client.PostAsync("https://stream.watsonplatform.net/speech-to-text/api/v1/recognize?interim_results=false&model=en-US_NarrowbandModel", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    //Console.WriteLine(res);
                }
                string json = response.Content.ReadAsStringAsync().Result;
                return json;
            }
        }
        public static string fromMp3File(string path)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(
                           "c3c96c75-0fec-4020-a1af-59857ab28bdc:qXlbSWgM2J5T")));

                var content = new StreamContent(new FileStream(path, FileMode.Open));
                content.Headers.ContentType = new MediaTypeHeaderValue("audio/mp3");
                var response = client.PostAsync("https://stream.watsonplatform.net/speech-to-text/api/v1/recognize?interim_results=false", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string res = response.Content.ReadAsStringAsync().Result;
                    //Console.WriteLine(res);
                }
                string json = response.Content.ReadAsStringAsync().Result;
                return json;
            }
        }
        string tempPath = Path.GetTempPath();
        public void GetResponses()
        {            
            for (int j = 0; ; j++)
            {
                try
                {
                    //Thread.Sleep(500);
                    // Console.WriteLine("reading from temp" + j.ToString());
                    if (File.Exists(tempPath+"temp" + j.ToString() + ".wav"))
                    {
                        Rootobject rootobject = new Rootobject();
                        speechtotext speechtotext = new speechtotext();
                        string json = speechtotext.fromWaveFile(tempPath + "temp" + j.ToString() + ".wav");
                        rootobject = speechtotext.parse(json);

                        for (int i = 0; i < rootobject.results.Length; i++)
                        {
                            string transcript = rootobject.results[i].alternatives[0].transcript.ToString();
                            //triggering the event response recieved
                            OnResponseRecieved(new ResponseEventArgs(transcript));                            
                        }
                    }
                    else
                        return;
                }
                catch
                {
                    j--; 
                }
            }
        }
        public static int recordingFlag = 0;

        public string Res { get => res; set => res = value; }

        public void recordInChunks()
        {
            for (int i = 0; recordingFlag!=0; i++)
            {
                //Console.WriteLine("writing on temp" + i.ToString());
                string filename = tempPath + "temp" + i.ToString() + ".wav";
                AudioRecorder recorder = new AudioRecorder();
                recorder.startRecording(filename);
                System.Threading.Thread.Sleep(5000);
                //Console.WriteLine("Press any key to split");
                //Console.ReadKey();
                recorder.stopRecording();
            }
        }
        public void StartTranscribe()
        {
            recordingFlag = 1;
            Thread recorderThread = new Thread(new ThreadStart(recordInChunks));            
            Thread responseThread = new Thread(new ThreadStart(GetResponses));
            recorderThread.Start();
            Thread.Sleep(5300);
            responseThread.Start();
        }
        public void stopTranscription()
        {
            recordingFlag = 0;
            for (int i = 0;  ; i++)
            {
                if (File.Exists(tempPath + "temp" + i.ToString() + ".wav"))
                {
                    try
                    {
                        File.Delete(tempPath + "temp" + i.ToString() + ".wav");
                    }
                    catch { }
                }
                else
                    return;
            }
        }
    }

}