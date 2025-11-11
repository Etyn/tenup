using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class TenupApiClient
{
    private readonly HttpClient httpClient;
    private readonly CookieContainer cookieContainer;

    public TenupApiClient()
    {
        this.cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler()
        {
            CookieContainer = this.cookieContainer
        };

        this.httpClient = new HttpClient(handler);

        // Configuration des en-têtes HTTP
        ConfigureHeaders();
    }

    private void ConfigureHeaders()
    {
        var headers = httpClient.DefaultRequestHeaders;

        headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        headers.Add("Accept-Language", "fr-FR,fr;q=0.9,en-CA;q=0.8,en;q=0.7,en-GB;q=0.6,en-US;q=0.5");
        headers.Add("Origin", "https://tenup.fft.fr");
        headers.Add("Referer", "https://tenup.fft.fr/recherche/tournois");
        headers.Add("Sec-Ch-Ua", "\"Not;A=Brand\";v=\"99\", \"Google Chrome\";v=\"139\", \"Chromium\";v=\"139\"");
        headers.Add("Sec-Ch-Ua-Mobile", "?0");
        headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
        headers.Add("Sec-Fetch-Dest", "empty");
        headers.Add("Sec-Fetch-Mode", "cors");
        headers.Add("Sec-Fetch-Site", "same-origin");
        headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");
        headers.Add("X-Api-Key", "15b12194-cca7-475c-9476-52e81851a654");
        headers.Add("X-Requested-With", "XMLHttpRequest");
    }

    public async Task<List<Tournoi>> SearchTournamentsAsync()
    {
        var url = "https://tenup.fft.fr/system/ajax";

        try
        {
            // Payload basé sur la requête réelle qui fonctionne
            var formData = new Dictionary<string, string>
            {
                // Paramètres de recherche principaux
                { "recherche_type", "ville" },
                { "ville[autocomplete][country]", "fr" },
                { "ville[autocomplete][textfield]", "" },
                { "ville[autocomplete][value_container][value_field]", "Bordeaux, 33300" },
                { "ville[autocomplete][value_container][label_field]", "Bordeaux, 33300" },
                { "ville[autocomplete][value_container][lat_field]", "44.851897" },
                { "ville[autocomplete][value_container][lng_field]", "-0.587877" },
                { "ville[distance][value_field]", "30" },
                { "club[autocomplete][textfield]", "" },
                { "club[autocomplete][value_container][value_field]", "" },
                { "club[autocomplete][value_container][label_field]", "" },
                { "filter_mine", "1" },
                { "pratique", "PADEL" },
                { "date[start]", "01/09/25" },
                { "date[end]", "01/12/25" },
                { "epreuve[DM]", "DM" },
                { "categorie_tournoi[P100]", "P100" },
                { "page", "0" },
                { "sort", "dateDebut asc" },
                
                // Tokens de formulaire (à actualiser si nécessaire)
                // { "form_build_id", "form-m38hWzJSV7jIC6kHWl8yC0V4ahxdCqtr3EX9RyHy8vQ" },
                // { "form_token", "1Zj1EtqBRLSW3OdRcOMr12EF5DXpyYw6P7nPheAh94c" },
                // { "form_id", "recherche_tournois_form" },
                // { "_triggering_element_name", "submit_main" },
                // { "_triggering_element_value", "Rechercher" }
            };

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded")
            {
                CharSet = "UTF-8"
            };

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Réponse de l'API :");
                Console.WriteLine(responseContent);

                // Parser la réponse JSON
                if (responseContent.TrimStart().StartsWith("[") || responseContent.TrimStart().StartsWith("{"))
                {
                    throw new NotImplementedException("La méthode d'extraction des tournois n'est pas encore implémentée.");
                    // var tournaments = ExtractTournaments(responseContent);
                    // return tournaments;
                }
                else
                {
                    Console.WriteLine("La réponse n'est pas du JSON valide.");
                    return new List<Tournoi>();
                }
            }
            else
            {
                Console.WriteLine($"Erreur HTTP : {response.StatusCode}");
                return new List<Tournoi>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'appel API : {ex.Message}");
            return new List<Tournoi>();
        }
    }

    // private List<Tournoi> ExtractTournaments(string jsonResponse)
    // {
    //     try
    //     {
    //         var responseData = JsonConvert.DeserializeObject<List<ResponseData>>(jsonResponse);
    //         var tournaments = new List<Tournoi>();

    //         if (responseData != null && responseData.Count > 0)
    //         {
    //             var items = responseData[0].RechercheTournoisUpdate.Items;
    //             tournaments = items.ConvertAll(i => i.Tournoi);
    //         }

    //         return tournaments ?? new List<Tournoi>();
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Erreur lors du parsing JSON : {ex.Message}");
    //         return new List<Tournoi>();
    //     }
    // }

    public void Dispose()
    {
        httpClient?.Dispose();
    }
}