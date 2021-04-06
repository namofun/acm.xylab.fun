using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule;

[assembly: AffiliateTo(
    typeof(Ccs.Connector.OpenXml.OpenXmlConnector),
    typeof(SatelliteSite.ContestModule.ContestModule<>))]

namespace Ccs.Connector.OpenXml
{
    public class OpenXmlConnector : AbstractConnector
    {
        public override string Area => "Contest";

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(CcsDefaults.JuryMenuAdmin, menu =>
            {
                menu.HasEntry(350)
                    .HasTitle(string.Empty, "Import / export")
                    .HasLink("Contest", "JuryExport", "Index")
                    .RequireThat(c => c.HttpContext.Features.Get<IContestFeature>().Kind != CcsDefaults.KindProblemset);
            });
        }
    }
}
