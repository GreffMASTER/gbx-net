﻿namespace GBX.NET.Managers;

public static class CollectionManager
{
    public static IDictionary<int, string> CustomCollections { get; } = new Dictionary<int, string>();

    public static string? GetName(int id) => id switch
    {
        0 => "Speed",
        1 => "Alpine",
        2 => "Rally",
        3 => "Island",
        4 => "Bay",
        5 => "Coast",
        6 => "Stadium",
        7 => "Basic",
        8 => "Plain",
        9 => "Moon",
        10 => "Toy",
        11 => "Valley",
        12 => "Canyon",
        13 => "Lagoon",
        14 => "Deprecated_Arena",
        17 => "TMCommon",
        18 => "Canyon4",
        19 => "Canyon256",
        20 => "Valley4",
        21 => "Valley256",
        22 => "Lagoon4",
        23 => "Lagoon256",
        24 => "Stadium4",
        25 => "Stadium256",
        26 => "Stadium2020",
        100 => "History",
        101 => "Society",
        102 => "Galaxy",
        200 => "Gothic",
        201 => "Paris",
        202 => "Storm",
        203 => "Cryo",
        204 => "Meteor",
        205 => "Meteor4",
        206 => "Meteor256",
        299 => "SMCommon",
        10000 => "Vehicles",
        10001 => "Orbital",
        10002 => "Actors",
        10003 => "Common",
        _ => CustomCollections.TryGetValue(id, out var v) ? v : null,
    };
}
