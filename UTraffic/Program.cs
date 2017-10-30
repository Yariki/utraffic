using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UklonTraffic.Models;
using Microsoft.VisualBasic.FileIO;
using UklonTraffic.Reader;

namespace UklonTraffic
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                var list = ReadRegions();
                int countChains = 4;
                var listIndexs = GetChainsIndexes(countChains, list.Count);
                var loaders = new List<UTrafficLoader>(countChains);
                foreach (var tuple in listIndexs)
                {
                    loaders.Add(new UTrafficLoader(list, tuple.Item1, tuple.Item2));
                }
                var events = loaders.Select(l => l.GetEvent()).ToArray();
                var watch = new Stopwatch();
                watch.Start();
                loaders.ForEach(l => l.Start());
                WaitHandle.WaitAll(events);

                watch.Stop();
                Console.WriteLine($"Executed time: {watch.ElapsedMilliseconds} ms.");

                SaveResult(list);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Press any key to Exit.");
            Console.ReadKey();
        }

        private static void SaveResult(List<UTrafficStatus> list)
        {
            var filepath = Path.Combine(Environment.CurrentDirectory, "Data\\Result.txt");
            using (var file = new StreamWriter(filepath,false))
            {
                foreach (var uTrafficStatuse in list)
                {
                    file.WriteLine(uTrafficStatuse);
                }
            }
        }

        public static List<UTrafficStatus> ReadRegions()
        {
            var filepath = Path.Combine(Environment.CurrentDirectory, "Data\\Traffic.csv");
            var list = new List<UTrafficStatus>(709); // 709 - count of line in Traffic.csv - to avoid unnecessary allocation and copy operations in List<T>.

            using (TextFieldParser csvParser = new TextFieldParser(filepath,Encoding.UTF8))
            {
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;
                csvParser.ReadLine();
                while (!csvParser.EndOfData)
                {
                    string[] fields = csvParser.ReadFields();
                    string id = fields[0];
                    string name = fields[1];
                    list.Add(new UTrafficStatus() {RegionCode = Int32.Parse(id),Title = name});
                }
            }
            return list;
        }

        private static List<Tuple<int, int>> GetChainsIndexes(int countChains, int count)
        {
            var chainSize = count / countChains;
            var list = new List<Tuple<int, int>>();

            var startIndex = 0;
            for (int i = 0; i < countChains; i++)
            {
                var temp = startIndex + chainSize;
                var finishIndex = temp >= count ? count - 1 : temp;
                list.Add(new Tuple<int, int>(startIndex, finishIndex));
                startIndex = finishIndex + 1;
            }
            return list;
        }

        public static String GetTimestamp()
        {
            long ticks = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
            ticks /= 10000000;
            return ticks.ToString();

        }

    }
}
