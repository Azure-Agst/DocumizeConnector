// ---------------------------------------------------------------------------
// <copyright file="ConnectorCrawlerServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using Grpc.Core;
using Microsoft.Graph.Connectors.Contracts.Grpc;
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
            Log.Information("GetCrawlStream Entry");

            // Placeholder code to remove compiler errors
            await Task.FromResult(true).ConfigureAwait(true);

            throw new RpcException(
                       new Status(
                           StatusCode.Unimplemented,
                           "'GetCrawlStream' is not implemented."));
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
            Log.Information("GetIncrementalCrawlStream Entry");

            // Placeholder code to remove compiler errors
            await Task.FromResult(true).ConfigureAwait(true);

            throw new RpcException(
                       new Status(
                           StatusCode.Unimplemented,
                           "'GetIncrementalCrawlStream' is not implemented."));
        }
    }
}
