# TspQuoteDownloader
Download price quotes from the federal Thrift Savings Program and export them into a .csv file that can be read by Quicken

#### Usage Notes
This project uses [Selenium](https://www.selenium.dev) to render and parse the quotes.  The old static quote page was taken down by the TSP.  The new page uses javascript to inject the quotes into the page, so there is no way to scrape the quotes without loading the document in a browser.
