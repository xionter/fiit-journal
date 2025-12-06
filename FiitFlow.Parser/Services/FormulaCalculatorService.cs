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
                    // Парсим формулу
                    string formula = formulaEntry.Value;
                    
                    // Находим все переменные в формуле
                    var variables = ExtractVariables(formula);
                    
                    // Получаем значения переменных из таблиц
                    var variableValues = new Dictionary<string, double>();
                    foreach (var variable in variables)
                    {
                        string lookupKey = variable;
                        
                        // Применяем маппинг, если есть
                        if (valueMappings != null && valueMappings.ContainsKey(variable))
                        {
                            lookupKey = valueMappings[variable];
                        }
                        
                        variableValues[variable] = FindValueInTables(lookupKey, tables);
                    }
                    
                    // Вычисляем формулу
                    double result = EvaluateFormula(formula, variableValues);
                    results[formulaEntry.Key] = Math.Round(result, 2);
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
            List<TableResult> tables)
        {
            if (string.IsNullOrWhiteSpace(formula))
                return 0;

            try
            {
                // Находим все переменные в формуле
                var variables = ExtractVariables(formula);
                var variableValues = new Dictionary<string, double>();

                foreach (var variable in variables)
                {
                    // Проверяем, есть ли переменная в компонентах
                    if (components.ContainsKey(variable))
                    {
                        variableValues[variable] = components[variable];
                    }
                    else
                    {
                        // Ищем значение в таблицах
                        variableValues[variable] = FindValueInTables(variable, tables);
                    }
                }

                // Вычисляем формулу
                return Math.Round(EvaluateFormula(formula, variableValues), 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка расчета итогового балла: {ex.Message}");
                return 0;
            }
        }

        private List<string> ExtractVariables(string formula)
        {
            // Убираем числа и операторы, оставляем только переменные
            var variables = new List<string>();
            var regex = new Regex(@"[a-zA-Zа-яА-Я][a-zA-Zа-яА-Я0-9_]*");
            
            foreach (Match match in regex.Matches(formula))
            {
                // Проверяем, что это не функция и не число
                string value = match.Value;
                if (!IsNumeric(value) && !IsOperator(value))
                {
                    variables.Add(value);
                }
            }

            return variables.Distinct().ToList();
        }

        private double FindValueInTables(string key, List<TableResult> tables)
        {
            // Сначала ищем точное совпадение
            foreach (var table in tables)
            {
                if (table.Data.TryGetValue(key, out string value))
                {
                    if (TryParseValue(value, out double numericValue))
                        return numericValue;
                }
            }

            // Затем ищем частичное совпадение
            foreach (var table in tables)
            {
                foreach (var kvp in table.Data)
                {
                    if (kvp.Key.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                        key.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryParseValue(kvp.Value, out double numericValue))
                            return numericValue;
                    }
                }
            }

            // Ищем общие ключи
            var commonKeys = new[] { "Сумма", "Итого", "Всего", "Total", "Sum", "Итог" };
            foreach (var commonKey in commonKeys)
            {
                foreach (var table in tables)
                {
                    if (table.Data.TryGetValue(commonKey, out string value))
                    {
                        if (TryParseValue(value, out double numericValue))
                            return numericValue;
                    }
                }
            }

            return 0;
        }

        private double EvaluateFormula(string formula, Dictionary<string, double> variables)
        {
            // Заменяем переменные на значения
            string expression = formula;
            foreach (var variable in variables)
            {
                expression = Regex.Replace(
                    expression,
                    $@"\b{Regex.Escape(variable.Key)}\b",
                    variable.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    RegexOptions.IgnoreCase
                );
            }

            // Заменяем русские запятые на точки
            expression = expression.Replace(',', '.');

            // Используем DataTable для вычисления
            using DataTable table = new DataTable();
            table.Columns.Add("expr", typeof(string), expression);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            
            return Convert.ToDouble(row["expr"]);
        }

        private bool TryParseValue(string value, out double result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0;
                return false;
            }

            // Убираем лишние символы
            string cleanValue = value.Trim()
                .Replace(',', '.')
                .Replace(" ", "");

            return double.TryParse(cleanValue, 
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out result);
        }

        private bool IsNumeric(string s)
        {
            return double.TryParse(s, out _);
        }

        private bool IsOperator(string s)
        {
            var operators = new[] { "+", "-", "*", "/", "(", ")", "^", "%" };
            return operators.Contains(s);
        }
    }
}
