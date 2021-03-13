using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace SatelliteSite.XylabModule
{
    public class XylabModule : AbstractModule
    {
        public override string Area => "Xylab";

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();
        }
    }
}
