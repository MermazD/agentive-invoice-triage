using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Extracts customer address information from OCR text.
/// Handles multi-line addresses where street/city/postal code may be split across lines.
/// </summary>
public class AddressExtractor
{
    /// <summary>
    /// Extract complete address from split OCR text lines.
    /// </summary>
    public static Address ExtractAddress(List<string> ocrLines)
    {
        var address = new Address
        {
            Street = string.Empty,
            PostalCode = string.Empty,
            City = string.Empty,
            Country = string.Empty
        };

        if (ocrLines == null || ocrLines.Count == 0)
            return address;

        // Find index of postal code first (more reliable anchor point)
        int postalCodeIndex = FindPostalCodeLineIndex(ocrLines);
        
        if (postalCodeIndex >= 0)
        {
            // Extract postal code and city from the line containing postal code
            var pcLine = ocrLines[postalCodeIndex];
            var (postalCode, city) = ExtractPostalCodeAndCity(pcLine);
            address.PostalCode = postalCode;
            address.City = city;

            // Street is likely a few lines before postal code
            // Scan backwards from postal code line to find street pattern
            int streetIndex = FindStreetLineIndex(ocrLines, postalCodeIndex);
            
            if (streetIndex >= 0)
            {
                // Collect street lines: may be split over 1-3 lines
                var streetLines = CollectStreetLines(ocrLines, streetIndex, postalCodeIndex);
                address.Street = string.Join(" ", streetLines);

                // Extract name from line(s) before street
                if (streetIndex > 0)
                {
                    var nameLine = ocrLines[streetIndex - 1];
                    // Skip if this line looks like street continuation or noise
                    if (!IsStreetLike(nameLine) && !IsNumericOnly(nameLine))
                    {
                        // Name is captured by OcrParser later; just note it
                    }
                }
            }

            // Extract country from lines after postal code (scan next 4-5 lines)
            address.Country = ExtractCountry(ocrLines, postalCodeIndex);
        }
        else
        {
            // Fallback: use old heuristic if no postal code found
            // This is a graceful degradation
            int streetIndex = FindStreetLineIndexLegacy(ocrLines);
            if (streetIndex >= 0 && streetIndex + 1 < ocrLines.Count)
            {
                address.Street = ocrLines[streetIndex];
                var (pc, city) = ExtractPostalCodeAndCity(ocrLines[streetIndex + 1]);
                address.PostalCode = pc;
                address.City = city;
            }
        }

        return address;
    }

    /// <summary>
    /// Find the line index containing a 5-digit postal code (French format).
    /// Scans all lines to find the most reliable postal code match.
    /// </summary>
    private static int FindPostalCodeLineIndex(List<string> ocrLines)
    {
        for (int i = 0; i < ocrLines.Count; i++)
        {
            // French postal code: exactly 5 digits
            if (Regex.IsMatch(ocrLines[i], @"\b\d{5}\b"))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Find the street line index by scanning backwards from postal code.
    /// Street pattern: number followed by words (e.g., "1337 N San Joaquin").
    /// Searches up to 10 lines before postal code.
    /// </summary>
    private static int FindStreetLineIndex(List<string> ocrLines, int postalCodeIndex)
    {
        var searchStart = Math.Max(0, postalCodeIndex - 10);
        for (int i = postalCodeIndex - 1; i >= searchStart; i--)
        {
            var line = ocrLines[i].Trim();
            // Street pattern: starts with digit(s) and contains letters
            // Examples: "1337 N San Joaquin", "4557 De Silva St", "215 Vine St"
            if (Regex.IsMatch(line, @"^\d+\s+.*[a-zA-Z]") && !IsAllCaps(line))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Legacy street finder (used as fallback). Finds first line matching street pattern.
    /// </summary>
    private static int FindStreetLineIndexLegacy(List<string> ocrLines)
    {
        for (int i = 0; i < ocrLines.Count; i++)
        {
            if (Regex.IsMatch(ocrLines[i], @"\d+\s+\w+"))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Collect street address lines that may span multiple OCR lines.
    /// If street is split (e.g., "1337 N San Joaquin" on one line, "St" on next),
    /// this collects all parts until city/postal line.
    /// </summary>
    private static List<string> CollectStreetLines(List<string> ocrLines, int streetIndex, int postalCodeIndex)
    {
        var streetLines = new List<string>();
        
        // Start from street index and collect consecutive street-like lines
        for (int i = streetIndex; i < postalCodeIndex; i++)
        {
            var line = ocrLines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            
            // Stop if we hit something that looks like a name or company (all caps, short)
            if (IsAllCaps(line) && line.Length < 50)
                break;
            
            // Stop if we hit numeric-only or irrelevant text
            if (IsNumericOnly(line) || line.Length > 200)
                break;
            
            streetLines.Add(line);
        }
        
        return streetLines.Count > 0 ? streetLines : new List<string> { ocrLines[streetIndex] };
    }

    /// <summary>
    /// Extract postal code and city from a line like "Stockton CA 95202" or "94538 San Francisco".
    /// Returns (postalCode, city).
    /// </summary>
    private static (string, string) ExtractPostalCodeAndCity(string line)
    {
        if (string.IsNullOrEmpty(line))
            return (string.Empty, string.Empty);

        // Pattern 1: "City State 12345"
        var match = Regex.Match(line, @"^(.+?)\s+([A-Z]{2})\s+(\d{5})$");
        if (match.Success)
        {
            return (match.Groups[3].Value, $"{match.Groups[1].Value.Trim()} {match.Groups[2].Value}");
        }

        // Pattern 2: "12345 City Country"
        match = Regex.Match(line, @"^(\d{5})\s+(.+)$");
        if (match.Success)
        {
            return (match.Groups[1].Value, match.Groups[2].Value.Trim());
        }

        // Pattern 3: just postal code at end
        match = Regex.Match(line, @"(\d{5})");
        if (match.Success)
        {
            var pc = match.Groups[1].Value;
            var city = line.Replace(pc, "").Trim();
            return (pc, city);
        }

        return (string.Empty, string.Empty);
    }

    /// <summary>
    /// Extract country name from lines after postal code (typically 1-5 lines later).
    /// Country is typically a line containing only letters and spaces.
    /// </summary>
    private static string ExtractCountry(List<string> ocrLines, int postalCodeIndex)
    {
        // Search next 5 lines after postal code for country
        for (int j = postalCodeIndex + 1; j < Math.Min(ocrLines.Count, postalCodeIndex + 6); j++)
        {
            var line = ocrLines[j].Trim();
            // Country line: mostly letters, no numbers or special chars
            if (!string.IsNullOrEmpty(line) && 
                line.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)) &&
                line.Length > 2)
            {
                return line;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Check if line is all uppercase (common for company names, which should be skipped).
    /// </summary>
    private static bool IsAllCaps(string line)
    {
        if (string.IsNullOrEmpty(line))
            return false;
        var letters = line.Where(c => char.IsLetter(c)).ToList();
        return letters.Count > 0 && letters.All(c => char.IsUpper(c));
    }

    /// <summary>
    /// Check if line contains only digits and whitespace.
    /// </summary>
    private static bool IsNumericOnly(string line)
    {
        return !string.IsNullOrEmpty(line) && line.All(c => char.IsDigit(c) || char.IsWhiteSpace(c));
    }

    /// <summary>
    /// Check if line looks like street address (starts with digit and has letters).
    /// </summary>
    private static bool IsStreetLike(string line)
    {
        return Regex.IsMatch(line, @"^\d+\s+.*[a-zA-Z]");
    }
}
