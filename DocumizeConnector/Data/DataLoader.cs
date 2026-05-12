using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Connectors.Contracts.Grpc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumizeConnector.Models;
using Serilog;

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

        public async Task<List<Label>> GetLabels(AuthenticationData authData)
        {
            using (var httpClient = this.httpClientFactory.CreateClient())
            {
                var dataSourceURL = authData.DatasourceUrl + "/api/labels";
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(dataSourceURL));
                var response = await httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var labelList = JsonConvert.DeserializeObject<List<Label>>(responseString);
                    return labelList;
                }
                else
                {
                    throw new HttpRequestException(response.ReasonPhrase, null, statusCode: response.StatusCode);
                }
            }
        }

        public async Task<List<Space>> GetSpaces(AuthenticationData authData)
        {
            using (var httpClient = this.httpClientFactory.CreateClient())
            {
                var dataSourceURL = authData.DatasourceUrl + "/api/space";
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(dataSourceURL));
                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var spaceList = JsonConvert.DeserializeObject<List<Space>>(responseString);
                    return spaceList;
                }
                else
                {
                    throw new HttpRequestException(response.ReasonPhrase, null, statusCode: response.StatusCode);
                }
            }
        }

        public async Task<List<Document>> GetDocumentsInSpace(AuthenticationData authData, Space space)
        {
            using (var httpClient = this.httpClientFactory.CreateClient())
            {
                var dataSourceURL = authData.DatasourceUrl + "/api/documents?space=" + space.ID;
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(dataSourceURL));
                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var docList = JsonConvert.DeserializeObject<List<Document>>(responseString);
                    return docList;
                }
                else
                {
                    throw new HttpRequestException(response.ReasonPhrase, null, statusCode: response.StatusCode);
                }
            }
        }

        public async Task<(List<CrawlItem>, bool)> ExecuteFullCrawl(AuthenticationData authData)
        {
            try
            {
                var crawlItems = new List<CrawlItem>();
                bool itemsRemaining = false;

                using (var httpClient = this.httpClientFactory.CreateClient())
                {
                    // Find label
                    var labels = await GetLabels(authData);
                    var relLabel = labels.FirstOrDefault(x => x.Name == _tempLabel);
                    if (relLabel == null) throw new Exception("No label for " + _tempLabel + " found!");

                    // Find space
                    var spaces = await GetSpaces(authData);
                    var relSpaces = spaces.Where(x => x.labelId == relLabel.ID);
                    if (relSpaces == null) return (crawlItems, false);

                    // Iterate through spaces
                    foreach (var space in relSpaces)
                    {
                        // Get all docs in space
                        var spaceDocs = await GetDocumentsInSpace(authData, space);
                        foreach (var doc in spaceDocs)
                        {
                            // Append to CrawlItem
                            crawlItems.Add(doc.ToCrawlItem());
                            itemsRemaining = true;
                        }
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

        public async Task<(List<IncrementalCrawlItem>, bool, DateTime)> ExecuteIncrementalCrawl(AuthenticationData authData, DateTime lastModifiedAt)
        {
            try
            {
                var crawlItems = new List<IncrementalCrawlItem>();
                bool itemsRemaining = false;

                using (var httpClient = this.httpClientFactory.CreateClient())
                {
                    // Find label
                    var labels = await GetLabels(authData);
                    var relLabel = labels.FirstOrDefault(x => x.Name == _tempLabel);
                    if (relLabel == null) throw new Exception("No label for " + _tempLabel + " found!");

                    // Find space
                    var spaces = await GetSpaces(authData);
                    var relSpaces = spaces.Where(x => x.labelId == relLabel.ID);
                    if (relSpaces == null) return (crawlItems, false, lastModifiedAt);

                    // Iterate through spaces
                    foreach (var space in relSpaces)
                    {
                        // Get all docs in space
                        var spaceDocs = await GetDocumentsInSpace(authData, space);
                        foreach (var doc in spaceDocs)
                        {
                            // Append to CrawlItem
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
