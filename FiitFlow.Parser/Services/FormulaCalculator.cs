using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services
{
    public class FormulaCalculator
    {
        private Dictionary<string, double> _formulaResults = new Dictionary<string, double>();
        
        public Dictionary<string, double> CalculateFormulasForSubject(
            SubjectConfig subject,
            Dictionary<string, Dictionary<string, string>> subjectData,
            Dictionary<string, Dictionary<string, string>> allTableData)
        {
            var results = new Dictionary<string, double>();
            _formulaResults.Clear();
            
            foreach (var formula in subject.Formulas)
            {
                try
                {
                    var result = CalculateFormula(formula, subjectData, allTableData, results);
                    results[formula.Name] = result;
                    _formulaResults[$"{subject.Name}.{formula.Name}"] = result;
                }
                catch (Exception ex)
                {
                    results[formula.Name] = double.NaN;
                    _formulaResults[$"{subject.Name}.{formula.Name}"] = double.NaN;
                    throw new InvalidOperationException($"Ошибка в формуле '{formula.Name}': {ex.Message}", ex);
                }
            }
            
            return results;
        }
        
        private double CalculateFormula(
            FormulaConfig formula,
            Dictionary<string, Dictionary<string, string>> subjectData,
            Dictionary<string, Dictionary<string, string>> allTableData,
            Dictionary<string, double> currentResults)
        {
            var expression = formula.Expression;
            
            foreach (var formulaRef in formula.FormulaReferences)
            {
                var formulaPath = formulaRef.Value;
                if (_formulaResults.ContainsKey(formulaPath))
                {
                    expression = expression.Replace(formulaRef.Key, 
                        _formulaResults[formulaPath].ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                else
                {
                    throw new ArgumentException($"Не найдена формула для ссылки: {formulaPath}");
                }
            }
            
            foreach (var variable in formula.Variables)
            {
                var value = ResolveVariableValue(variable.Value, subjectData, allTableData);
                expression = expression.Replace(variable.Key, 
                    value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            
            return EvaluateExpression(expression);
        }
        
        private double ResolveVariableValue(
            string variablePath,
            Dictionary<string, Dictionary<string, string>> subjectData,
            Dictionary<string, Dictionary<string, string>> allTableData)
        {
            if (variablePath.Contains('.'))
            {
                var parts = variablePath.Split('.');
                var tableName = parts[0];
                var fieldName = parts[1];
                
                if (allTableData.ContainsKey(tableName) && 
                    allTableData[tableName].ContainsKey(fieldName))
                {
                    return ParseValue(allTableData[tableName][fieldName]);
                }
            }
            else
            {
                foreach (var tableData in subjectData.Values)
                {
                    if (tableData.ContainsKey(variablePath))
                    {
                        return ParseValue(tableData[variablePath]);
                    }
                }
                
                foreach (var tableData in allTableData.Values)
                {
                    if (tableData.ContainsKey(variablePath))
                    {
                        return ParseValue(tableData[variablePath]);
                    }
                }
            }
            
            throw new ArgumentException($"Не удалось найти значение для переменной: {variablePath}");
        }
        
        private double ParseValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
                
            var cleanValue = value.Replace(",", ".");
            
            if (double.TryParse(cleanValue, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            
            return 0;
        }
        
        private double EvaluateExpression(string expression)
        {
            try
            {
                var table = new DataTable();
                table.Columns.Add("expr", typeof(string), expression);
                var row = table.NewRow();
                table.Rows.Add(row);
                var result = row["expr"];
                
                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка вычисления выражения '{expression}': {ex.Message}");
            }
        }
    }
}
