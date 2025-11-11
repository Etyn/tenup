using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using Tenup.Configuration;
using Tenup.Mappers;
using Tenup.Repositories;
using Tenup.Services;
using Microsoft.ApplicationInsights;
using Marten;
using SendGrid;

Console.WriteLine("Recherche de tournois FFT avec Playwright et sauvegarde en base");

// Configuration des services avec Host Builder
var builder = Host.CreateApplicationBuilder(args);

// Configuration de la chaîne de connexion PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
    ?? "Host=localhost;Database=tenup;Username=postgres;Password=sa;";

// Configuration de MartenDB
builder.Services.ConfigureMarten(connectionString);

// Configuration d'Application Insights
builder.Services.AddApplicationInsightsTelemetryWorkerService();

// Enregistrement du repository
builder.Services.AddScoped<TournoiRepository>();

// Enregistrement du client Playwright
builder.Services.AddScoped<TenupPlaywrightClient>();

// Configuration SendGrid
var sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? "";
builder.Services.AddTransient<ISendGridClient>(_ => new SendGridClient(sendGridApiKey));

// Enregistrement du service d'email
builder.Services.AddScoped<EmailService>();

// Build de l'application
using var host = builder.Build();

// Récupération des services
using var scope = host.Services.CreateScope();
var tournoiRepository = scope.ServiceProvider.GetRequiredService<TournoiRepository>();
var playwrightClient = scope.ServiceProvider.GetRequiredService<TenupPlaywrightClient>();
var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

try
{
    // Initialiser le navigateur
    using (await playwrightClient.InitializeAsync())
    {
        // Calculer les dates dynamiquement : aujourd'hui + 4 mois
        var dateDebut = DateTime.Now;
        var dateFin = dateDebut.AddMonths(3);

        Console.WriteLine($"Recherche de tournois du {dateDebut:dd/MM/yyyy} au {dateFin:dd/MM/yyyy}");

        // Rechercher les tournois avec dates dynamiques
        var tournaments = await playwrightClient.SearchTournamentsAsync("Bordeaux, 33300", 30, dateDebut, dateFin);

        if (tournaments != null && tournaments.Count > 0)
        {
            Console.WriteLine($"\nTrouvé {tournaments.Count} tournois au total");

            // Filtrer les tournois selon nos critères
            var filteredTournaments = FilterTournaments(tournaments);
            Console.WriteLine($"Après filtrage : {filteredTournaments.Count} tournois (P25/P100 + DM + Seniors uniquement)");

            // Mapper les tournois FILTRÉS vers les entités de base de données
            var tournoisDb = filteredTournaments.ToDbList("Bordeaux, 33300", 30);

            // Sauvegarder en base de données - OBLIGATOIRE
            Console.WriteLine("\nSauvegarde en base de données...");
            var savedIds = await tournoiRepository.AddTournoisAsync(tournoisDb);
            Console.WriteLine($"{savedIds.Count} tournois sauvegardés en base");

            // Envoyer un email pour les nouveaux tournois
            if (savedIds.Count > 0)
            {
                Console.WriteLine("\nEnvoi d'email pour les nouveaux tournois...");
                var nouveauxTournois = tournoisDb.Where(t => savedIds.Contains(t.Id)).ToList();
                await emailService.SendNewTournamentsEmailAsync(nouveauxTournois);
            }

            // Vérifier le nombre total de tournois en base
            var totalCount = await tournoiRepository.CountAsync();
            Console.WriteLine($"\nTotal de nouveau tournoi : {savedIds.Count}");
        }
        else
        {
            Console.WriteLine("Aucun tournoi trouvé.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erreur : {ex.Message}");
    Console.WriteLine($"Stack trace : {ex.StackTrace}");
}
finally
{
    // Nettoyer les ressources
    try
    {
        await playwrightClient.DisposeAsync();
    }
    catch (Exception)
    {
        // Ignorer les erreurs de nettoyage
        Console.WriteLine("Ressources nettoyées (avec quelques avertissements normaux)");
    }
}

/// <summary>
/// Filtre les tournois selon nos critères :
/// - Catégories P25 et P100 uniquement (TypeEpreuve.Code)
/// - Double Messieurs (DM) uniquement (NatureEpreuve.Code)
/// - Seniors uniquement (CategorieAge.Id = 200)
/// </summary>
static List<Tournoi> FilterTournaments(List<Tournoi> tournaments)
{
    var filteredTournaments = new List<Tournoi>();

    foreach (var tournoi in tournaments)
    {
        // Vérifier si le tournoi a au moins une épreuve qui correspond à nos critères
        bool hasValidEpreuve = false;

        if (tournoi.Epreuves != null && tournoi.Epreuves.Count > 0)
        {
            foreach (var epreuve in tournoi.Epreuves)
            {
                // Vérifier la catégorie (P25 ou P100)
                bool isValidCategory = epreuve.TypeEpreuve?.Code == "P25" || epreuve.TypeEpreuve?.Code == "P100";

                // Vérifier la nature (Double Messieurs)
                bool isDoubleMen = epreuve.NatureEpreuve?.Code == "DM";

                // Vérifier la catégorie d'âge (Seniors)
                bool isSenior = epreuve.CategorieAge?.Id == 200;

                if (isValidCategory && isDoubleMen && isSenior)
                {
                    hasValidEpreuve = true;
                    Console.WriteLine($"✅ Tournoi retenu : {tournoi.Libelle} - {epreuve.TypeEpreuve?.Code} {epreuve.NatureEpreuve?.Code} (CategorieAge: {epreuve.CategorieAge?.Id})");
                    break; // Pas besoin de vérifier les autres épreuves
                }
            }
        }

        if (hasValidEpreuve)
        {
            filteredTournaments.Add(tournoi);
        }
        else
        {
            // Afficher les détails des épreuves pour comprendre pourquoi le tournoi est exclu
            var epreuvesDetails = "";
            if (tournoi.Epreuves != null && tournoi.Epreuves.Count > 0)
            {
                var details = tournoi.Epreuves.Select(e =>
                    $"{e.TypeEpreuve?.Code ?? "NULL"}-{e.NatureEpreuve?.Code ?? "NULL"}-{e.CategorieAge?.Id.ToString() ?? "NULL"}");
                epreuvesDetails = string.Join(", ", details);
            }
            else
            {
                epreuvesDetails = "Aucune épreuve";
            }

            Console.WriteLine($"❌ Tournoi exclu : {tournoi.Libelle} - Épreuves: [{epreuvesDetails}]");
        }
    }

    return filteredTournaments;
}
