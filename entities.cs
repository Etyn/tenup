using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class Tournoi
{
    public string Id { get; set; }
    public string Type { get; set; }
    public int Millesime { get; set; }
    public string Code { get; set; }
    public string Libelle { get; set; }
    public string CodeClub { get; set; }
    public string NomClub { get; set; }
    public string CodeComite { get; set; }
    public string CodeLigue { get; set; }
    public DateInfo DateDebut { get; set; }
    public DateInfo DateFin { get; set; }
    public bool PaiementEnLigne { get; set; }
    public bool InscriptionEnLigne { get; set; }
    public string NomEngagement { get; set; }
    public string Adresse1Engagement { get; set; }
    public string Adresse2Engagement { get; set; }
    public string CodePostalEngagement { get; set; }
    public string VilleEngagement { get; set; }
    public string CourrielEngagement { get; set; }
    public Installation Installation { get; set; }
    public List<int> IdsArbitres { get; set; }
    public JugeArbitre JugeArbitre { get; set; }
    public List<object> NaturesTerrains { get; set; }
    public List<Epreuve> Epreuves { get; set; }
    public string DistanceEnMetres { get; set; }
    public bool International { get; set; }
    public string OriginalId { get; set; }
    public List<object> FamilleTournoi { get; set; }
    public bool IsTournoi { get; set; }
    public Dictionary<string, Dictionary<string, string>> NatureWithCatAge { get; set; }
    public string Favoris { get; set; }
}

public class DateInfo
{
    public string Date { get; set; }
    public int Timezone_Type { get; set; }
    public string Timezone { get; set; }
}

public class Installation
{
    public string Nom { get; set; }
    public string Adresse2 { get; set; }
    public string CodePostal { get; set; }
    public string Ville { get; set; }
    public string Telephone { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public class JugeArbitre
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
}

public class Epreuve
{
    public CategorieAge CategorieAge { get; set; }
    public NatureEpreuve NatureEpreuve { get; set; }
    public string Libelle { get; set; }
    public TypeEpreuve TypeEpreuve { get; set; }
}

public class CategorieAge
{
    public int Id { get; set; }
    public string Libelle { get; set; }
}

public class NatureEpreuve
{
    public string Code { get; set; }
    public string Libelle { get; set; }
}

public class TypeEpreuve
{
    public string Code { get; set; }
}

public class AjaxCommand
{
    [JsonProperty("command")]
    public string Command { get; set; }
    
    [JsonProperty("results")]
    public RechercheResults Results { get; set; }
}

public class RechercheResults
{
    [JsonProperty("nb_results")]
    public int NbResults { get; set; }
    
    [JsonProperty("items")]
    public List<Tournoi> Items { get; set; }
}
