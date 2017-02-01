using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Google.Analytics.Services
{
    public class AnalyticsService
    {
        private IList<string> startDates;

        private IList<string> endDates;

        private IList<string> dimensions;

        private IList<string> metrics;

        private string viewID;

        public AnalyticsService(IList<string> StartDates, IList<string> EndDates, IList<string> Dimensions, IList<string> Metrics, string ViewID)
        {
            startDates = StartDates;
            endDates = EndDates;
            dimensions = Dimensions;
            metrics = Metrics;
            viewID = ViewID;
        }

        public async Task<GetReportsResponse> GetReport()
        {
            var requests = new List<ReportRequest>();
            var googleRanges = new List<DateRange>();
            var googleMetrics = new List<Metric>();
            var googleDimensions = new List<Dimension>();
            var credService = new CredentialService();

            var client = new AnalyticsReportingService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credService.GetGoogleCredential(),
                ApplicationName = "Reporting Test"
            });

            foreach (var met in metrics)
                googleMetrics.Add(new Metric() { Expression = met });

            foreach (var start in startDates)
                googleRanges.Add(new DateRange() { StartDate = start });

            for (var i = 0; i < googleRanges.Count(); i++)
                googleRanges[i].EndDate = endDates[i];

            foreach (var dim in dimensions)
                googleDimensions.Add(new Dimension() { Name = dim });

            requests.Add(new ReportRequest()
            {
                DateRanges = googleRanges,
                ViewId = viewID,
                Metrics = googleMetrics,
                Dimensions = googleDimensions
            });

            var result = client.Reports.BatchGet(new GetReportsRequest()
            {
                ReportRequests = requests
            });

            return await result.ExecuteAsync();

        }

        public async Task<Tuple<IList<string>, IList<string>>> CreateSeries()
        {

            IList<string> Groups = new List<string>();
            IList<string> Counts = new List<string>();

            // var result = await ExecuteReport();
            var result = await GetReport();

            result.Reports.ToList().ForEach(rpt => rpt.Data.Rows.ToList()
                                                       .ForEach(dim => dim.Dimensions.ToList()
                                                           .ForEach(d => Groups.Add(d))
                                                       )
                                            );


            result.Reports.ToList().ForEach(rpt => rpt.Data.Rows.ToList()
                                                     .ForEach(met => met.Metrics.ToList()
                                                        .ForEach(dr => dr.Values.ToList()
                                                          .ForEach(d => Counts.Add(d))
                                                        )
                                                    )
                                             );

            return Tuple.Create<IList<string>, IList<string>>(Groups, Counts);

        }

    }

    public static class NLPDimensions
    {
        public static string userType { get { return "user type"; } }

        public static string sessionCount { get { return "session count"; } }

        public static string city { get { return "city"; } }

        public static string country { get { return "country"; } }

        public static string pageTitle { get { return "page title"; } }

        public static string pagePath { get { return "page path"; } }

        public static string browser { get { return "browser"; } }

    }

    public static class NLPMetrics
    {
        public const string users = "users";
        public const string newUsers = "new users";
        public const string sessions = "sessions";
        public const string sessionsPerUser = "sessions per user";
        public const string pageValue = "page value";
        public const string avgTimeOnPage = "average time on page";

    }
}
