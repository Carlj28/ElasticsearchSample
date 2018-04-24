using System;
using System.Collections.Generic;
using Nest;

namespace NestTest
{
    class Program
    {
        public static Uri node;
        public static ConnectionSettings settings;
        public static ElasticClient client;

        static void Main(string[] args)
        {
            node = new Uri("http://localhost:9200");
            settings = new ConnectionSettings(node);
            settings.DefaultIndex("dot_net_watch_list");
            client = new ElasticClient(settings);

            if (!client.IndexExists("dot_net_watch_list").Exists)
            {
                Console.WriteLine("index dot_net_watch_list does not exists.");

                CreateIndex();
            }

            if (client.Search<Movie>(s => s.MatchAll()).Total == 0)
            {
                Console.WriteLine("inserting some data");

                InsertData();
            }

            BasicQuery();

            QueryByDate();

            Console.ReadKey();
        }

        public static void CreateIndex()
        {
            Console.WriteLine("creating index dot_net_watch_list");

            var indexSettings = new IndexState
            {
                Settings = new IndexSettings
                {
                    NumberOfReplicas = 1,
                    NumberOfShards = 1
                }
            };

            //create new index
            var response = client.CreateIndex("dot_net_watch_list",
                x => x.InitializeUsing(indexSettings).Mappings(m => m.Map<Movie>(mv => mv.AutoMap())));

            Console.WriteLine(response.ToString());
        }

        public static void InsertData()
        {
            var dataToInsert = new List<Movie>(); 

            var r = new Random();
            for (var i = 0; i < r.Next(10, 100); i++)
            {
                dataToInsert.Add(new Movie
                {
                    Genre = "Test genre",
                    Description = "Test description",
                    Title = "Test title",
                    Date_of_release = DateTime.Today
                });
            }

            var response = client.IndexMany(dataToInsert, "dot_net_watch_list");
            Console.WriteLine(response.ToString());
        }

        public static void BasicQuery()
        {
            var result = client.Search<Movie>(x => x.Index("dot_net_watch_list").Query(q => q.MatchAll()));

            Console.WriteLine($"query has returned {result.Total} results");

            foreach (var document in result.Documents)
            {
                Console.WriteLine("------------");
                Console.WriteLine(document.Description);
                Console.WriteLine(document.Title);
                Console.WriteLine("------------");
            }
        }

        public static void QueryByDate()
        {
            var result = client.Search<Movie>(x => x.Index("dot_net_watch_list").Query(q => q.Match(m => m.Field(f => f.Description).Query("Test description"))));

            Console.WriteLine($"query by date has returned {result.Total} results");

            foreach (var document in result.Documents)
            {
                Console.WriteLine("------------");
                Console.WriteLine(document.Description);
                Console.WriteLine(document.Title);
                Console.WriteLine("------------");
            }
        }
    }
}
