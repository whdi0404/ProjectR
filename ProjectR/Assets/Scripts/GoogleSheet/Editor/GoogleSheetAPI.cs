using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Table
{
	internal class GoogleSheetAPI
	{
		public struct SpreadSheetData
		{
			public string Name { get; private set; }
			public List<(string, IList<IList<object>>)> WorkSheetList { get; private set; }

			public SpreadSheetData(string name, List<(string, IList<IList<object>>)> workSheetList)
			{
				Name = name;
				WorkSheetList = workSheetList;
			}
		}

		// If modifying these scopes, delete your previously saved credentials
		// at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
		private static string CredentialPath = $"{Application.dataPath}{Path.DirectorySeparatorChar}Plugins{Path.DirectorySeparatorChar}GoogleApis{Path.DirectorySeparatorChar}client_secret_753833698309-k34ar8gsqkgv3v3jk4m5mvfpqtm77405.apps.googleusercontent.com.json";

		private static string TokenPath = $"{Application.dataPath}{Path.DirectorySeparatorChar}Plugins{Path.DirectorySeparatorChar}GoogleApis{Path.DirectorySeparatorChar}";
		private static string User = "whdi04044@gmail.com";
		private static string ApplicationName = "ProjectR";

		private static UserCredential credential;

		private static UserCredential Credential
		{
			get
			{
				if (credential == null)
				{
					using (var stream = new FileStream(CredentialPath, FileMode.Open, FileAccess.Read))
					{
						// The file token.json stores the user's access and refresh tokens, and is created
						// automatically when the authorization flow completes for the first time.

						var broker = GoogleWebAuthorizationBroker.AuthorizeAsync(
							GoogleClientSecrets.Load(stream).Secrets,
							new string[] { SheetsService.Scope.SpreadsheetsReadonly },
							User,
							CancellationToken.None, new FileDataStore(TokenPath));

						EditorUtility.DisplayProgressBar("GoogleAPIAuthorize", "progressing..", 0);
						EditorUtility.ClearProgressBar();
						credential = broker.Result;
					}

					return credential;
				}

				return credential;
			}
		}

		private static SheetsService service;

		public static SheetsService Service
		{
			get
			{
				if (service == null)
				{
					service = new SheetsService(new BaseClientService.Initializer()
					{
						HttpClientInitializer = Credential,
						ApplicationName = ApplicationName,
					});

					return service;
				}

				return service;
			}
		}

		public static Spreadsheet GetSpreadSheet(string spreadSheetId)
		{
			SpreadsheetsResource.GetRequest request = Service.Spreadsheets.Get(spreadSheetId);
			Spreadsheet response = request.Execute();

			return response;
		}

		public static IList<IList<object>> GetSpreadSheetData(string spreadSheetId, string spreadSheetName)
		{
			string range = $"{spreadSheetName}!1:10000";
			SpreadsheetsResource.ValuesResource.GetRequest request = Service.Spreadsheets.Values.Get(spreadSheetId, range);
			ValueRange response = request.Execute();
			return response.Values;
		}

		public static SpreadSheetData GetSpreadSheetData(string spreadSheetId)
		{
			Spreadsheet spreadSheet = GetSpreadSheet(spreadSheetId);

			string name = spreadSheet.Properties.Title;
			List<(string, IList<IList<object>>)> workSheetList = new List<(string, IList<IList<object>>)>();

			foreach (var sheet in spreadSheet.Sheets)
			{
				string workSheetName = sheet.Properties.Title;
				var workSheetData = GetSpreadSheetData(spreadSheetId, workSheetName);

				workSheetList.Add((workSheetName, workSheetData));
			}

			return new SpreadSheetData(name, workSheetList);
		}
	}
}