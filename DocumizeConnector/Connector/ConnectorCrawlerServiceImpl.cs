// ---------------------------------------------------------------------------
// <copyright file="ConnectorCrawlerServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using DocumizeConnector.Data;
using DocumizeConnector.Models;
using Grpc.Core;
using Microsoft.Graph.Connectors.Contracts.Grpc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Graph.Connectors.Contracts.Grpc.ConnectorCrawlerService;

namespace DocumizeConnector.Connector
{
    /// <summary>
    /// Class to implement crawl APIs needed to read data from datasource and pass it onto Graph connector platform
    /// </summary>
    public class ConnectorCrawlerServiceImpl : ConnectorCrawlerServiceBase
    {
        /// <summary>
        /// API to crawl datasource
        /// Expectation is to crawl datasource from the checkpoint provided and send the crawlItem
        /// Keep updating checkpoint info with every crawlItem so that Graph connector platform can try to resume crawl in-case of a crash or failure
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request containing all needed info to connect to datasource</param>
        /// <param name="responseStream">response as a stream. Keep sending crawl item in stream</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Close stream and end function to indicate success and build appropriate OperationStatus object in case of an exception or failure.</returns>
        public override async Task GetCrawlStream(GetCrawlStreamRequest request, IServerStreamWriter<CrawlStreamBit> responseStream, ServerCallContext context)
        {
            
            try
            {
                Log.Information("GetCrawlStream Entry");

                var crawlItems = new List<CrawlItem>();
                bool itemsRemaining = true;

                while (itemsRemaining)
                {
                    var dataLoader = new DataLoader();
                    var customParams = JsonConvert.DeserializeObject<CustomParams>(request.CustomConfiguration.Configuration);
                    (crawlItems, itemsRemaining) = await dataLoader.ExecuteFullCrawl(request.AuthenticationData, customParams);
                    IEnumerator<CrawlItem> ciEnumerator = crawlItems.GetEnumerator();
                    while (ciEnumerator.MoveNext())
                    {
                        CrawlStreamBit csBit = this.GetCrawlStreamBit(ciEnumerator.Current);
                        await responseStream.WriteAsync(csBit).ConfigureAwait(false);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                CrawlStreamBit csBit = new CrawlStreamBit
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.DatasourceError,
                        StatusMessage = "Fetching items from datasource failed",
                        RetryInfo = new RetryDetails
                        {
                            Type = RetryDetails.Types.RetryType.Standard
                        }
                    }
                };
                await responseStream.WriteAsync(csBit).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// API to crawl datasource from the point where last incremental crawl ended
        /// Expectation is to crawl datasource from the checkpoint provided and send the items which are added/modified or deleted since the last incremental crawl.
        /// Keep updating checkpoint info with every crawlItem so that Graph connector platform will send this checkpoint for the next incremental crawl.
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request containing all needed info to connect to datasource</param>
        /// <param name="responseStream">response as a stream. Keep sending crawl item in stream</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Close stream and end function to indicate success and build appropriate OperationStatus object in case of an exception or failure.</returns>
        public override async Task GetIncrementalCrawlStream(GetIncrementalCrawlStreamRequest request, IServerStreamWriter<IncrementalCrawlStreamBit> responseStream, ServerCallContext context)
        {

            try
            {
                Log.Information("GetIncrementalCrawlStream Entry");

                var crawlItems = new List<IncrementalCrawlItem>();
                bool itemsRemaining = true;

                // Also parse the date here :)
                DateTime lastModifiedAt = request.PreviousCrawlStartTimeInUtc.ToDateTime();
                if (DateTime.TryParse(request.CrawlProgressMarker.CustomMarkerData, out DateTime result))
                {
                    lastModifiedAt = result;
                }

                while (itemsRemaining)
                {
                    var dataLoader = new DataLoader();
                    var customParams = JsonConvert.DeserializeObject<CustomParams>(request.CustomConfiguration.Configuration);
                    (crawlItems, itemsRemaining, lastModifiedAt) = await dataLoader.ExecuteIncrementalCrawl(request.AuthenticationData, customParams, lastModifiedAt);
                    IEnumerator<IncrementalCrawlItem> ciEnumerator = crawlItems.GetEnumerator();
                    while (ciEnumerator.MoveNext())
                    {
                        IncrementalCrawlStreamBit csBit = this.GetIncrementalCrawlStreamBit(ciEnumerator.Current);
                        await responseStream.WriteAsync(csBit).ConfigureAwait(false);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                IncrementalCrawlStreamBit csBit = new IncrementalCrawlStreamBit
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.DatasourceError,
                        StatusMessage = "Fetching items from datasource failed",
                        RetryInfo = new RetryDetails
                        {
                            Type = RetryDetails.Types.RetryType.Standard
                        }
                    }
                };
                await responseStream.WriteAsync(csBit).ConfigureAwait(false);
            }
        }

        private CrawlStreamBit GetCrawlStreamBit(CrawlItem crawlItem)
        {
            return new CrawlStreamBit
            {
                Status = new OperationStatus
                {
                    Result = OperationResult.Success,
                },
                CrawlItem = crawlItem,
                CrawlProgressMarker = new CrawlCheckpoint
                {
                    CustomMarkerData = crawlItem.ItemId,
                },
            };
        }

        private IncrementalCrawlStreamBit GetIncrementalCrawlStreamBit(IncrementalCrawlItem crawlItem)
        {
            return new IncrementalCrawlStreamBit
            {
                Status = new OperationStatus
                {
                    Result = OperationResult.Success,
                },
                CrawlItem = crawlItem,
                CrawlProgressMarker = new CrawlCheckpoint
                {
                    CustomMarkerData = crawlItem.ItemId,
                },
            };
        }
    }
}
