using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UklonTraffic.Models;

namespace UklonTraffic.Reader
{
    public class UTrafficLoader
    {
        private Thread loadingThread = null;
        private int startIndex = -1;
        private int finishIndex = -1;
        private List<UTrafficStatus> list;
        private AutoResetEvent eventThread;


        // I use List<T> because I don't write any information to List<T> and it's easier to get access to item by index. In other case it's better to use colletions from namespace "Concurrent".

        public UTrafficLoader(List<UTrafficStatus> list, int start, int finish)
        {
            this.list = list;
            startIndex = start;
            finishIndex = finish;
            eventThread = new AutoResetEvent(false);
        }
        
        public void Start()
        {
            if (list == null || list.Count == 0)
            {
                Console.WriteLine("List of data is empty.");
                return;
            }
                if (startIndex < 0 || finishIndex < 0 || finishIndex >= list.Count)
            {
                Console.WriteLine("startIndex or finishIndex is out of boundary.");
                return;
            }
            
            loadingThread = new Thread(DoLoading);
            loadingThread.IsBackground = true;
            loadingThread.Start();
        }

        public AutoResetEvent GetEvent()
        {
            eventThread.Reset();
            return eventThread;
        }

        private void DoLoading(object arg)
        {
            try
            {
                for (int i = startIndex; i <= finishIndex; i++)
                {
                    var trafficStatus = list[i];
                    var regionCode = trafficStatus.RegionCode;
                    var timeStamp = GetTimestamp();
                    var url = $"https://export.yandex.com/bar/reginfo.xml?region={regionCode}&bustCache={timeStamp}";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    if (request != null)
                    {
                        try
                        {
                            request.Timeout = 2000;
                            using (var response = (HttpWebResponse) request.GetResponse())
                            {
                                switch (response.StatusCode)
                                {

                                    case HttpStatusCode.OK:
                                        Stream resp = response.GetResponseStream();
                                        if (resp != null)
                                        {
                                            Encoding fromenc = Encoding.GetEncoding(response.CharacterSet);
                                            StreamReader reader = new StreamReader(resp, fromenc);
                                            XmlSerializer serializer = new XmlSerializer(typeof(UInfo));
                                            var info = serializer.Deserialize(reader) as UInfo;
                                            if (info != null && info.Traffic != null && info.Traffic.RegionInfo != null)
                                            {
                                                trafficStatus.TrafficValue = info.Traffic.RegionInfo.Level;
                                                trafficStatus.Status = eUStatus.Success;
                                                Console.WriteLine(
                                                    $"RegionCode: {trafficStatus.RegionCode}; Status: {trafficStatus.Status}");
                                            }
                                            else
                                            {
                                                trafficStatus.Status = eUStatus.TrafficNotFound;
                                            }
                                            
                                            reader.Close();
                                            resp.Close();
                                        }
                                        break;
                                    default:
                                        trafficStatus.Status = eUStatus.Error;
                                        break;
                                }
                                
                            }

                        }
                        catch (WebException e)
                        {
                            trafficStatus.Status = eUStatus.Timeout;
                            Console.WriteLine($"RegionCode: {trafficStatus.RegionCode};  Url ({url}) ; Problem: {e.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                eventThread.Set();
            }
        }


        public static String GetTimestamp()
        {
            long ticks = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
            ticks /= 10000000; 
            return ticks.ToString();

        }

    }
}