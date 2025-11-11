using NodaTime;
using System;
using System.Collections.Generic;

namespace Tenup.Repositories.Entities
{
    public class TournoiDb
    {
        public Guid Id { get; set; }
        public string OriginalId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Millesime { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Libelle { get; set; } = string.Empty;
        public string CodeClub { get; set; } = string.Empty;
        public string NomClub { get; set; } = string.Empty;
        public string CodeComite { get; set; } = string.Empty;
        public string CodeLigue { get; set; } = string.Empty;
        public DateInfo DateDebut { get; set; } = new DateInfo();
        public DateInfo DateFin { get; set; } = new DateInfo();
        public bool PaiementEnLigne { get; set; }
        public bool InscriptionEnLigne { get; set; }
        public string NomEngagement { get; set; } = string.Empty;
        public string Adresse1Engagement { get; set; } = string.Empty;
        public string Adresse2Engagement { get; set; } = string.Empty;
        public string CodePostalEngagement { get; set; } = string.Empty;
        public string VilleEngagement { get; set; } = string.Empty;
        public string CourrielEngagement { get; set; } = string.Empty;
        public Installation Installation { get; set; } = new Installation();
        public List<int> IdsArbitres { get; set; } = new List<int>();
        public JugeArbitre JugeArbitre { get; set; } = new JugeArbitre();
        public List<object> NaturesTerrains { get; set; } = new List<object>();
        public List<Epreuve> Epreuves { get; set; } = new List<Epreuve>();
        public string DistanceEnMetres { get; set; } = string.Empty;
        public bool International { get; set; }
        public List<object> FamilleTournoi { get; set; } = new List<object>();
        public bool IsTournoi { get; set; }
        public Dictionary<string, Dictionary<string, string>> NatureWithCatAge { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        public string Favoris { get; set; } = string.Empty;
        
        // Propriétés d'audit
        public LocalDateTime CreatedDateUtc { get; set; }
        public LocalDateTime? UpdatedDateUtc { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Propriétés de recherche
        public string SearchCity { get; set; } = string.Empty;
        public int SearchDistance { get; set; }
        public LocalDateTime SearchDateUtc { get; set; }
    }
}