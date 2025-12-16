using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services
{
    public class FormulaCalculatorService
    {
        public Dictionary<string, double> CalculateComponents(
            List<TableResult> tables,
            Dictionary<string, string> componentFormulas,
            Dictionary<string, string>? valueMappings = null)
        {
            var results = new Dictionary<string, double>();

            foreach (var formulaEntry in componentFormulas)
            {
                try
                {
                    string formula = formulaEntry.Value;
                    
                    if (!ContainsOperators(formula))
                    {
                        string lookupKey = formula;
                        
                        if (valueMappings != null && valueMappings.TryGetValue(formulaEntry.Key, out var mappedName))
                        {
                            lookupKey = mappedName;
                        }
                        
                        double value = FindExactValue(lookupKey, tables) ?? FindPartialValue(lookupKey, tables);
                        results[formulaEntry.Key] = Math.Round(value, 2);
                    }
                    else
                    {
                        double result = EvaluateComplexFormula(formula, tables, valueMappings);
                        results[formulaEntry.Key] = Math.Round(result, 2);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка расчета компонента '{formulaEntry.Key}': {ex.Message}");
                    results[formulaEntry.Key] = 0;
                }
            }

            return results;
        }

        public double CalculateFinalScore(
            string formula,
            Dictionary<string, double> components,
            List<TableResult> tables,
            string? aggregateMethod = null)
        {
            if (string.IsNullOrWhiteSpace(formula))
                return 0;

            try
            {
                if (!string.IsNullOrWhiteSpace(aggregateMethod))
                {
                    switch (aggregateMethod.ToUpper())
                    {
                        case "SUM":
                            return CalculateSumForColumn(formula, tables);
                        case "AVG":
                            return CalculateAverageForColumn(formula, tables);
                        case "MAX":
                            return CalculateMaxForColumn(formula, tables);
                        case "MIN":
                            return CalculateMinForColumn(formula, tables);
                    }
                }
                
                if (components.ContainsKey(formula.Trim()))
                {
                    return components[formula.Trim()];
                }
                
                if (!ContainsOperators(formula) && !IsNumericExpression(formula))
                {
                    double value = FindExactValue(formula, tables) ?? FindPartialValue(formula, tables);
                    return Math.Round(value, 2);
                }
                
                if (ContainsOperators(formula))
                {
                    var variables = ExtractVariables(formula);
                    var variableValues = new Dictionary<string, double>();
                    
                    foreach (var variable in variables)
                    {
                        if (components.ContainsKey(variable))
                        {
                            variableValues[variable] = components[variable];
                        }
                        else
                        {
                            variableValues[variable] = FindExactValue(variable, tables) ?? FindPartialValue(variable, tables);
                        }
                    }
                    
                    string expression = formula;
                    foreach (var variable in variableValues)
                    {
                        expression = expression.Replace(variable.Key, 
                            variable.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                    }
                    
                    return Math.Round(EvaluateExpression(expression), 2);
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка расчета итогового балла '{formula}': {ex.Message}");
                return 0;
            }
        }

        private double CalculateSumForColumn(string columnName, List<TableResult> tables)
        {
            double sum = 0;
            
            foreach (var table in tables)
            {
                foreach (var kvp in table.Data)
                {
                    if (string.Equals(kvp.Key, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryParseValue(kvp.Value, out double value))
                        {
                            sum += value;
                        }
                    }
                }
            }
            
            return Math.Round(sum, 2);
        }

        private double CalculateAverageForColumn(string columnName, List<TableResult> tables)
        {
            var values = new List<double>();
            
            foreach (var table in tables)
            {
                foreach (var kvp in table.Data)
                {
                    if (string.Equals(kvp.Key, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryParseValue(kvp.Value, out double value))
                        {
                            values.Add(value);
                        }
                    }
                }
            }
            
            return values.Any() ? Math.Round(values.Average(), 2) : 0;
        }

        private double CalculateMaxForColumn(string columnName, List<TableResult> tables)
        {
            var values = new List<double>();
            
            foreach (var table in tables)
            {
                foreach (var kvp in table.Data)
                {
                    if (string.Equals(kvp.Key, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryParseValue(kvp.Value, out double value))
                        {
                            values.Add(value);
                        }
                    }
                }
            }
            
            return values.Any() ? Math.Round(values.Max(), 2) : 0;
        }

        private double CalculateMinForColumn(string columnName, List<TableResult> tables)
        {
            var values = new List<double>();
            
            foreach (var table in tables)
            {
                foreach (var kvp in table.Data)
                {
                    if (string.Equals(kvp.Key, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryParseValue(kvp.Value, out double value))
                        {
                            values.Add(value);
                        }
                    }
                }
            }
            
            return values.Any() ? Math.Round(values.Min(), 2) : 0;
        }

        private double? FindExactValue(string key, List<TableResult> tables)
        {
            double? lastValue = null;
            
            foreach (var table in tables)
            {
                foreach (var kvp in table.Data)
                {
                    if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryParseValue(kvp.Value, out double value))
                        {
                            lastValue = value;
                        }
                    }
                }
            }
            
            return lastValue;
        }

        private double FindPartialValue(string key, List<TableResult> tables)
        {
            foreach (var table in tables)
            {
                foreach (var kvp in table.Data)
                {
                    if (kvp.Key.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        key.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (TryParseValue(kvp.Value, out double value))
                        {
                            return value;
                        }
                    }
                }
            }
            
            return 0;
        }

        private double EvaluateComplexFormula(string formula, List<TableResult> tables, Dictionary<string, string>? mappings)
        {
            var variables = ExtractVariables(formula);
            var variableValues = new Dictionary<string, double>();
            
            foreach (var variable in variables)
            {
                string lookupKey = variable;
                if (mappings != null && mappings.TryGetValue(variable, out var mappedName))
                {
                    lookupKey = mappedName;
                }
                
                variableValues[variable] = FindExactValue(lookupKey, tables) ?? FindPartialValue(lookupKey, tables);
            }
            
            return EvaluateExpression(formula, variableValues);
        }

        private List<string> ExtractVariables(string formula)
        {
            var cleaned = Regex.Replace(formula, @"[\d\+\-\*/\(\)]", " ");
            var words = cleaned.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            return words
                .Where(w => !string.IsNullOrWhiteSpace(w) && w.Length > 1 && !IsNumeric(w))
                .Distinct()
                .ToList();
        }

        private double EvaluateExpression(string expression, Dictionary<string, double>? variables = null)
        {
            string expr = expression;
            
            if (variables != null)
            {
                foreach (var variable in variables)
                {
                    expr = expr.Replace(variable.Key, 
                        variable.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            
            expr = expr.Replace(',', '.');
            
            expr = expr.Replace(" ", "");
            
            try
            {
                using DataTable table = new DataTable();
                table.Columns.Add("expr", typeof(string), expr);
                DataRow row = table.NewRow();
                table.Rows.Add(row);
                
                return Convert.ToDouble(row["expr"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка вычисления выражения '{expr}': {ex.Message}");
                return 0;
            }
        }

        private bool TryParseValue(string value, out double result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string cleanValue = value.Trim()
                .Replace(',', '.')
                .Replace(" ", "")
                .Replace("\u00A0", "");
                
            return double.TryParse(cleanValue, 
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out result);
        }

        private bool ContainsOperators(string s) =>
            s.Contains('+') || s.Contains('-') || s.Contains('*') || s.Contains('/');

        private bool IsNumericExpression(string s) => TryParseValue(s, out _);

        private bool IsNumeric(string s) => TryParseValue(s, out _);
    }
}
