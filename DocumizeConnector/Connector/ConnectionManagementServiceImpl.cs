// ---------------------------------------------------------------------------
// <copyright file="ConnectionManagementServiceImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using DocumizeConnector.Models;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Connectors.Contracts.Grpc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Microsoft.Graph.Connectors.Contracts.Grpc.ConnectionManagementService;

namespace DocumizeConnector.Connector
{
    /// <summary>
    /// Implements connection management APIs
    /// </summary>
    public class ConnectionManagementServiceImpl : ConnectionManagementServiceBase
    {
        /// <summary>
        /// Validates if the credentials provided during connection creation are valid and allow us to access the specified datasource.
        /// This is the first API called during connection creation process.
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request containing all the authentication information</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Response with validation result</returns>
        public override Task<ValidateAuthenticationResponse> ValidateAuthentication(ValidateAuthenticationRequest _request, ServerCallContext context)
        {
            Log.Information("Validating Authentication");

            try
            {
                ServiceProvider serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
                using (var httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient())
                {
                    var authData = _request.AuthenticationData;
                    var dataSourceURL = authData.DatasourceUrl + "/api/public/authenticate";
                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(dataSourceURL));

                    var basicCreds = authData.BasicCredential.Username + ":" + authData.BasicCredential.Secret;
                    var basicHex = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(basicCreds));
                    request.Headers.Add("Authorization", "Basic " + basicHex);

                    var response = httpClient.Send(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return this.BuildAuthValidationResponse(true, "Not Implemented");
                    }
                    else
                    {
                        return this.BuildAuthValidationResponse(false, "Bad Authentication");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while valicating auth: " + ex.Message.ToString());
                return this.BuildAuthValidationResponse(false, "Exception Thrown!");
            }
        }

        private Task<ValidateAuthenticationResponse> BuildAuthValidationResponse(bool accessSuccess, string errorMessageOnFailure = "")
        {
            Log.Information($"Building Authentication validation response for {accessSuccess} with message: {errorMessageOnFailure}");
            OperationStatus validationStatus = null;
            if (accessSuccess)
            {
                validationStatus = new OperationStatus()
                {
                    Result = OperationResult.Success,
                };
            }
            else
            {
                validationStatus = new OperationStatus()
                {
                    Result = OperationResult.AuthenticationIssue,
                    StatusMessage = errorMessageOnFailure,
                };
            }

            ValidateAuthenticationResponse response = new ValidateAuthenticationResponse()
            {
                Status = validationStatus,
            };

            return Task.FromResult(response);
        }

        /// <summary>
        /// Validates if the custom configuration provided during connection creation is valid and datasource can be accessed based on the configuration provided.
        /// This will be called after ValidateAuthentication from Graph connectors platform
        /// The format and structure of the configuration is decoded by the developer of connector and validation done here should be based on those definitions.
        /// This is an optional step in connection creation and can be ignored (return success) if there is no specific configuration needed to access datasource.
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request with all required information</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>Validation status</returns>
        public override Task<ValidateCustomConfigurationResponse> ValidateCustomConfiguration(ValidateCustomConfigurationRequest request, ServerCallContext context)
        {
            Log.Information("Validating custom configuration");
            ValidateCustomConfigurationResponse response;

            try
            {
                if (!string.IsNullOrWhiteSpace(request.CustomConfiguration.Configuration))
                {
                    var _ = JsonConvert.DeserializeObject<CustomParams>(request.CustomConfiguration.Configuration);
                }

                response = new ValidateCustomConfigurationResponse
                {
                    Status = new OperationStatus
                    {
                        Result = OperationResult.Success,
                    }
                };
            }
            catch (Exception ex) 
            {
                Log.Error(ex, "Error while validating custom configuration");
                response = new ValidateCustomConfigurationResponse()
                {
                    Status = new OperationStatus()
                    {
                        Result = OperationResult.ValidationFailure,
                        StatusMessage = ex.Message
                    },
                };
                return Task.FromResult(response);
            }

            return Task.FromResult(response);
        }

        /// <summary>
        /// Returns schema of item retrieved from data source. Schema defines properties available to be read from datasource for individual entities.
        /// Ex: A file can have attributes like Filename, Extension, FileSize, ModifiedDate etc...
        /// API is expected to return list of all properties available for datasource entities
        /// This is the third API called by Graph connectors service. Called after ValidateCustomConfiguration
        /// Use proper Exception Handling mechanism to catch and log exceptions and build appropriate OperationStatus object in case of an exception or failure.
        /// </summary>
        /// <param name="request">Request will all info to connect to datasource</param>
        /// <param name="context">Grpc caller context</param>
        /// <returns>List of properties available for datasource entities</returns>
        public override Task<GetDataSourceSchemaResponse> GetDataSourceSchema(GetDataSourceSchemaRequest request, ServerCallContext context)
        {
            Log.Information("Trying to fetch datasource schema");
            GetDataSourceSchemaResponse response = new GetDataSourceSchemaResponse()
            {
                DataSourceSchema = Document.GetSchema(),
                Status = new OperationStatus()
                {
                    Result = OperationResult.Success,
                },
            };

            return Task.FromResult(response);
        }
    }
}
