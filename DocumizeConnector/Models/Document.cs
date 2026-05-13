using Google.Protobuf.WellKnownTypes;
using Microsoft.Graph.Connectors.Contracts.Grpc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.Graph.Connectors.Contracts.Grpc.SourcePropertyDefinition.Types;

namespace DocumizeConnector.Models
{
    internal class Document
    {
        [Key]
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Populate, Required = Required.Always)]
        public string ID { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty("excerpt", Required = Required.Always)]
        public string Description { get; set; }

        // NOTE: These two are populated manually, so it's not required
        [JsonIgnore]
        public string Body { get; set; }

        [JsonIgnore]
        public string URL { get; set; }
        // End manual fields

        [JsonProperty("tags", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Tags { get; set; }

        [JsonProperty("created", Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        [DefaultValue(null)]
        [JsonProperty("revised", DefaultValueHandling = DefaultValueHandling.Populate)]
        public DateTime UpdatedAt { get; set; }


        public static DataSourceSchema GetSchema()
        {
            DataSourceSchema schema = new DataSourceSchema();

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(ID),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)SearchAnnotations.IsRetrievable,
                    RequiredSearchAnnotations = (uint)SearchAnnotations.IsRetrievable,
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Title),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Description),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Body),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)SearchAnnotations.IsSearchable,
                    RequiredSearchAnnotations = (uint)SearchAnnotations.IsSearchable,
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(URL),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)SearchAnnotations.IsRetrievable,
                    RequiredSearchAnnotations = (uint)SearchAnnotations.IsRetrievable,
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Tags),
                    Type = SourcePropertyType.String,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsSearchable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(CreatedAt),
                    Type = SourcePropertyType.DateTime,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(UpdatedAt),
                    Type = SourcePropertyType.DateTime,
                    DefaultSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                    RequiredSearchAnnotations = (uint)(SearchAnnotations.IsQueryable | SearchAnnotations.IsRetrievable),
                });

            return schema;
        }


        public CrawlItem ToCrawlItem()
        {
            try
            {
                return new CrawlItem
                {
                    ItemType = CrawlItem.Types.ItemType.ContentItem,
                    ItemId = this.ID.ToString(CultureInfo.InvariantCulture),
                    ContentItem = this.GetContentItem(),
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }

        public IncrementalCrawlItem ToIncrementalCrawlItem()
        {
            try
            {
                return new IncrementalCrawlItem
                {
                    ItemType = IncrementalCrawlItem.Types.ItemType.ContentItem,
                    ItemId = this.ID.ToString(CultureInfo.InvariantCulture),
                    ContentItem = this.GetContentItem(),
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }

        private ContentItem GetContentItem()
        {
            return new ContentItem
            {
                AccessList = this.GetAccessControlList(),
                PropertyValues = this.GetSourcePropertyValueMap(),
                Content = this.GetContent(),
            };
        }

        private Content GetContent()
        {
            return new Content
            {
                ContentType = Content.Types.ContentType.Text,
                ContentValue = this.Body,
            };
        }

        private AccessControlList GetAccessControlList()
        {
            AccessControlList accessControlList = new AccessControlList();
            accessControlList.Entries.Add(this.GetAllowEveryoneAccessControlEntry());
            return accessControlList;
        }

        private AccessControlEntry GetAllowEveryoneAccessControlEntry()
        {
            return new AccessControlEntry
            {
                AccessType = AccessControlEntry.Types.AclAccessType.Grant,
                Principal = new Principal
                {
                    Type = Principal.Types.PrincipalType.Everyone,
                    IdentitySource = Principal.Types.IdentitySource.AzureActiveDirectory,
                    IdentityType = Principal.Types.IdentityType.AadId,
                    Value = "EVERYONE",
                }
            };
        }

        private SourcePropertyValueMap GetSourcePropertyValueMap()
        {
            SourcePropertyValueMap sourcePropertyValueMap = new SourcePropertyValueMap();

            sourcePropertyValueMap.Values.Add(
                nameof(this.ID),
                new GenericType
                {
                    StringValue = this.ID,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Title),
                new GenericType
                {
                    StringValue = this.Title,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Description),
                new GenericType
                {
                    StringValue = this.Description,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Body),
                new GenericType
                {
                    StringValue = this.Body,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.URL),
                new GenericType
                {
                    StringValue = this.URL,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.Tags),
                new GenericType
                {
                    StringValue = this.Tags,
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.CreatedAt),
                new GenericType
                {
                    DateTimeValue = Timestamp.FromDateTime(this.CreatedAt.ToUniversalTime()),
                });

            sourcePropertyValueMap.Values.Add(
                nameof(this.UpdatedAt),
                new GenericType
                {
                    DateTimeValue = Timestamp.FromDateTime(this.UpdatedAt.ToUniversalTime()),
                });

            return sourcePropertyValueMap;
        }
    }
}
