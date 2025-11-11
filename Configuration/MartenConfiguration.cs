using Marten;
using Marten.NodaTimePlugin;
using Marten.Services;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Tenup.Repositories;
using Weasel.Core;

namespace Tenup.Configuration
{
    public static class MartenConfiguration
    {
        public static void ConfigureMarten(this IServiceCollection services, string connectionString)
        {
            Console.WriteLine($"Configuration MartenDB avec : {connectionString}");
            
            services.AddMarten(options =>
            {
                // Établir la chaîne de connexion vers la base de données Marten
                options.Connection(connectionString);

                var serializer = new JsonNetSerializer();
                
                // Pour stocker les enums comme des chaînes
                serializer.EnumStorage = EnumStorage.AsString;
                options.Serializer(serializer);
                UseNodaTime(options);

                // Inclure le registre des tournois
                options.Schema.Include<TournoiRegistry>();

                // Création automatique des objets de schéma - OBLIGATOIRE
                options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
                
                Console.WriteLine("✅ Configuration MartenDB terminée");
            });
        }

        private static void UseNodaTime(StoreOptions options)
        {
            NodaTimeExtensions.SetNodaTimeTypeMappings();

            options.Advanced.ModifySerializer(serializer =>
            {
                if (serializer is JsonNetSerializer jsonNetSerializer)
                {
                    jsonNetSerializer.Customize(s =>
                    {
                        s.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                    });
                }
            });
        }
    }
}