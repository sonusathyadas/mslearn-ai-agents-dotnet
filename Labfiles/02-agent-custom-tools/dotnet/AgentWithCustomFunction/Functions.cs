using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Azure.AI.Extensions.OpenAI;
using OpenAI.Responses;

#pragma warning disable OPENAI001

public static class AstronomyFunctions
{
    // -----------------------------------------------------------------------
    // Data loading helpers
    // -----------------------------------------------------------------------

    private static List<(string Name, string Type, int SortKey, string DateStr, HashSet<string> Locations)> LoadEvents(string filePath = "data/events.txt")
    {
        var events = new List<(string, string, int, string, HashSet<string>)>();
        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Trim().Split('|');
            if (parts.Length != 4) continue;
            var md = parts[2].Split('-');
            int month = int.Parse(md[0]);
            int day = int.Parse(md[1]);
            events.Add((
                parts[0],
                parts[1],
                month * 100 + day,
                parts[2],
                new HashSet<string>(parts[3].Split(';'))
            ));
        }
        events.Sort((a, b) => a.Item3.CompareTo(b.Item3));
        return events;
    }

    private static Dictionary<string, double> LoadRates(string filePath)
    {
        var rates = new Dictionary<string, double>();
        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Trim().Split('|');
            if (parts.Length == 2 && double.TryParse(parts[1], out double val))
                rates[parts[0]] = val;
        }
        return rates;
    }

    // Static data loaded once at startup
    private static readonly List<(string Name, string Type, int SortKey, string DateStr, HashSet<string> Locations)> Events = LoadEvents();

    private static readonly Dictionary<string, double> TelescopeRates = LoadRates("data/telescope_rates.txt");
    private static readonly Dictionary<string, double> PriorityMultipliers = LoadRates("data/priority_multipliers.txt");


    /// Returns the next visible astronomical event for a location.
    

    /// <summary>Calculates the cost of telescope observation time.</summary>
    public static string CalculateObservationCost(string telescopeTier, double hours, string priority)
    {
        string tier = telescopeTier.ToLower();
        string pri = priority.ToLower();

        if (!TelescopeRates.TryGetValue(tier, out double hourlyRate))
            return JsonSerializer.Serialize(new { error = $"Unknown telescope tier '{telescopeTier}'. Choose from: {string.Join(", ", TelescopeRates.Keys)}" });

        if (!PriorityMultipliers.TryGetValue(pri, out double multiplier))
            return JsonSerializer.Serialize(new { error = $"Unknown priority '{priority}'. Choose from: {string.Join(", ", PriorityMultipliers.Keys)}" });

        if (hours <= 0)
            return JsonSerializer.Serialize(new { error = "Hours must be greater than zero." });

        double baseCost = hourlyRate * hours;
        double totalCost = baseCost * multiplier;

        return JsonSerializer.Serialize(new
        {
            telescope_tier = tier,
            hours,
            hourly_rate = hourlyRate,
            priority = pri,
            priority_multiplier = multiplier,
            base_cost = baseCost,
            total_cost = totalCost
        });
    }

    /// <summary>Generates an observation session report and saves it to a file.</summary>
    public static string GenerateObservationReport(string eventName, string location, string telescopeTier, double hours, string priority, string observerName)
    {
        var costResult = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                              CalculateObservationCost(telescopeTier, hours, priority))!;
        var eventResult = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                              NextVisibleEvent(location))!;

        if (costResult.ContainsKey("error"))
            return JsonSerializer.Serialize(new { error = costResult["error"].GetString() });

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        string safeName = eventName.Replace(" ", "_").ToLower();
        string safeTs = timestamp.Replace(":", "").Replace(" ", "_");
        string filename = $"report_{safeName}_{safeTs}.txt";

        string nextEvent = eventResult.TryGetValue("event", out var ev) ? ev.GetString()! : "N/A";
        string nextDate = eventResult.TryGetValue("date", out var dt) ? dt.GetString()! : "N/A";

        var report = new StringBuilder();
        report.AppendLine("======================================");
        report.AppendLine("  CONTOSO OBSERVATORIES - SESSION REPORT");
        report.AppendLine("======================================");
        report.AppendLine($"Date:           {timestamp}");
        report.AppendLine($"Observer:       {observerName}");
        report.AppendLine($"Event:          {eventName}");
        report.AppendLine($"Location:       {location}");
        report.AppendLine();
        report.AppendLine("NEXT VISIBLE EVENT");
        report.AppendLine($"  Event:        {nextEvent}");
        report.AppendLine($"  Date:         {nextDate}");
        report.AppendLine();
        report.AppendLine("TELESCOPE BOOKING");
        report.AppendLine($"  Tier:         {costResult["telescope_tier"].GetString()}");
        report.AppendLine($"  Hours:        {costResult["hours"].GetDouble()}");
        report.AppendLine($"  Hourly Rate:  ${costResult["hourly_rate"].GetDouble():F2}");
        report.AppendLine($"  Priority:     {costResult["priority"].GetString()}");
        report.AppendLine($"  Multiplier:   {costResult["priority_multiplier"].GetDouble()}x");
        report.AppendLine();
        report.AppendLine("COST SUMMARY");
        report.AppendLine($"  Base Cost:    ${costResult["base_cost"].GetDouble():F2}");
        report.AppendLine($"  Total Cost:   ${costResult["total_cost"].GetDouble():F2}");
        report.AppendLine("======================================");

        File.WriteAllText(filename, report.ToString());
        return JsonSerializer.Serialize(new { status = "Report generated", file = filename });
    }

    

    public static readonly FunctionTool generateObservationReportTool = ResponseTool.CreateFunctionTool(
        functionName: "generate_observation_report",
        functionDescription: "Generate a report summarizing an astronomical observation.",
        functionParameters: BinaryData.FromObjectAsJson(new
        {
            type = "object",
            properties = new
            {
                event_name = new { type = "string", description = "The name of the astronomical event being observed" },
                location = new { type = "string", description = "The location of the observer" },
                telescope_tier = new { type = "string", description = "The tier of the telescope used (e.g. 'standard', 'advanced', 'premium')" },
                hours = new { type = "number", description = "The number of hours the telescope was used" },
                priority = new { type = "string", description = "The priority level of the observation (e.g. 'low', 'normal', 'high')" },
                observer_name = new { type = "string", description = "The name of the person who conducted the observation" }
            },
            required = new[] { "event_name", "location", "telescope_tier", "hours", "priority", "observer_name" },
            additionalProperties = false
        }),
        strictModeEnabled: true
    );


    /// <summary>Routes an incoming function call to the correct local method and wraps the result.</summary>
    public static FunctionCallOutputResponseItem GetResolvedToolOutput(FunctionCallResponseItem item)
    {
        using var doc = JsonDocument.Parse(item.FunctionArguments);
        var args = doc.RootElement;
        string result;

        switch (item.FunctionName)
        {
            case "next_visible_event":
                result = NextVisibleEvent(
                    args.GetProperty("location").GetString()!);
                break;

            case "calculate_observation_cost":
                result = CalculateObservationCost(
                    args.GetProperty("telescope_tier").GetString()!,
                    args.GetProperty("hours").GetDouble(),
                    args.GetProperty("priority").GetString()!);
                break;

            case "generate_observation_report":
                result = GenerateObservationReport(
                    args.GetProperty("event_name").GetString()!,
                    args.GetProperty("location").GetString()!,
                    args.GetProperty("telescope_tier").GetString()!,
                    args.GetProperty("hours").GetDouble(),
                    args.GetProperty("priority").GetString()!,
                    args.GetProperty("observer_name").GetString()!);
                break;

            default:
                result = JsonSerializer.Serialize(new { error = $"Unknown function: {item.FunctionName}" });
                break;
        }

        return ResponseItem.CreateFunctionCallOutputItem(item.CallId, result);
    }
}