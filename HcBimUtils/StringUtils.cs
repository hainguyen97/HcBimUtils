using System.Text ;
using System.Text.RegularExpressions ;

namespace HcBimUtils
{
   public static class StringUtils
   {
      public static int LastNumber(this string inputString)
      {
         int number = -1;
         if (!string.IsNullOrEmpty(inputString))
         {
            string lastNumber = Regex.Match(inputString, @"(?<=(\D|^))\d+(?=\D*$)", RegexOptions.RightToLeft).ToString();
            if (int.TryParse(lastNumber, out number))
            {
               return number;
            }
         }
         return number;
      }

      //Kiểm tra string sau khi remove sign và tolower
      public static bool IsEqual(this string text, string target)
      {
         text = text.RemoveSign().ToLower();
         target = target.RemoveSign().ToLower();
         return text == target;
      }
      public static bool IsContainFilter(this string text, string filter)
      {
         return text.Trim().RemoveSign().ToLower().Contains(filter.Trim().RemoveSign().ToLower());
      }

      public static bool IsValidFilename(string testName)
      {
         Regex containsABadCharacter = new Regex("["
               + Regex.Escape(new string(System.IO.Path.GetInvalidPathChars())) + "]");
         if (containsABadCharacter.IsMatch(testName)) { return false; };

         // other checks for UNC, drive-path format, etc

         return true;
      }


      public static string RemoveSign(this string s)
      {
         Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
         string temp = s.Normalize(NormalizationForm.FormD);
         return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
      }

      public static double StringToDouble(this string str)
      {
         double value = 0;
         try
         {
            value = Convert.ToDouble(str);
         }
         catch
         {
         }
         return value;
      }

      public static string Substring(this string @this, string from = null, string until = null, StringComparison comparison = StringComparison.InvariantCulture)
      {
         var fromLength = (from ?? string.Empty).Length;
         var startIndex = !string.IsNullOrEmpty(from)
               ? @this.IndexOf(from, comparison) + fromLength
               : 0;

         if (startIndex < fromLength) { throw new ArgumentException("from: Failed to find an instance of the first anchor"); }

         var endIndex = !string.IsNullOrEmpty(until)
         ? @this.IndexOf(until, startIndex, comparison)
         : @this.Length;

         if (endIndex < 0) { throw new ArgumentException("until: Failed to find an instance of the last anchor"); }

         var subString = @this.Substring(startIndex, endIndex - startIndex);
         return subString;
      }

      public static List<string> EverythingBetween(this string source, string start, string end)
      {
         var results = new List<string>();

         string pattern = string.Format(
               "{0}({1}){2}",
               Regex.Escape(start),
               ".+?",
                Regex.Escape(end));

         foreach (Match m in Regex.Matches(source, pattern))
         {
            results.Add(m.Groups[1].Value);
         }

         return results;
      }

      public static string StringBetweenCharacter(this string source, string start, string end)
      {
         string valueString = string.Empty;
         if (start.Length > 0 && end.Length > 0)
         {
            int st = source.IndexOf(start) + start.Length;
            int ed = source.IndexOf(end);
            if (st >= 0 && ed >= 0)
            {
               valueString = source.Substring(st, ed - st);
            }
         }
         return valueString;
      }

      public static string StringAfterCharacter(this string source, string after)
      {
         string valueString = string.Empty;
         if (after.Length > 0)
         {
            if (source.IndexOf(after) < 0)
            {
               return source;
            }
            valueString = source.Substring(source.IndexOf(after) + after.Length);
         }
         return valueString;
      }

      public static string StringBeforeCharacter(this string source, string after)
      {
         string valueString = string.Empty;
         if (after.Length > 0)
         {
            try
            {
               valueString = source.Substring(0, source.IndexOf(after));
            }
            catch
            {
               return valueString;
            }
         }

         return valueString;
      }

      public static bool IsNullOrEmpty(this string source)
      {
         bool valueBool = false;
         if (string.IsNullOrEmpty(source))
         {
            valueBool = true;
         }

         return valueBool;
      }
   }
}