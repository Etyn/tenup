using Marten;
using Marten.Schema;
using Tenup.Repositories.Entities;

namespace Tenup.Repositories
{
    public class TournoiRegistry : MartenRegistry
    {
        public TournoiRegistry()
        {
            For<TournoiDb>()
                .DocumentAlias("Tournoi")
                .Index(x => x.OriginalId);
        }
    }
}