using eu.driver.model.edxl;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldExplorerClient
{
    public static class EDXLDistributionExtension
    {
        public static EDXLDistribution CreateHeader(string pSenderId = "")
        {
            return new EDXLDistribution()
            {
                senderID = pSenderId,
                distributionID = Guid.NewGuid().ToString(),
                distributionKind = DistributionKind.Unknown,
                distributionStatus = DistributionStatus.Unknown,
                dateTimeSent = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
                dateTimeExpires = (long)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)) + new TimeSpan(0, 0, 10, 0, 0)).TotalMilliseconds,
            };
        }

    }
}
