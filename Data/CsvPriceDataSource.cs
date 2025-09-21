using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TradingBacktester.Models;

namespace TradingBacktester.Data
{
    /// <summary>
    /// Loads price data from CSV files
    /// Handles file I/O, parsing, and validation
    /// </summary>
    public class CsvPriceDataSource
    {
        /// <summary>
        /// Load price data from a CSV file
        /// Expected format: Date,Open,High,Low,Close (with header row)
        /// MVP in mind, eventually will edit file to be able to handle more than OHLC
        /// </summary>
        public List<Price> LoadPrices(string filePath)
        {
            var prices = new List<Price>();

            try
            {
                // NOTETOSELF: File.ReadAllLines() automatically handles file opening/closing 
                string[] lines = File.ReadAllLines(filePath);

                if (lines.Length <= 1)
                    throw new InvalidDataException("CSV file must contain header row and at least one data row");

                // SKIP HEADER: Start from line 1 to skip "Date,Open,High,Low,Close"
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var price = ParseCsvLine(lines[i], i + 1); // i+1 is line number
                        prices.Add(price);
                    }
                    catch (Exception ex)
                    {
                        // DATA VALIDATION: Skip bad rows but warn user
                        // not critical but should log
                        Console.WriteLine($"Warning: Skipping line {i + 1} due to error: {ex.Message}");
                    }
                }

                // FINAL VALIDATION: Make sure we got some data
                if (prices.Count == 0)
                    throw new InvalidDataException("No valid price data found in CSV file");

                // SORT BY DATE in case csv data file was not in order for whatever reason (extra validation)
                prices = prices.OrderBy(p => p.Date).ToList();

                Console.WriteLine($"✅ Successfully loaded {prices.Count} price records");
                return prices;
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"CSV file not found: {filePath}");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"Access denied to file: {filePath}");
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Error loading CSV file: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper for parsing a single CSV line into a Price object
        /// Example line: "2024-01-02,185.50,188.20,184.30,187.45"
        /// </summary>
        private Price ParseCsvLine(string csvLine, int lineNumber)
        {
            // NOTETOSELF split is similar to Python
            string[] fields = csvLine.Split(',');

            // FIELD COUNT VALIDATION
            // Const may need to be changed as future include more columns and expected length may need to be extracted
            if (fields.Length != 5)
                throw new InvalidDataException($"Expected 5 fields (Date,Open,High,Low,Close), got {fields.Length}");

            try
            {
                // PARSE EACH FIELD: Convert strings to appropriate types and trim
                DateTime date = DateTime.Parse(fields[0].Trim());
                decimal open = decimal.Parse(fields[1].Trim());
                decimal high = decimal.Parse(fields[2].Trim());
                decimal low = decimal.Parse(fields[3].Trim());
                decimal close = decimal.Parse(fields[4].Trim());

                // CREATE PRICE OBJECT: Constructor will validate OHLC relationships
                return new Price(date, open, high, low, close);
            }
            catch (FormatException ex)
            {
                throw new InvalidDataException($"Line {lineNumber}: Unable to parse field - {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                throw new InvalidDataException($"Line {lineNumber}: Invalid price data - {ex.Message}");
            }
        }
    }
}