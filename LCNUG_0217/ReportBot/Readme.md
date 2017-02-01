1.  Use the LuisReportModel.json file as training data for your natural language interpreter
2.  Use your Account and Subscription ID when you decorate the RootLuisDialog class
 [LuisModel("HERE", "AND HERE TOO")] 
 3.  Hook up your Google Analytics account to the Google service provided.  See https://github.com/google/google-api-dotnet-client 
 4.  Put the Google account secret here:  c: \Google\secret.json.  Otherwise, update the CredentialService.cs class to point to another location.