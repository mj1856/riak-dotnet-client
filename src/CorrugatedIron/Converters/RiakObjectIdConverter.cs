// Copyright (c) 2011 - OJ Reeves & Jeremiah Peschka
// Copyright (c) 2015 - Basho Technologies, Inc.
//
// This file is provided to you under the Apache License,
// Version 2.0 (the "License"); you may not use this file
// except in compliance with the License.  You may obtain
// a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using CorrugatedIron.Models;
using Newtonsoft.Json;

namespace CorrugatedIron.Converters
{
    /*
     * TODO: Figure out if this is still needed
     */
    public class RiakObjectIdConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
        {
            int pos = 0;
            string bucket = null;
            string key = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.String || reader.TokenType == JsonToken.PropertyName)
                {
                    if (pos == 0)
                    {
                        bucket = reader.Value.ToString();
                    }
                    if (pos == 1)
                    {
                        key = reader.Value.ToString();
                    }
                }
                if (pos > 1)
                {
                    break;
                }
                pos++;
            }

            return new RiakObjectId(bucket, key);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
