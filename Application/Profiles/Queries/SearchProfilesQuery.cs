using System.Globalization;
using System.Text.RegularExpressions;
using Application.Common;
using MediatR;

namespace Application.Profiles.Queries;

public class SearchProfilesQuery : IRequest<Result>
{
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
}

public class SearchProfilesQueryHandler : IRequestHandler<SearchProfilesQuery, Result>
{
    private readonly ISender _sender;

    public SearchProfilesQueryHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<Result> Handle(SearchProfilesQuery request, CancellationToken cancellationToken)
    {
        if (!ProfileNaturalLanguageQueryParser.TryParse(request.Q, out var filters))
        {
            return Result.Error("Unable to interpret query", 400);
        }

        filters.Page = request.Page;
        filters.Limit = request.Limit;

        return await _sender.Send(filters, cancellationToken);
    }
}

internal static partial class ProfileNaturalLanguageQueryParser
{
    private static readonly Dictionary<string, string> CountryLookup = BuildCountryLookup();

    public static bool TryParse(string? query, out GetAllProfilesQuery filters)
    {
        filters = new GetAllProfilesQuery();

        if (string.IsNullOrWhiteSpace(query))
        {
            return false;
        }

        var normalizedQuery = NormalizeFreeText(query);
        var matchedAnyRule = false;

        var hasMale = ContainsWord(normalizedQuery, "male") || ContainsWord(normalizedQuery, "males");
        var hasFemale = ContainsWord(normalizedQuery, "female") || ContainsWord(normalizedQuery, "females");

        if (hasMale ^ hasFemale)
        {
            filters.Gender = hasMale ? "male" : "female";
            matchedAnyRule = true;
        }

        if (ContainsAny(normalizedQuery, "teen", "teens", "teenager", "teenagers"))
        {
            filters.AgeGroup = "teenager";
            matchedAnyRule = true;
        }
        else if (ContainsAny(normalizedQuery, "child", "children"))
        {
            filters.AgeGroup = "child";
            matchedAnyRule = true;
        }
        else if (ContainsAny(normalizedQuery, "adult", "adults"))
        {
            filters.AgeGroup = "adult";
            matchedAnyRule = true;
        }
        else if (ContainsAny(normalizedQuery, "senior", "seniors"))
        {
            filters.AgeGroup = "senior";
            matchedAnyRule = true;
        }

        if (ContainsWord(normalizedQuery, "young"))
        {
            filters.MinAge = 16;
            filters.MaxAge = 24;
            matchedAnyRule = true;
        }

        var minAge = ExtractAge(normalizedQuery, AboveAgeRegex());
        if (minAge.HasValue)
        {
            filters.MinAge = Max(filters.MinAge, minAge.Value);
            matchedAnyRule = true;
        }

        var maxAge = ExtractAge(normalizedQuery, BelowAgeRegex());
        if (maxAge.HasValue)
        {
            filters.MaxAge = Min(filters.MaxAge, maxAge.Value);
            matchedAnyRule = true;
        }

        var countryId = ExtractCountryId(normalizedQuery);
        if (!string.IsNullOrWhiteSpace(countryId))
        {
            filters.CountryId = countryId;
            matchedAnyRule = true;
        }

        return matchedAnyRule;
    }

    private static string? ExtractCountryId(string normalizedQuery)
    {
        var match = FromCountryRegex().Match(normalizedQuery);
        if (!match.Success)
        {
            return null;
        }

        var countryPhrase = NormalizeFreeText(match.Groups["country"].Value);
        if (string.IsNullOrWhiteSpace(countryPhrase))
        {
            return null;
        }

        var words = countryPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var length = words.Length; length >= 1; length--)
        {
            var candidate = string.Join(' ', words.Take(length));

            if (CountryLookup.TryGetValue(candidate, out var countryId))
            {
                return countryId;
            }
        }

        if (countryPhrase.Length == 2 && countryPhrase.All(char.IsLetter))
        {
            return countryPhrase.ToUpperInvariant();
        }

        return null;
    }

    private static int? ExtractAge(string normalizedQuery, Regex regex)
    {
        var match = regex.Match(normalizedQuery);
        if (!match.Success)
        {
            return null;
        }

        return int.TryParse(match.Groups["age"].Value, out var age) ? age : null;
    }

    private static bool ContainsAny(string value, params string[] candidates)
    {
        return candidates.Any(candidate => ContainsWord(value, candidate));
    }

    private static bool ContainsWord(string value, string word)
    {
        return Regex.IsMatch(value, $@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase);
    }

    private static int Min(int? current, int candidate)
    {
        return current.HasValue ? Math.Min(current.Value, candidate) : candidate;
    }

    private static int Max(int? current, int candidate)
    {
        return current.HasValue ? Math.Max(current.Value, candidate) : candidate;
    }

    private static string NormalizeFreeText(string value)
    {
        var normalized = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^\p{L}\p{N}\s]", " ");
        normalized = Regex.Replace(normalized, @"\s+", " ");
        return normalized.Trim();
    }

    private static Dictionary<string, string> BuildCountryLookup()
    {
        var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var region = new RegionInfo(culture.Name);
                var countryId = region.TwoLetterISORegionName.ToUpperInvariant();

                lookup[NormalizeFreeText(region.EnglishName)] = countryId;
                lookup[NormalizeFreeText(region.NativeName)] = countryId;
            }
            catch (ArgumentException)
            {
                continue;
            }
        }

        return lookup;
    }

    [GeneratedRegex(@"\bfrom\s+(?<country>.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex FromCountryRegex();

    [GeneratedRegex(@"\b(?:above|over|older than|older)\s+(?<age>\d{1,3})\b", RegexOptions.IgnoreCase)]
    private static partial Regex AboveAgeRegex();

    [GeneratedRegex(@"\b(?:under|below|younger than|younger)\s+(?<age>\d{1,3})\b", RegexOptions.IgnoreCase)]
    private static partial Regex BelowAgeRegex();
}