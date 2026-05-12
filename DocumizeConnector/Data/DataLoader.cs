using DocumizeConnector.Models;
using DocumizeConnector.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Connectors.Contracts.Grpc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace DocumizeConnector.Data
{
    internal class DataLoader
    {
        /// <summary>The HTTP client factory</summary>
        private readonly IHttpClientFactory httpClientFactory;
        private string _tempLabel = "Internal IT";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLoader"/>
        /// </summary>
        public DataLoader()
        {
            ServiceProvider serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            this.httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        }


        // NOTE: This is the only one that breaks from the pattern.
        public async Task<string> GetBearer(AuthenticationData authData)
        {
            Log.Information("Fetching Bearer Token");
            using (var httpClient = this.httpClientFactory.CreateClient())
            {
                var dataSourceURL = authData.DatasourceUrl + "/api/public/authenticate";
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(dataSourceURL));

                var basicCreds = authData.BasicCredential.Username + ":" + authData.BasicCredential.Secret;
                var basicHex = Convert.ToBase64String(Encoding.UTF8.GetBytes(basicCreds));
                request.Headers.Add("Authorization", "Basic " + basicHex);

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var authRes = JsonConvert.DeserializeObject<BearerAuth>(responseString);
                    return authRes.Bearer;
                }
                else
                {
                    throw new HttpRequestException(response.ReasonPhrase, null, statusCode: response.StatusCode);
                }
            }
        }

        public async Task<List<Label>> GetLabels(AuthenticationData authData, string bearer)
        {
            return await getDocumizeData<List<Label>>(authData, bearer, "/api/label");
        }

        public async Task<List<Space>> GetSpaces(AuthenticationData authData, string bearer)
        {
            return await getDocumizeData<List<Space>>(authData, bearer, "/api/space");
        }

        public async Task<List<Document>> GetDocumentsInSpace(AuthenticationData authData, string bearer, Space space)
        {
            var path = "/api/documents?space=" + space.ID;
            return await getDocumizeData<List<Document>>(authData, bearer, path);
        }
        public async Task<string> GetDocumentBody(AuthenticationData authData, string bearer, Document doc)
        {
            var path = "/api/documents/" + doc.ID + "/pages";
            var pages = await getDocumizeData<List<Page>>(authData, bearer, path);

            string fullBody = "";
            foreach (var page in pages) { 
                fullBody += page.Body;
            }

            return fullBody;
        }

        public async Task<T> getDocumizeData<T>(AuthenticationData authData, string bearer, string path)
        {
            Log.Information("Fetching " + path);
            using (var httpClient = this.httpClientFactory.CreateClient())
            {
                var dataSourceURL = authData.DatasourceUrl + path;
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(dataSourceURL));

                request.Headers.Add("Authorization", "Bearer " + bearer);

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(responseString);
                }
                else
                {
                    throw new HttpRequestException(response.ReasonPhrase, null, statusCode: response.StatusCode);
                }
            }
        }


        /*
         * Full/Incremental Crawl
         */

        public async Task<(List<CrawlItem>, bool)> ExecuteFullCrawl(AuthenticationData authData, CustomParams customParams)
        {
            try
            {
                var crawlItems = new List<CrawlItem>();
                bool itemsRemaining = false;

                using (var httpClient = this.httpClientFactory.CreateClient())
                {
                    // Get Bearer
                    var bearer = await GetBearer(authData);

                    // Find label
                    var labels = await GetLabels(authData, bearer);
                    var relLabel = labels.FirstOrDefault(x => x.Name == _tempLabel);
                    if (relLabel == null) throw new Exception("No label for " + _tempLabel + " found!");

                    // Find space
                    var spaces = await GetSpaces(authData, bearer);
                    var relSpaces = spaces.Where(x => x.labelId == relLabel.ID);
                    if (relSpaces == null) return (crawlItems, false);

                    // Iterate through spaces
                    foreach (var space in relSpaces)
                    {
                        // Get all docs in space
                        var spaceDocs = await GetDocumentsInSpace(authData, bearer, space);
                        if (spaceDocs == null) continue; // Edge case: Empty Space
                        foreach (var doc in spaceDocs)
                        {
                            // Get body (w/ images stripped)
                            // NOTE: Not stripping images can sometimes cause pages to go over 4MB upload limit.
                            string body = await GetDocumentBody(authData, bearer, doc);
                            string clean = Regex.Replace(body, @"<img\b[^>]*>", string.Empty, RegexOptions.IgnoreCase);
                            doc.Body = clean;

                            // Calculate URL
                            doc.URL = URL.UrlFromDocument(authData.DatasourceUrl, space, doc);
                            //Log.Information("URL: " + doc.URL);

                            // Convert to CrawlItem
                            crawlItems.Add(doc.ToCrawlItem());
                            itemsRemaining = true; // Note: Not sure this is needed? Maybe this is for pagination?
                        }

                        itemsRemaining = false;
                    }

                    // Return status
                    return (crawlItems, itemsRemaining);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public async Task<(List<IncrementalCrawlItem>, bool, DateTime)> ExecuteIncrementalCrawl(AuthenticationData authData, CustomParams customParams, DateTime lastModifiedAt)
        {
            try
            {
                var crawlItems = new List<IncrementalCrawlItem>();
                bool itemsRemaining = false;

                using (var httpClient = this.httpClientFactory.CreateClient())
                {
                    // Get Bearer
                    var bearer = await GetBearer(authData);

                    // Find label
                    var labels = await GetLabels(authData, bearer);
                    var relLabel = labels.FirstOrDefault(x => x.Name == _tempLabel);
                    if (relLabel == null) throw new Exception("No label for " + _tempLabel + " found!");

                    // Find space
                    var spaces = await GetSpaces(authData, bearer);
                    var relSpaces = spaces.Where(x => x.labelId == relLabel.ID);
                    if (relSpaces == null) return (crawlItems, false, lastModifiedAt);

                    // Iterate through spaces
                    foreach (var space in relSpaces)
                    {
                        // Get all docs in space
                        var spaceDocs = await GetDocumentsInSpace(authData, bearer, space);

                        // Filter out all older documents
                        spaceDocs = spaceDocs.Where(x => x.UpdatedAt > lastModifiedAt).ToList();

                        // Loop
                        foreach (var doc in spaceDocs)
                        {
                            // Get body (w/ images stripped)
                            // NOTE: Not stripping images can sometimes cause pages to go over 4MB upload limit.
                            string body = await GetDocumentBody(authData, bearer, doc);
                            string clean = Regex.Replace(body, @"<img\b[^>]*>", string.Empty, RegexOptions.IgnoreCase);
                            doc.Body = clean;

                            // Calculate URL
                            doc.URL = URL.UrlFromDocument(authData.DatasourceUrl, space, doc);
                            //Log.Information("URL: " + doc.URL);

                            // Convert to CrawlItem
                            crawlItems.Add(doc.ToIncrementalCrawlItem());
                            itemsRemaining = true;
                            lastModifiedAt = doc.UpdatedAt;
                        }
                    }

                    // Return status
                    return (crawlItems, itemsRemaining, lastModifiedAt);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }
    }
}
