using System.Collections.Generic;
using Newtonsoft.Json;

namespace GeoJSONModel
{
    public class FeatureCollection
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("features")]
        public List<Feature> Features { get; set; }
    }

    public class Feature
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }
    }

    public class Geometry
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        // Pour un Point, "coordinates" est un tableau de nombres [x, y]
        // Pour une LineString, "coordinates" est une liste de tableaux [ [x, y], [x, y], ... ]
        [JsonProperty("coordinates")]
        public object Coordinates { get; set; }
    }
}
