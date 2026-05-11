// ---------------------------------------------------------------------------
// <copyright file="Readme.md" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

This is a sample project that provides an example for creating a custom connector that can run on Microsoft Graph Connector Agent (GCA). The project builds on the contracts provided by Microsoft to interact with GCA.

The project includes
    1. A Connector server that listens to queries from GCA
    2. Contracts defined by platform for interoperability
    3. Stubs for all the APIs defined in the contract

Project is built as a .net core based console application. Once configured correctly, GCA will be able to make calls into this application through GRPC protocol. The primary responsibility of the connector code is to help GCA during connection creation and later during crawling to fetch information from the datasource. Ensure the process is always running.

Execution flow:
    1. First step is connection creation flow. This happens on Microsoft Admin portal where Search admin goes through a series of steps to configure a connection. Many of these steps end up making calls into the connector code in the following order.
        a. ConnectionManagementServiceImpl.ValidateAuthentication
        b. ConnectionManagementServiceImpl.ValidateCustomConfiguration
        c. ConnectionManagementServiceImpl.GetDataSourceSchema
    2. Once the connection is created successfully, GCA will start calling crawler API to crawl the datasource and return the items. ConnectorCrawlerServiceImpl.GetCrawlStream would be called to read data from data source during full crawl and GetIncrementalCrawlStream will be called during incremental crawls.
    3. In case OAuth is selected as the authentication type, ConnectorOAuthServiceImpl.RefreshAccessToken would be called to refresh the Access token.

    Tutorial: https://learn.microsoft.com/en-us/graph/custom-connector-sdk-sample-create
    How to test code: https://learn.microsoft.com/en-us/graph/custom-connector-sdk-sample-test
 