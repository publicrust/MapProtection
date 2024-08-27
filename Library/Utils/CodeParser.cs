using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Library.Utils
{
    internal class CodeParser
    {
        public static string ParseAndConvertToUnicode(string inputCode)
        {
            // Очистка текста от лишних символов, оставляя только \n в качестве разделителя строк
            string cleanedInput = inputCode.Replace("\r\n", "\n").Replace("\r", "\n");

            // Ищем место, где начинается "class MapProtection : RustPlugin"
            string pattern = @"class\s+MapProtection\s+:\s+RustPlugin";
            Match match = Regex.Match(cleanedInput, pattern);

            // Проверяем, найден ли класс
            if (match.Success)
            {
                // Находим конец строки с классом и берем текст после него
                int index = match.Index + match.Length;
                string beforeClass = cleanedInput.Substring(0, index);
                string afterClass = cleanedInput.Substring(index);

                // Убираем пробелы вокруг '{' и '}' и оставляем остальной текст неизменным
                string minimizedAfterClass = Regex.Replace(afterClass, @"\s*{\s*", "{");
                minimizedAfterClass = Regex.Replace(minimizedAfterClass, @"\s*}\s*", "}");

                // Преобразование оставшегося кода в Unicode, исключая скобки '{' и '}'
                StringBuilder unicodeBuilder = new StringBuilder();
                foreach (char c in minimizedAfterClass)
                {
                    if (c == '{' || c == '}')
                    {
                        unicodeBuilder.Append(c); // Оставляем скобки как есть
                    }
                    else
                    {
                        // Преобразуем остальные символы в Unicode
                        unicodeBuilder.AppendFormat("\\u{0:X4}", (int)c);
                    }
                }

                // Соединяем неизмененную часть с зашифрованной частью
                return beforeClass + unicodeBuilder.ToString();
            }

            // Если класс не найден, возвращаем оригинальный текст
            return cleanedInput;
        }
    }
}
