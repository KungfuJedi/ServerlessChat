using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using Serverless.Domain.Constants;

namespace Serverless.Domain.Models
{
    [DynamoDBTable(DynamoDbTables.UserConnectionMappings)]
    public class UserConnectionMappings
    {
        public const int IdValue = 1;

        [DynamoDBHashKey] 
        public int Id { get; set; }

        [DynamoDBProperty]
        public string Mapping { get; set; }

        [DynamoDBIgnore]
        private List<UserConnectionMapping> _mappings;

        public UserConnectionMappings()
        {
            Id = 1;
        }

        public IReadOnlyList<UserConnectionMapping> GetUserConnectionMappings() => GetOrDeserializeMappings().AsReadOnly();

        public void AddMapping(UserConnectionMapping mapping)
        {
            var mappings = GetOrDeserializeMappings();
            if (mappings.Any(ucm => ucm.ConnectionId == mapping.ConnectionId && ucm.UserId == mapping.UserId))
                return;

            mappings.Add(mapping);
            Mapping = JsonConvert.SerializeObject(mappings);
        }

        public void RemoveMapping(UserConnectionMapping mapping)
        {
            var mappings = GetOrDeserializeMappings();
            if (!mappings.Any(ucm => ucm.ConnectionId == mapping.ConnectionId && ucm.UserId == mapping.UserId))
                return;

            mappings.RemoveAll(ucm => ucm.ConnectionId == mapping.ConnectionId && ucm.UserId == mapping.UserId);
            Mapping = JsonConvert.SerializeObject(mappings);
        }

        private List<UserConnectionMapping> GetOrDeserializeMappings()
        {
            if (_mappings != null)
                return _mappings;

            _mappings = string.IsNullOrEmpty(Mapping)
                ? new List<UserConnectionMapping>()
                : JsonConvert.DeserializeObject<List<UserConnectionMapping>>(Mapping);

            return _mappings;
        }
    }
}