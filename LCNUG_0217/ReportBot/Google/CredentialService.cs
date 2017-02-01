using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.AnalyticsReporting.v4;

namespace Google.Analytics.Services
{
    public class CredentialService
    {
        public GoogleCredential GetGoogleCredential()
        {
            using (var stream = new FileStream(@"c: \Google\secret.json", FileMode.Open, FileAccess.Read))
                return GoogleCredential.FromStream(stream).CreateScoped(AnalyticsReportingService.Scope.AnalyticsReadonly);
        }
        
    }
}
