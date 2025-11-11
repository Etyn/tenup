using NodaTime;
using Tenup.Repositories.Entities;

namespace Tenup.Mappers
{
    public static class TournoiMapper
    {
        public static TournoiDb ToDb(this Tournoi tournoi, string searchCity = "", int searchDistance = 0)
        {
            return new TournoiDb
            {
                OriginalId = tournoi.OriginalId ?? string.Empty,
                Type = tournoi.Type ?? string.Empty,
                Millesime = tournoi.Millesime,
                Code = tournoi.Code ?? string.Empty,
                Libelle = tournoi.Libelle ?? string.Empty,
                CodeClub = tournoi.CodeClub ?? string.Empty,
                NomClub = tournoi.NomClub ?? string.Empty,
                CodeComite = tournoi.CodeComite ?? string.Empty,
                CodeLigue = tournoi.CodeLigue ?? string.Empty,
                DateDebut = tournoi.DateDebut ?? new DateInfo(),
                DateFin = tournoi.DateFin ?? new DateInfo(),
                PaiementEnLigne = tournoi.PaiementEnLigne,
                InscriptionEnLigne = tournoi.InscriptionEnLigne,
                NomEngagement = tournoi.NomEngagement ?? string.Empty,
                Adresse1Engagement = tournoi.Adresse1Engagement ?? string.Empty,
                Adresse2Engagement = tournoi.Adresse2Engagement ?? string.Empty,
                CodePostalEngagement = tournoi.CodePostalEngagement ?? string.Empty,
                VilleEngagement = tournoi.VilleEngagement ?? string.Empty,
                CourrielEngagement = tournoi.CourrielEngagement ?? string.Empty,
                Installation = tournoi.Installation ?? new Installation(),
                IdsArbitres = tournoi.IdsArbitres ?? new List<int>(),
                JugeArbitre = tournoi.JugeArbitre ?? new JugeArbitre(),
                NaturesTerrains = tournoi.NaturesTerrains ?? new List<object>(),
                Epreuves = tournoi.Epreuves ?? new List<Epreuve>(),
                DistanceEnMetres = tournoi.DistanceEnMetres ?? string.Empty,
                International = tournoi.International,
                FamilleTournoi = tournoi.FamilleTournoi ?? new List<object>(),
                IsTournoi = tournoi.IsTournoi,
                NatureWithCatAge = tournoi.NatureWithCatAge ?? new Dictionary<string, Dictionary<string, string>>(),
                Favoris = tournoi.Favoris ?? string.Empty,
                SearchCity = searchCity,
                SearchDistance = searchDistance,
                IsActive = true
            };
        }

        public static Tournoi FromDb(this TournoiDb tournoiDb)
        {
            return new Tournoi
            {
                Id = tournoiDb.OriginalId,
                OriginalId = tournoiDb.OriginalId,
                Type = tournoiDb.Type,
                Millesime = tournoiDb.Millesime,
                Code = tournoiDb.Code,
                Libelle = tournoiDb.Libelle,
                CodeClub = tournoiDb.CodeClub,
                NomClub = tournoiDb.NomClub,
                CodeComite = tournoiDb.CodeComite,
                CodeLigue = tournoiDb.CodeLigue,
                DateDebut = tournoiDb.DateDebut,
                DateFin = tournoiDb.DateFin,
                PaiementEnLigne = tournoiDb.PaiementEnLigne,
                InscriptionEnLigne = tournoiDb.InscriptionEnLigne,
                NomEngagement = tournoiDb.NomEngagement,
                Adresse1Engagement = tournoiDb.Adresse1Engagement,
                Adresse2Engagement = tournoiDb.Adresse2Engagement,
                CodePostalEngagement = tournoiDb.CodePostalEngagement,
                VilleEngagement = tournoiDb.VilleEngagement,
                CourrielEngagement = tournoiDb.CourrielEngagement,
                Installation = tournoiDb.Installation,
                IdsArbitres = tournoiDb.IdsArbitres,
                JugeArbitre = tournoiDb.JugeArbitre,
                NaturesTerrains = tournoiDb.NaturesTerrains,
                Epreuves = tournoiDb.Epreuves,
                DistanceEnMetres = tournoiDb.DistanceEnMetres,
                International = tournoiDb.International,
                FamilleTournoi = tournoiDb.FamilleTournoi,
                IsTournoi = tournoiDb.IsTournoi,
                NatureWithCatAge = tournoiDb.NatureWithCatAge,
                Favoris = tournoiDb.Favoris
            };
        }

        public static List<TournoiDb> ToDbList(this List<Tournoi> tournois, string searchCity = "", int searchDistance = 0)
        {
            return tournois.Select(t => t.ToDb(searchCity, searchDistance)).ToList();
        }

        public static List<Tournoi> FromDbList(this List<TournoiDb> tournoiDbs)
        {
            return tournoiDbs.Select(t => t.FromDb()).ToList();
        }
    }
}