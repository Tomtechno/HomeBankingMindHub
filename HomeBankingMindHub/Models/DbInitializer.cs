using System;
using System.Linq;

namespace HomeBankingMindHub.Models
{
    public class DBInitializer
    {
        public static void Initialize(HomeBankingContext context)
        {
            if (!context.Clients.Any())
            {
                var clients = new Client[]
                {
                    new Client { Email = "vcoronado@gmail.com", FirstName="Victor", LastName="Coronado", Password="123v23r456"},
                    new Client { Email = "eduardo@gmail.com", FirstName="Eduardo", LastName="Mendoza", Password="aaet342"},
                    new Client { Email = "eren@gmail.com", FirstName="Eren", LastName="Jaeger", Password="Pasfd#a2"},
                    new Client { Email = "mikasa12@gmail.com", FirstName="Mikasa", LastName="Ackerman", Password="o2314nnsaA%"},
                    new Client { Email = "tomas@gmail.com", FirstName="Tomas", LastName="Uzquiano", Password="hae345b%"},

                };

                foreach (Client client in clients)
                {
                    context.Clients.Add(client);
                }

                //guardamos
                context.SaveChanges();
            }

        }
    }
}

