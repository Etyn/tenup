using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TenupPlaywrightClient
{
    private IBrowser? browser;
    private IBrowserContext? context;
    private IPage? page;

    public async Task<IPlaywright> InitializeAsync()
    {
        // Initialiser Playwright
        var playwright = await Playwright.CreateAsync();

        // Lancer le navigateur (Chromium par défaut)
        browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, // Mettre à true pour exécution sans interface
            SlowMo = 1000 // Ralentir pour debug (optionnel)
        });

        // Créer un contexte de navigateur
        context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36",
            Locale = "fr-FR",
            TimezoneId = "Europe/Paris"
        });

        // Créer une nouvelle page
        page = await context.NewPageAsync();

        return playwright;
    }

    public async Task<List<Tournoi>> SearchTournamentsAsync(string ville = "Bordeaux, 33300", int distance = 30, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        if (page == null)
        {
            Console.WriteLine("Le navigateur n'est pas initialisé. Appelez InitializeAsync() d'abord.");
            return new List<Tournoi>();
        }

        try
        {
            Console.WriteLine("Navigation vers la page de recherche de tournois...");

            // Naviguer vers la page de recherche
            await page.GotoAsync("https://tenup.fft.fr/recherche/tournois");

            // Attendre que la page soit chargée
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            Console.WriteLine("Fermeture de la popup de confidentialité...");

            // Fermer la popup de confidentialité
            try
            {
                await page.ClickAsync("#popin_tc_privacy_button", new PageClickOptions { Timeout = 5000 });
                await page.WaitForTimeoutAsync(1000); // Attendre que la popup se ferme
            }
            catch (Exception)
            {
                Console.WriteLine("Popup de confidentialité non trouvée ou déjà fermée.");
            }

            Console.WriteLine("Remplissage du formulaire de recherche...");

            // Recherche par ville (déjà sélectionnée par défaut, mais on s'assure)
            await page.ClickAsync("label[for='edit-recherche-type-ville']");

            // Remplir le champ ville
            await page.FillAsync("#autocomplete-custom-input", ville);
            await page.WaitForTimeoutAsync(1000); // Attendre les suggestions

            // Sélectionner la première suggestion
            var suggestion = page.Locator(".ui-autocomplete li:first-child");
            if (await suggestion.CountAsync() > 0)
            {
                await suggestion.ClickAsync();
            }

            // Sélectionner la pratique PADEL
            // D'abord cliquer sur le header pour ouvrir le conteneur
            await page.ClickAsync("#container-custom .container-custom-collapsible-header");
            await page.WaitForTimeoutAsync(500); // Attendre que le conteneur s'ouvre

            // Sélectionner PADEL
            await page.ClickAsync("label[for='edit-pratique-padel']");

            // Cliquer sur Appliquer
            await page.ClickAsync("#edit-btn-apply");

            // Définir les dates de recherche
            // D'abord cliquer sur le header des dates pour ouvrir le conteneur
            await page.ClickAsync("#container-custom--2 .container-custom-collapsible-header");
            await page.WaitForTimeoutAsync(500); // Attendre que le conteneur s'ouvre

            // Calculer les dates dynamiquement si non fournies
            var startDate = dateDebut ?? DateTime.Now;
            var endDate = dateFin ?? DateTime.Now.AddMonths(3);

            // Formatter les dates au format attendu par le site (dd/MM/yy)
            var startDateStr = startDate.ToString("dd/MM/yy", System.Globalization.CultureInfo.InvariantCulture);
            var endDateStr = endDate.ToString("dd/MM/yy", System.Globalization.CultureInfo.InvariantCulture);

            Console.WriteLine($"Dates utilisées : {startDateStr} → {endDateStr}");

            // Modifier les dates dynamiquement
            await page.FillAsync("#date-range-custom-input-start", startDateStr);
            await page.FillAsync("#date-range-custom-input-end", endDateStr);

            // Cliquer sur Appliquer
            await page.ClickAsync("#edit-btn-apply--2");

            // // Sélectionner l'épreuve Messieurs
            // Console.WriteLine("Sélection de l'épreuve Messieurs...");
            // await page.ClickAsync("#epreuves-checkboxes-replace .container-custom-collapsible-header");
            // await page.WaitForTimeoutAsync(500); // Attendre que le conteneur s'ouvre

            // // Cliquer sur Messieurs
            // await page.ClickAsync("label[for='edit-epreuve-dm']", new PageClickOptions { Force = true });
            // await page.WaitForTimeoutAsync(300);

            // // Cliquer sur Appliquer pour les épreuves
            // await page.ClickAsync("#edit-btn-apply--3");

            // // Sélectionner la catégorie P25 et P100
            // // D'abord cliquer sur le header des catégories pour ouvrir le conteneur
            // await page.ClickAsync("#categorie-tournoi-container-replace .container-custom-collapsible-header");
            // await page.WaitForTimeoutAsync(1000); // Attendre que le conteneur s'ouvre

            // // Sélectionner P25 et P100
            // Console.WriteLine("Clic sur le label P25...");
            // await page.ClickAsync("label[for='edit-categorie-tournoi-p25']", new PageClickOptions { Force = true });
            // await page.WaitForTimeoutAsync(300);

            // // Cliquer sur Appliquer
            // await page.ClickAsync("#edit-btn-apply--6");

            Console.WriteLine("Soumission du formulaire...");

            // Liste pour collecter tous les tournois de toutes les pages
            var allTournaments = new List<Tournoi>();
            int currentPage = 0;
            bool hasNextPage = true;

            while (hasNextPage)
            {
                Console.WriteLine($"Traitement de la page {currentPage + 1}...");

                // Intercepter les requêtes AJAX pour capturer la réponse JSON
                var responsePromise = page.WaitForResponseAsync(response =>
                    response.Url.Contains("/system/ajax") && response.Request.Method == "POST");

                if (currentPage == 0)
                {
                    // Première page : cliquer sur le bouton de recherche
                    await page.ClickAsync("#edit-submit");
                }
                else
                {
                    // Pages suivantes : cliquer sur le lien "next" ou le numéro de page
                    try
                    {
                        var nextPageLink = page.Locator(".pagination .next a");
                        if (await nextPageLink.CountAsync() > 0)
                        {
                            await nextPageLink.ClickAsync();
                        }
                        else
                        {
                            // Pas de lien "next", on a atteint la dernière page
                            hasNextPage = false;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors du clic sur la page suivante : {ex.Message}");
                        hasNextPage = false;
                        break;
                    }
                }

                // Attendre la réponse AJAX
                var response = await responsePromise;
                var responseText = await response.TextAsync();

                Console.WriteLine($"Réponse AJAX reçue pour la page {currentPage + 1}");

                // Parser la réponse JSON et ajouter les tournois à la liste globale
                var pageTournaments = ExtractTournaments(responseText);
                allTournaments.AddRange(pageTournaments);

                Console.WriteLine($"Page {currentPage + 1} : {pageTournaments.Count} tournois trouvés");

                // Vérifier s'il y a une page suivante en regardant la pagination
                try
                {
                    await page.WaitForTimeoutAsync(1000); // Attendre que la pagination se mette à jour
                    var nextPageLink = page.Locator(".pagination .next a");
                    hasNextPage = await nextPageLink.CountAsync() > 0;
                }
                catch (Exception)
                {
                    hasNextPage = false;
                }

                currentPage++;

                // Sécurité : limiter à 10 pages max pour éviter les boucles infinies
                if (currentPage >= 10)
                {
                    Console.WriteLine("Limite de 10 pages atteinte, arrêt de la pagination");
                    hasNextPage = false;
                }
            }

            Console.WriteLine($"Total : {allTournaments.Count} tournois trouvés sur {currentPage} page(s)");
            return allTournaments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la recherche : {ex.Message}");
            return new List<Tournoi>();
        }
    }

    private List<Tournoi> ExtractTournaments(string jsonResponse)
    {
        try
        {
            var commands = JsonConvert.DeserializeObject<List<AjaxCommand>>(jsonResponse);
            var tournaments = new List<Tournoi>();

            if (commands != null)
            {
                // Chercher la commande "recherche_tournois_update"
                var searchCommand = commands.FirstOrDefault(c => c.Command == "recherche_tournois_update");
                if (searchCommand?.Results?.Items != null)
                {
                    tournaments = searchCommand.Results.Items;
                    Console.WriteLine($"Trouvé {searchCommand.Results.NbResults} résultats, {tournaments.Count} tournois parsés");
                }
            }

            return tournaments ?? new List<Tournoi>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du parsing JSON : {ex.Message}");
            Console.WriteLine("Début de la réponse JSON :");
            Console.WriteLine(jsonResponse.Substring(0, Math.Min(500, jsonResponse.Length)));
            return new List<Tournoi>();
        }
    }

    private List<Tournoi> ExtractTournamentsFromHtml(string htmlContent)
    {
        // En cas d'échec du parsing JSON, on pourrait parser le HTML retourné
        // Pour l'instant, on retourne une liste vide
        Console.WriteLine("Tentative d'extraction depuis HTML non implémentée.");
        return new List<Tournoi>();
    }

    public async Task DisposeAsync()
    {
        if (page != null) await page.CloseAsync();
        if (context != null) await context.DisposeAsync();
        if (browser != null) await browser.DisposeAsync();
    }
}