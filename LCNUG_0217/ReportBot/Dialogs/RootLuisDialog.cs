
namespace ReportBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using System.Net;
    using System.Threading;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using Google.Apis.AnalyticsReporting.v4.Data;
    using Google.Analytics.Services;

    [Serializable]
    [LuisModel("00500d02-b38a-4ab8-b429-ff1e71678423", "29a36c9bb8f1494e87923f411c4c4832")]
    public class RootLuisDialog : LuisDialog<object>
    {

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {

            // Give them a card with the commands.
            var reply = context.MakeMessage();

            var card = new CardAction();

            reply.Attachments = new List<Attachment>();
       
            List<CardAction> cardButtons = new List<CardAction>();

            cardButtons.Add(new CardAction()
            {
                Value = "users by browser last week",
                Type = "postBack",
                Title = "Users last week"
            });
            cardButtons.Add(new CardAction()
            {
                Value = "sessions last week",
                Type = "postBack",
                Title = "How many sessions did I have yesterday?"
            });


            HeroCard plCard = new HeroCard()
            {
                Title = "Here are some examples",
                Buttons = cardButtons
            };
            
             Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            await context.PostAsync(reply);

            context.Wait(this.MessageReceived);


        }

        [LuisIntent("GAReport")]
        public async Task Reports(IDialogContext context, LuisResult result)
        {
            var reportType = "";
            var reportFilter = "";
            var reportDate = "";
           
            foreach (var v in result.Entities)
            {
                if (v.Type == "ReportType")
                    reportType = v.Entity;

                if (v.Type == "ReportGrouping")
                    reportFilter = v.Entity;

                if (v.Type == "builtin.datetime.date")
                {
                    reportDate = v.Resolution.Values.First();
                }
            }
            

            IList<string> startDates = new List<string>();
            IList<string> endDates = new List<string>();
            IList<string> metrics = new List<string>();
            IList<string> dimensions = new List<string>();
            
            // Parse Date:  Get Year, and week.
            Regex r = new Regex(@"(^\d{4})-W(\d{1,2})");
            var match = r.Match(reportDate);
            if (match.Success)
            {
                var dt = GetDateFromWeekNumberAndDayOfWeek(Convert.ToInt32(match.Groups[2].Value), 0);
                reportDate = dt.ToString("yyyy-MM-dd");
            }
            
            // Format Request 
            startDates.Add(setDate(reportDate));
            endDates.Add(setDate("today"));
            if (reportFilter.Length > 0)
              dimensions.Add(SetDimension(reportFilter));
            metrics.Add(SetMetric(reportType));

            // Call the service
            var service = new AnalyticsService(startDates, endDates, dimensions,metrics, "130610833");
            var reportResult = await service.GetReport();
           
            // Save last report!
            context.UserData.SetValue<GetReportsResponse>("LastReport", reportResult);
            context.UserData.SetValue<string>("ReportFilter", reportFilter);
            context.UserData.SetValue<string>("ReportDate", reportDate);
            context.UserData.SetValue<string>("ReportType", reportType);

            // Report dimension?  If so, show as a pie chart.
            if (reportFilter.Length > 0)
            {
                var analyticsResult = await service.CreateSeries();
           
                var labels = "";
                var series = "";
                for (int i = 0; i < analyticsResult.Item1.Count; i++)
                {
                    labels += analyticsResult.Item1[i] + ",";
                    series += analyticsResult.Item2[i] + ",";
                }

                await CreateDynamicChart(context, labels, series);
            }
            else
                await context.PostAsync(reportResult.Reports[0].Data.Rows[0].Metrics[0].Values[0]);
            

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("ChangeReportView")]
        public async Task ChangeReport(IDialogContext context, LuisResult result)
        {
            // Save last report!
            var reportData = context.UserData.Get<GetReportsResponse>("LastReport");
            var reportFilter = context.UserData.Get<string>("ReportFilter");
            var reportDate = context.UserData.Get<string>("ReportDate");
            var reportType = context.UserData.Get<string>("ReportType");

            await context.PostAsync($"I know your last report was {reportFilter} {reportDate} {reportType}");

            context.Wait(this.MessageReceived);
        }
      
        private async Task CreateDynamicChart(IDialogContext context, string labels,string series)
        {

            var reply = context.MakeMessage();

            var card = new CardAction();

            reply.Attachments = new List<Attachment>();
            List<CardImage> cardImages = new List<CardImage>();

            labels = labels.Remove(labels.Length - 1, 1);
            series = series.Remove(series.Length - 1, 1);
            using (var v = new WebClient())
                cardImages.Add(new CardImage(url: v.DownloadString($"http://nodecharts.azurewebsites.net/app.js?labels={labels}&series={series}")));

          
            HeroCard plCard = new HeroCard()
            {
                Title = "Results",
                Images = cardImages
            };

            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            await context.PostAsync(reply);

        }
        
        private string setDate(string dateString)
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(dateString.ToLower()).Replace(" ", "");
        }

        private string SetDimension(string dimension)
        {

            if (NLPDimensions.city == dimension)
                return "ga:city";
            else if (NLPDimensions.country == dimension)
                return "ga:country";
            else if (NLPDimensions.pagePath == dimension)
                return "ga:pagePath";
            else if (NLPDimensions.pageTitle == dimension)
                return "ga:pageTitle";
            else if (NLPDimensions.sessionCount == dimension)
                return "ga:sessionCount";
            else if (NLPDimensions.userType == dimension)
                return "ga:userType";
            else
                return "ga:browser";

        }

        private string SetMetric(string metric)
        {
            if (NLPMetrics.avgTimeOnPage == metric)
                return "ga:avgTimeOnPage";
            else if (NLPMetrics.newUsers == metric)
                return "ga:newUsers";
            else if (NLPMetrics.pageValue == metric)
                return "ga:pageValue";
            else if (NLPMetrics.sessions == metric)
                return "ga:sessions";
            else if (NLPMetrics.sessionsPerUser == metric)
                return "ga:sessionsPerUser";
            else
                return "ga:users";
        }

        private DateTime GetDateFromWeekNumberAndDayOfWeek(int weekNumber, int dayOfWeek)
        {
            weekNumber = weekNumber - 1;
            DateTime jan1 = new DateTime(DateTime.Now.Year, 1, 1);
            int daysOffset = DayOfWeek.Tuesday - jan1.DayOfWeek;

            DateTime firstMonday = jan1.AddDays(daysOffset);

            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekNumber;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            var result = firstMonday.AddDays(weekNum * 7 + dayOfWeek - 1);
            return result;
        }
        
    }
}
