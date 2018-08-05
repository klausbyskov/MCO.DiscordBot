using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace MCO.DiscordBot
{
    /// <summary>
    /// Fetches data from mco.life by mikef
    /// </summary>
    internal class McoLifeService
    {
        public static async Task<Reservations> GetReservations()
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Referrer", "http://www.mco.life/");
            httpClient.DefaultRequestHeaders.Add("Origin", "http://www.mco.life/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            var data = await httpClient.GetByteArrayAsync("https://mco-life-api.herokuapp.com/status");

            var serializer = new DataContractJsonSerializer(typeof(McoLifeResult));

            using (var memStream = new MemoryStream(data))
            {
                var result = (McoLifeResult)serializer.ReadObject(memStream);
                return result.Reservations;
            }
        }
    }

    [DataContract]
    internal class McoLifeResult
    {
        [DataMember(Name ="reservations")]
        public Reservations Reservations { get; set; }
    }

    [DataContract]
    internal class Reservations
    {
        [DataMember(Name = "total")]
        public int Total { get; set; }
        [DataMember(Name = "today")]
        public int Today { get; set; }
    }
}
