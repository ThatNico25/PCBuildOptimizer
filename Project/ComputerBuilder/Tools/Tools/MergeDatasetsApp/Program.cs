using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

//namespace ComputerBuilder.Tools
//{
    // Enhanced dataset merger that includes all component types found in the Datasets folder.
    // - Scans CSV filenames with keyword heuristics to map files to component categories
    // - Parses name + price from each CSV (robust numeric detection)
    // - Uses allocation percentages to pick components for each Type/Budget row
    //
    // Output: Datasets/combined-builds.csv
    //
    // Usage:
    // dotnet run --project Tools/Tools/MergeDatasetsApp -- "path/to/Datasets"
    /*
    record ComponentItem(string Name, double Price);

    class Program
    {
        static void Main(string[] args)
        {
            var cwd = Directory.GetCurrentDirectory();
            var datasetsDir = args.Length > 0 ? args[0] : Path.Combine(cwd, "Datasets");
            Console.WriteLine($"Working dir: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Looking for datasets at: {datasetsDir}");

            if (!Directory.Exists(datasetsDir))
            {
                Console.Error.WriteLine($"Datasets folder not found: {datasetsDir}");
                return;
            }

            var csvFiles = Directory.GetFiles(datasetsDir, "*.csv", SearchOption.TopDirectoryOnly)
                                    .Where(f => !Path.GetFileName(f).Equals("combined-builds.csv", StringComparison.OrdinalIgnoreCase))
                                    .ToArray();

            if (!csvFiles.Any())
            {
                Console.Error.WriteLine("No CSV files found in datasets folder.");
                return;
            }

            // Map component key -> keywords to detect from filename
            var componentKeywords = new Dictionary<string, string[]>
            {
                ["cpu"] = new[] { "cpu", "processor", "amd", "intel" },
                ["gpu"] = new[] { "gpu", "graphics", "vga", "video" },
                ["motherboard"] = new[] { "motherboard", "mb", "board" },
                ["ram"] = new[] { "ram", "memory", "ddr" },
                ["optical-drive"] = new[] { "optical", "dvd", "cd", "blu-ray", "bd" },
                ["psu"] = new[] { "psu", "power", "power_supply", "psu_unit" , "power supply" },
                ["case"] = new[] { "case", "chassis", "enclosure" },
                ["cooling"] = new[] { "cooler", "cpu-cooler", "aio", "liquid", "fan", "cooling" },
                ["peripherals"] = new[] { "keyboard", "mouse", "monitor", "peripheral", "headset" },
                // fallback: miscellaneous
                ["other"] = new string[] { }
            };

            // Prepare container
            var components = componentKeywords.Keys.ToDictionary(k => k, k => new List<ComponentItem>());

            // First pass: assign files to components by filename keyword
            var unassignedFiles = new List<string>();
            foreach (var file in csvFiles)
            {
                var fname = Path.GetFileName(file).ToLowerInvariant();
                bool assigned = false;
                foreach (var kv in componentKeywords)
                {
                    if (kv.Value.Any(kw => fname.Contains(kw)))
                    {
                        components[kv.Key].AddRange(ParseCsvForItems(file));
                        assigned = true;
                        break;
                    }
                }

                if (!assigned)
                {
                    unassignedFiles.Add(file);
                }
            }

            // Second pass: try to heuristically parse unassigned files and distribute if possible
            foreach (var file in unassignedFiles)
            {
                var parsed = ParseCsvForItems(file);
                if (!parsed.Any()) continue;

                // Heuristic: inspect header names (done inside ParseCsvForItems) - but we don't have type.
                // Conservative fallback: add to "other" so data isn't lost.
                components["other"].AddRange(parsed);
            }

            // Clean components: keep items with positive price, deduplicate by name, sort ascending price
            foreach (var key in components.Keys.ToList())
            {
                var list = components[key]
                    .Where(i => i.Price > 0 && !string.IsNullOrWhiteSpace(i.Name))
                    .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .OrderBy(i => i.Price)
                    .ToList();
                components[key] = list;
                Console.WriteLine($"Found {list.Count} items for '{key}'");
            }

            // Types and budgets to generate rows for
            var types = new[] { "Gaming", "Office", "Workstation", "Server", "Content Creation" };
            var budgets = new[] { 500.0, 1000.0, 2000.0, 5000.0 };

            // Allocation percentages (sum = 1.0). Tweak to taste per target Type if desired.
            var allocations = new Dictionary<string, double>
            {
                ["cpu"] = 0.20,
                ["gpu"] = 0.30,
                ["motherboard"] = 0.10,
                ["ram"] = 0.08,
                ["optical-drive"] = 0.10,
                ["psu"] = 0.05,
                ["case"] = 0.03,
                ["cooling"] = 0.08,
                ["peripherals"] = 0.06
            };

            // Build header dynamically from allocation keys (keeps ordering deterministic)
            var compKeysOrdered = allocations.Keys.ToList();
            var headerCols = new List<string> { "ID", "Type", "Budget" };
            foreach (var k in compKeysOrdered)
            {
                headerCols.Add(k.ToUpperInvariant());
                headerCols.Add($"{k.ToUpperInvariant()}_Price");
            }
            headerCols.Add("TotalPrice");

            var outputLines = new List<string>();
            outputLines.Add(string.Join(",", headerCols));

            int buildId = 1;
            foreach (var t in types)
            {
                foreach (var b in budgets)
                {
                    var picks = new Dictionary<string, ComponentItem?>();

                    double total = 0;

                    foreach (var comp in compKeysOrdered)
                    {
                        var compList = components.ContainsKey(comp) ? components[comp] : new List<ComponentItem>();
                        if (!compList.Any())
                        {
                            picks[comp] = null;
                            continue;
                        }

                        var alloc = allocations[comp] * b;

                        // pick strategy:
                        // 1) prefer item priced <= alloc, choose the most expensive under alloc
                        // 2) otherwise choose closest (cheapest or best-fit by ratio)
                        var pick = compList.Where(i => i.Price <= alloc).OrderByDescending(i => i.Price).FirstOrDefault();
                        if (pick == null)
                        {
                            // choose cheapest if all exceed allocation
                            pick = compList.OrderBy(i => Math.Abs(i.Price - alloc)).FirstOrDefault();
                        }

                        picks[comp] = pick;
                        if (pick is not null) total += pick.Price;
                    }

                    // CSV row assembly with escaping
                    string Esc(string s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";

                    var row = new List<string>
                    {
                        buildId.ToString(),
                        Esc(t),
                        Esc($"${b:0}")
                    };

                    foreach (var comp in compKeysOrdered)
                    {
                        var name = picks[comp]?.Name ?? "";
                        var price = picks[comp]?.Price.ToString("F2", CultureInfo.InvariantCulture) ?? "";
                        row.Add(Esc(name));
                        row.Add(price);
                    }

                    row.Add(total.ToString("F2", CultureInfo.InvariantCulture));
                    outputLines.Add(string.Join(",", row));
                    buildId++;
                }
            }

            var outPath = Path.Combine(datasetsDir, "combined-builds.csv");
            File.WriteAllLines(outPath, outputLines, Encoding.UTF8);
            Console.WriteLine($"Combined builds written to: {outPath}");
        }

        static List<ComponentItem> ParseCsvForItems(string path)
        {
            var items = new List<ComponentItem>();
            try
            {
                using var sr = new StreamReader(path);
                var headerLine = sr.ReadLine();
                var headers = headerLine != null ? ParseCsvLine(headerLine).Select(h => h.Trim().ToLowerInvariant()).ToArray() : Array.Empty<string>();

                int nameIdx = Array.FindIndex(headers, h => h.Contains("name") || h.Contains("title") || h.Contains("part") || h.Contains("model"));
                if (nameIdx == -1) nameIdx = 0;

                int priceIdx = Array.FindIndex(headers, h => h.Contains("price") || h.Contains("cost") || h.Contains("usd") || h.Contains("$") || h.Contains("msrp") || h.Contains("retail"));
                // priceIdx may remain -1 -> we will scan fields to find a numeric

                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = ParseCsvLine(line);
                    if (cols.Length == 0) continue;

                    var name = cols.Length > nameIdx ? cols[nameIdx].Trim() : cols.FirstOrDefault()?.Trim() ?? "";
                    double price = 0;

                    if (priceIdx >= 0 && cols.Length > priceIdx)
                    {
                        price = TryParsePrice(cols[priceIdx]);
                    }

                    // if price not found, scan other columns for plausible price (numeric, >0, <20000)
                    if (price == 0)
                    {
                        foreach (var c in cols)
                        {
                            var p = TryParsePrice(c);
                            if (p > 0 && p < 20000)
                            {
                                price = p;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(name) && price > 0)
                    {
                        items.Add(new ComponentItem(name, price));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to parse {path}: {ex.Message}");
            }

            return items;
        }

        static double TryParsePrice(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 0;

            var s = raw.Replace("$", "")
                       .Replace("CAD", "", StringComparison.OrdinalIgnoreCase)
                       .Replace("USD", "", StringComparison.OrdinalIgnoreCase)
                       .Replace("€", "")
                       .Replace("£", "")
                       .Trim();

            // Some CSVs use ranges like "2000,3000" or "2000-3000" — try to pick the numeric portion or average
            if (s.Contains("-") || s.Contains("–"))
            {
                var parts = s.Split(new[] { '-', '–' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                var nums = parts.Select(p =>
                {
                    if (double.TryParse(p, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedToken))
                        return (double?)parsedToken;
                    return null;
                }).Where(x => x.HasValue).Select(x => x!.Value).ToArray();

                if (nums.Any()) return nums.Average();
            }

            if (s.Contains(","))
            {
                // comma may separate thousands or be used in ranges — try parse directly, else try to take first numeric token
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double directValue)) return directValue;
                var tokens = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in tokens)
                {
                    if (double.TryParse(t.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double tokenVal)) return tokenVal;
                }
            }

            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double invariantVal))
                return invariantVal;

            if (double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out double currentVal))
                return currentVal;

            return 0;
        }

        // Basic CSV line parser supporting quoted fields and commas inside quotes.
        static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            if (line is null) return fields.ToArray();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            fields.Add(sb.ToString());
            return fields.ToArray();
        }
    }
}*/