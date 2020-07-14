using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Linq;
using System.Globalization;

namespace TspQuoteDownloader
{
	class Program
	{
		static void Main(string[] args)
		{
			TextWriter csvWriter = (args.Length > 0) ? new StreamWriter(args[0]) : Console.Out;
			RemoteWebDriver driver = null;

			try
			{
				var url = "https://www.tsp.gov/fund-performance/share-price-history";

				// suppress diagnostics and warnings
				var service = ChromeDriverService.CreateDefaultService();
				service.SuppressInitialDiagnosticInformation = true;
				service.HideCommandPromptWindow = true;
				var options = new ChromeOptions();
				options.AddArgument("--log-level=3");

				// open Chrome instance to the TSP quote history page
				driver = new ChromeDriver(service, options);
				driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
				driver.Navigate().GoToUrl(url);
				driver.Manage().Window.Maximize();

				// locate the table of quotes
				var quoteTable = driver.FindElement(By.XPath("//table[@class='dynamic-share-price-table']"));

				// locate the column headers
				var headerRow = quoteTable.FindElement(By.XPath("thead/tr[1]"));
				var headers = headerRow.FindElements(By.TagName("th")).Select(n => n.Text).ToArray();

				// locate the quote rows
				var rows = quoteTable.FindElements(By.XPath("tbody/tr")).ToArray();

				for (int iRow = 0; iRow < rows.Length; iRow++)
				{
					// the date node is a th, not a td
					// locate and parse it
					var dateNode = rows[iRow].FindElement(By.TagName("th"));
					if (dateNode == null)
						continue;

					var dateStr = dateNode.Text;
					if (!DateTime.TryParseExact(dateStr, "MMM dd, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
						continue;

					// pull the quotes
					var quotes = rows[iRow].FindElements(By.TagName("td")).Select(n => n.Text).ToArray();

					// Quicken date is mm/dd/yy
					var outputDate = date.ToString("MM/dd/yy");

					for (int i = 0; i < quotes.Length; i++)
					{
						// omit blank quotes
						if (string.IsNullOrWhiteSpace(quotes[i]))
							continue;

						// write quote 
						var ticker = columnHeaderToQuickenTicker(headers[i+1]);
						csvWriter.WriteLine($"TSP {ticker},{outputDate},{quotes[i]}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.ToString());
			}
			finally
			{
				csvWriter.Close();

				if (driver != null)
					driver.Quit();
			}
		}

		static string columnHeaderToQuickenTicker(string columnHeader)
		{
			return columnHeader.Replace(" Fund", string.Empty).Replace("L Income", "L INC");
		}

	}
}
