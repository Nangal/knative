// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using CloudNative.CloudEvents;
using Newtonsoft.Json.Linq;

namespace Common
{
    public class AuditLogBucketEventDataReader : IBucketEventDataReader
    {
        public (string, string) Read(CloudEvent cloudEvent)
        {
            dynamic data = JValue.Parse((string)cloudEvent.Data);
            //"protoPayload" : {"resourceName":"projects/_/buckets/events-nikhilbarthwal-images-input/objects/nikhilbarthwal.jpg}";
            var tokens = ((string)data.protoPayload.resourceName).Split('/');
            var bucket = tokens[3];
            var name = tokens[5];
            return (bucket, name);
        }
    }
}