using NetTopologySuite.Geometries;
using SurfScout.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfScout.Functions
{
    public static class Json_Helpers
    {
        public static Polygon? CreatePolygonFromDto(GeoJsonDto dto)
        {
            if (dto.Coordinates == null || !dto.Coordinates.Any())
                return null;

            var shell = new LinearRing(dto.Coordinates[0]
                .Select(pair => new Coordinate(pair[0], pair[1])).ToArray());

            return new Polygon(shell) { SRID = 4326 };
        }
    }
}
