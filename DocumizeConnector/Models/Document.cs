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
        public string Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty("excerpt", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("body", Required = Required.Always)]
        public string Body { get; set; }

        [JsonProperty("tags", DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Tags { get; set; }

        [JsonProperty("created_at", Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        [DefaultValue(null)]
        [JsonProperty("updated_at", DefaultValueHandling = DefaultValueHandling.Populate)]
        public DateTime UpdatedAt { get; set; }


        public static DataSourceSchema GetSchema()
        {
            DataSourceSchema schema = new DataSourceSchema();

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Id),
                    Type = SourcePropertyType.String,
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Title),
                    Type = SourcePropertyType.String,
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Description),
                    Type = SourcePropertyType.String,
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(Tags),
                    Type = SourcePropertyType.StringCollection,
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(CreatedAt),
                    Type = SourcePropertyType.DateTime,
                });

            schema.PropertyList.Add(
                new SourcePropertyDefinition
                {
                    Name = nameof(UpdatedAt),
                    Type = SourcePropertyType.DateTime,
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
                    ItemId = this.Id.ToString(CultureInfo.InvariantCulture),
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
                    ItemId = this.Id.ToString(CultureInfo.InvariantCulture),
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
                nameof(this.Id),
                new GenericType
                {
                    StringValue = this.Id,
                });

            //sourcePropertyValueMap.Values.Add(
            //    nameof(this.Url),
            //    new GenericType
            //    {
            //        StringValue = this.Url,
            //    });

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

            var tagPropertyValue = new StringCollectionType();
            foreach (var tag in this.Tags)
            {
                tagPropertyValue.Values.Add(tag);
            }
            sourcePropertyValueMap.Values.Add(
                nameof(this.Tags),
                new GenericType
                {
                    StringCollectionValue = tagPropertyValue,
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
