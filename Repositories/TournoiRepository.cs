using Marten;
using Marten.Exceptions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using NodaTime;
using Tenup.Repositories.Entities;

namespace Tenup.Repositories
{
    public class TournoiRepository : BaseRepository
    {
        public TournoiRepository(IDocumentStore store, TelemetryClient telemetryClient)
            : base(store, telemetryClient)
        {
        }

        public async Task<Guid> AddTournoiAsync(TournoiDb tournoi)
        {
            using var operation = _telemetryClient.StartOperation<DependencyTelemetry>($"{nameof(TournoiRepository)}:{nameof(AddTournoiAsync)}");

            using var session = _store.LightweightSession();

            tournoi.Id = Guid.NewGuid();
            tournoi.CreatedDateUtc = LocalDateTime.FromDateTime(DateTime.UtcNow);
            tournoi.SearchDateUtc = LocalDateTime.FromDateTime(DateTime.UtcNow);

            try
            {
                session.Insert(tournoi);
                await session.SaveChangesAsync();

                LogOperation("TournoiAdded", new Dictionary<string, string>
                {
                    { "TournoiId", tournoi.Id.ToString() },
                    { "OriginalId", tournoi.OriginalId },
                    { "Libelle", tournoi.Libelle }
                });

                return tournoi.Id;
            }
            catch (DocumentAlreadyExistsException ex)
            {
                LogOperation("TournoiAddFailed", new Dictionary<string, string>
                {
                    { "Error", "DocumentAlreadyExists" },
                    { "OriginalId", tournoi.OriginalId }
                });
                throw new InvalidOperationException($"Tournoi with OriginalId {tournoi.OriginalId} already exists", ex);
            }
        }

        public async Task<List<Guid>> AddTournoisAsync(List<TournoiDb> tournois)
        {
            using var operation = _telemetryClient.StartOperation<DependencyTelemetry>($"{nameof(TournoiRepository)}:{nameof(AddTournoisAsync)}");

            using var session = _store.LightweightSession();

            var ids = new List<Guid>();
            var currentTime = LocalDateTime.FromDateTime(DateTime.UtcNow);

            foreach (var tournoi in tournois)
            {
                // Vérifier si le tournoi existe déjà par OriginalId
                var existingTournoi = await session.Query<TournoiDb>()
                    .Where(t => t.OriginalId == tournoi.OriginalId)
                    .FirstOrDefaultAsync();

                if (existingTournoi == null)
                {
                    tournoi.Id = Guid.NewGuid();
                    tournoi.CreatedDateUtc = currentTime;
                    tournoi.SearchDateUtc = currentTime;
                    session.Insert(tournoi);
                    ids.Add(tournoi.Id);
                }
            }

            await session.SaveChangesAsync();

            LogOperation("TournoisBatchAdded", new Dictionary<string, string>
            {
                { "Count", tournois.Count.ToString() },
                { "NewCount", ids.Count.ToString() }
            });

            return ids;
        }

        public async Task<TournoiDb?> GetByIdAsync(Guid id)
        {
            using var session = _store.LightweightSession();

            return await session.Query<TournoiDb>()
                .Where(t => t.Id == id && t.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<TournoiDb?> GetByOriginalIdAsync(string originalId)
        {
            using var session = _store.LightweightSession();

            return await session.Query<TournoiDb>()
                .Where(t => t.OriginalId == originalId && t.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TournoiDb>> GetBySearchCriteriaAsync(string searchCity, int maxDistance = 50)
        {
            using var session = _store.LightweightSession();

            var result = await session.Query<TournoiDb>()
                .Where(t => t.SearchCity == searchCity &&
                           t.SearchDistance <= maxDistance &&
                           t.IsActive)
                .OrderBy(t => t.CreatedDateUtc)
                .ToListAsync();

            return result.ToList();
        }

        public async Task<List<TournoiDb>> GetAllActiveAsync()
        {
            using var session = _store.LightweightSession();

            var result = await session.Query<TournoiDb>()
                .Where(t => t.IsActive)
                .OrderBy(t => t.CreatedDateUtc)
                .ToListAsync();

            return result.ToList();
        }

        public async Task<List<TournoiDb>> GetRecentAsync(int takeCount = 100)
        {
            using var session = _store.LightweightSession();

            var result = await session.Query<TournoiDb>()
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.CreatedDateUtc)
                .Take(takeCount)
                .ToListAsync();

            return result.ToList();
        }

        public async Task<bool> UpdateAsync(Guid id, Action<TournoiDb> updateAction)
        {
            using var session = _store.DirtyTrackedSession();

            var tournoi = await session.Query<TournoiDb>()
                .Where(t => t.Id == id && t.IsActive)
                .FirstOrDefaultAsync();

            if (tournoi == null)
                return false;

            updateAction(tournoi);
            tournoi.UpdatedDateUtc = LocalDateTime.FromDateTime(DateTime.UtcNow);

            await session.SaveChangesAsync();

            LogOperation("TournoiUpdated", new Dictionary<string, string>
            {
                { "TournoiId", id.ToString() }
            });

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await UpdateAsync(id, tournoi => tournoi.IsActive = false);
        }

        public async Task<int> CountAsync()
        {
            using var session = _store.LightweightSession();

            return await session.Query<TournoiDb>()
                .Where(t => t.IsActive)
                .CountAsync();
        }
    }
}