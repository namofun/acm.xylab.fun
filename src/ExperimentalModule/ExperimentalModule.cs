using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace SatelliteSite.ExperimentalModule
{
    public class ExperimentalModule : AbstractModule
    {
        public override string Area => "Xylab";

        public override void Initialize()
        {
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();
        }
    }
}
