using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace ComputerBuilder.Tools
{ }
    // Simple utility to merge component CSVs in the Datasets folder into a single wide CSV:
    // Output: Datasets/combined-builds.csv
    // Usage:
    // 1) Create a console project (if you don't already have one):
    //    dotnet new console -o Tools/MergeDatasetsApp -f net10.0
    // 2) Replace the generated Program.cs with this file (path shown above) and run:
    //    dotnet run --project Tools/MergeDatasetsApp -- "Datasets"
    //
    // The tool:
    // - Scans the specified Datasets folder (defaults to ./Datasets)
    // - Detects CSVs for CPU, GPU, Motherboard and RAM (by filename keywords)
    // - Parses each CSV, extracts a name and numeric price when possible
    // - For a configurable set of Types and Budgets it picks one component per category
    //   using a simple budget-allocation heuristic (percentages per component)
    // - Writes Datasets/combined-builds.csv with rows: Type,Budget,CPU,GPU,Motherboard,RAM,TotalPrice
    //
    // This is a pragmatic merge script (no external libs). Tweak budgets/types/allocations as needed.
    /*
    record ComponentItem(string Name, double Price);

    class Program
    {
        static void Main(string[] args)
        {
            var cwd = Directory.GetCurrentDirectory();
            var datasetsDir = args.Length > 0 ? args[0] : Path.Combine(cwd, "Datasets");
            if (!Directory.Exists(datasetsDir))
            {
                Console.Error.WriteLine($"Datasets folder not found: {datasetsDir}");
                return;
            }

            Console.WriteLine($"Scanning CSVs in: {datasetsDir}");

            var csvFiles = Directory.GetFiles(datasetsDir, "*.csv", SearchOption.TopDirectoryOnly)
                                    .Where(f => !Path.GetFileName(f).Equals("combined-builds.csv", StringComparison.OrdinalIgnoreCase))
                                    .ToArray();

            // Group files by component type using filename keywords
            var filesByType = new Dictionary<string, string[]>
            {
                ["cpu"] = csvFiles.Where(f => Path.GetFileName(f).ToLower().Contains("cpu")).ToArray(),
                ["gpu"] = csvFiles.Where(f => Path.GetFileName(f).ToLower().Contains("gpu")).ToArray(),
                ["motherboard"] = csvFiles.Where(f => Path.GetFileName(f).ToLower().Contains("motherboard") || Path.GetFileName(f).ToLower().Contains("mb")).ToArray(),
                ["ram"] = csvFiles.Where(f => Path.GetFileName(f).ToLower().Contains("ram") || Path.GetFileName(f).ToLower().Contains("memory")).ToArray()
            };

            // If specific files are missing, try to pick likely candidates by heuristics
            // (e.g., cpu dataset might be named "processors.csv" etc.). We'll also try fallback by header detection.
            var components = new Dictionary<string, List<ComponentItem>>();
            foreach (var key in filesByType.Keys)
            {
                var items = new List<ComponentItem>();
                foreach (var file in filesByType[key])
                {
                    items.AddRange(ParseCsvForItems(file));
                }

                // If none found by filename, attempt to search all CSVs by header heuristics
                if (!items.Any())
                {
                    foreach (var file in csvFiles.Except(filesByType.SelectMany(kv => kv.Value)).ToArray())
                    {
                        var parsed = ParseCsvForItems(file);
                        // heuristics: if parsed rows contain columns that look like CPUs/GPUs (presence of 'tdp','cores','vram' etc.)
                        // We'll add everything anyway to give the merger data to work with
                        if (parsed.Any())
                            items.AddRange(parsed);
                    }
                }

                // remove items without price
                items = items.Where(i => i.Price > 0).GroupBy(i => i.Name).Select(g => g.First()).ToList();
                components[key] = items.OrderBy(i => i.Price).ToList();
                Console.WriteLine($"Found {components[key].Count} items for '{key}'");
            }

            // Types and budgets to generate rows for -- change as needed
            var types = new[] { "Gaming", "Office", "Workstation", "Server", "Content Creation" };
            var budgets = new[] { 500.0, 1000.0, 2000.0, 5000.0 };

            // Allocation percentages (simple heuristic)
            var allocations = new Dictionary<string, double>
            {
                ["cpu"] = 0.25,
                ["gpu"] = 0.40, // gaming heavy
                ["motherboard"] = 0.15,
                ["ram"] = 0.10
            };

            var outputLines = new List<string>();
            outputLines.Add("Type,Budget,CPU,CPU_Price,GPU,GPU_Price,Motherboard,MB_Price,RAM,RAM_Price,TotalPrice");

            foreach (var t in types)
            {
                foreach (var b in budgets)
                {
                    var picks = new Dictionary<string, ComponentItem?>();
                    double total = 0;

                    foreach (var comp in allocations.Keys)
                    {
                        var compList = components.ContainsKey(comp) ? components[comp] : new List<ComponentItem>();
                        if (!compList.Any())
                        {
                            picks[comp] = null;
                            continue;
                        }

                        var alloc = allocations[comp] * b;
                        // choose the most expensive item that does not exceed the allocation, if possible,
                        // otherwise choose the cheapest available (best-effort)
                        var pick = compList.Where(i => i.Price <= alloc).OrderByDescending(i => i.Price).FirstOrDefault()
                                   ?? compList.FirstOrDefault();

                        picks[comp] = pick;
                        if (pick is not null) total += pick.Price;
                    }

                    var cpu = picks["cpu"]?.Name ?? "";
                    var cpuP = picks["cpu"]?.Price.ToString("F2", CultureInfo.InvariantCulture) ?? "";
                    var gpu = picks["gpu"]?.Name ?? "";
                    var gpuP = picks["gpu"]?.Price.ToString("F2", CultureInfo.InvariantCulture) ?? "";
                    var mb = picks["motherboard"]?.Name ?? "";
                    var mbP = picks["motherboard"]?.Price.ToString("F2", CultureInfo.InvariantCulture) ?? "";
                    var ram = picks["ram"]?.Name ?? "";
                    var ramP = picks["ram"]?.Price.ToString("F2", CultureInfo.InvariantCulture) ?? "";

                    // CSV-escape values (wrap with quotes and double-up inner quotes)
                    string Esc(string s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";

                    outputLines.Add(string.Join(",",
                        Esc(t),
                        Esc($"${b:0}"),
                        Esc(cpu), cpuP,
                        Esc(gpu), gpuP,
                        Esc(mb), mbP,
                        Esc(ram), ramP,
                        total.ToString("F2", CultureInfo.InvariantCulture)
                    ));
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
                if (headerLine == null) return items;
                var headers = ParseCsvLine(headerLine).Select(h => h.Trim().ToLowerInvariant()).ToArray();

                // heuristics for name and price columns
                var nameIdx = Array.FindIndex(headers, h => h.Contains("name") || h.Contains("title") || h.Contains("part"));
                if (nameIdx == -1) nameIdx = 0;

                var priceIdx = Array.FindIndex(headers, h => h.Contains("price") || h.Contains("cost") || h.Contains("usd") || h.Contains("$"));
                if (priceIdx == -1)
                {
                    // fallback: try columns that look numeric
                    priceIdx = Array.FindIndex(headers, h => h.Contains("msrp") || h.Contains("retail"));
                }

                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = ParseCsvLine(line);
                    var name = cols.Length > nameIdx ? cols[nameIdx].Trim() : "";
                    double price = 0;
                    if (priceIdx >= 0 && cols.Length > priceIdx)
                    {
                        var pstr = cols[priceIdx].Replace("$", "").Replace(",", "").Trim();
                        if (!double.TryParse(pstr, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
                        {
                            // try localized parse
                            double tmp;
                            if (double.TryParse(pstr, NumberStyles.Any, CultureInfo.CurrentCulture, out tmp))
                                price = tmp;
                        }
                    }
                    // additional attempt: if price not found, try scanning other columns for numeric value
                    if (price == 0)
                    {
                        foreach (var c in cols.Skip(1).Take(5))
                        {
                            var pstr2 = c.Replace("$", "").Replace(",", "").Trim();
                            if (double.TryParse(pstr2, NumberStyles.Any, CultureInfo.InvariantCulture, out var p2) && p2 > 0 && p2 < 10000)
                            {
                                price = p2;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(name) && price > 0)
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
                if (c == '"' )
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // escaped quote
                        sb.Append('"');
                        i++; // skip next
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