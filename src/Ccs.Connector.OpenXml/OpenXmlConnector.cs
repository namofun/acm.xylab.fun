using Microsoft.AspNetCore.Mvc;

[assembly: AffiliateTo(
    typeof(Ccs.Connector.OpenXml.OpenXmlConnector),
    typeof(SatelliteSite.ContestModule.ContestModule<>))]

namespace Ccs.Connector.OpenXml
{
    public class OpenXmlConnector : AbstractConnector
    {
        public override string Area => "Contest";
    }
}
