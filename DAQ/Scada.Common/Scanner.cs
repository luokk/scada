///
/// Geoffrey Slinker
///
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Scada.Common
{
	/// <summary>
	/// Summary description for Scanner.
	/// </summary>
	public class Scanner
	{
		protected static Dictionary<string, string> patterns;

		static Scanner()
		{
			Scanner.patterns = new Dictionary<string, string>();

			patterns.Add("String", @"[\w\d\S]+");
			patterns.Add("Int16", @"-[0-9]+|[0-9]+");
			patterns.Add("UInt16", @"[0-9]+");
			patterns.Add("Int32", @"-[0-9]+|[0-9]+");
			patterns.Add("UInt32", @"[0-9]+");
			patterns.Add("Int64", @"-[0-9]+|[0-9]+");
			patterns.Add("UInt64", @"[0-9]+");
			patterns.Add("Single", @"[-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+");
			patterns.Add("Double", @"[-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+");
			patterns.Add("Boolean", @"true|false");
			patterns.Add("Byte", @"[0-9]{1,3}");
			patterns.Add("SByte", @"-[0-9]{1,3}|[0-9]{1,3}");
			patterns.Add("Char", @"[\w\S]{1}");
			patterns.Add("Decimal", @"[-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+");
		}

		public Scanner()
		{
		}

		/// <summary>
		/// Scan memics scanf.
		/// A master regular expression pattern is created that will group each "word" in the text and using regex grouping
		/// extract the values for the field specifications.
		/// Example text: "Hello true 6.5"  fieldSpecification: "{String} {Boolean} {Double}"
		/// The fieldSpecification will result in the generation of a master Pattern:
		/// ([\w\d\S]+)\s+(true|false)\s+([-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+)
		/// This masterPattern is ran against the text string and the groups are extracted.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="pattern">A string that may contain simple field specifications of the form {Int16}, {String}, etc</param>
		/// <returns>object[] that contains values for each field</returns>
		public object[] ScanObject(string text, string pattern)
		{
			// string pattern;
			object[] targets = null;
			try
			{
				ArrayList targetMatchGroups = new ArrayList();
				ArrayList targetTypes = new ArrayList();

				string matchingPattern = "";
				Regex reggie = null;
				MatchCollection matches = null;

				//masterPattern is going to hold a "big" regex pattern that will be ran against the original text
				string masterPattern = pattern.Trim();
				matchingPattern = @"(\S+)";
				masterPattern = Regex.Replace(masterPattern, matchingPattern, "($1)");		//insert grouping parens

				//store the group location of the format tags so that we can select the correct group values later.
				matchingPattern = @"(\([\w\d\S]+\))";
				reggie = new Regex(matchingPattern);
				matches = reggie.Matches(masterPattern);
				for (int i = 0; i < matches.Count; i++)
				{
					Match m = matches[i];
					string sVal = m.Groups[1].Captures[0].Value;

					//is this value a {n} value. We will determine this by checking for {
					if (sVal.IndexOf('{') >= 0)
					{
						targetMatchGroups.Add(i);
						string p = @"\(\{(\w*)\}\)";	//pull out the type
						sVal = Regex.Replace(sVal, p, "$1");
						targetTypes.Add(sVal);
					}
				}

				//Replace all of the types with the pattern that matches that type
				masterPattern = Regex.Replace(masterPattern, @"\{String\}", (String)patterns["String"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Int16\}", (String)patterns["Int16"]);
				masterPattern = Regex.Replace(masterPattern, @"\{UInt16\}", (String)patterns["UInt16"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Int32\}", (String)patterns["Int32"]);
				masterPattern = Regex.Replace(masterPattern, @"\{UInt32\}", (String)patterns["UInt32"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Int64\}", (String)patterns["Int64"]);
				masterPattern = Regex.Replace(masterPattern, @"\{UInt64\}", (String)patterns["UInt64"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Single\}", (String)patterns["Single"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Double\}", (String)patterns["Double"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Boolean\}", (String)patterns["Boolean"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Byte\}", (String)patterns["Byte"]);
				masterPattern = Regex.Replace(masterPattern, @"\{SByte\}", (String)patterns["SByte"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Char\}", (String)patterns["Char"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Decimal\}", (String)patterns["Decimal"]);

				masterPattern = Regex.Replace(masterPattern, @"\s+", "\\s+");	//replace the white space with the pattern for white space

				//run our generated pattern against the original text.
				reggie = new Regex(masterPattern);
				matches = reggie.Matches(text);
				//PrintMatches(matches);

				//allocate the targets
				targets = new object[targetMatchGroups.Count];
				for (int x = 0; x < targetMatchGroups.Count; x++)
				{
					int i = (int)targetMatchGroups[x];
					string tName = (string)targetTypes[x];
					if (i < matches[0].Groups.Count)
					{
						//add 1 to i because i is a result of serveral matches each resulting in one group.
						//this query is one match resulting in serveral groups.
						string sValue = matches[0].Groups[i + 1].Captures[0].Value;
						targets[x] = ReturnValue(tName, sValue);
					}
				}
			}
			catch (Exception ex)
			{
				throw new ScannerExeption("Scan exception", ex);
			}

			return targets;
		}//Scan


		/// <summary>
		/// Scan memics scanf.
		/// A master regular expression pattern is created that will group each "word" in the text and using regex grouping
		/// extract the values for the field specifications.
		/// Example text: "Hello true 6.5"  fieldSpecification: "{String} {Boolean} {Double}"
		/// The fieldSpecification will result in the generation of a master Pattern:
		/// ([\w\d\S]+)\s+(true|false)\s+([-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+)
		/// This masterPattern is ran against the text string and the groups are extracted.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="pattern">A string that may contain simple field specifications of the form {Int16}, {String}, etc</param>
		/// <returns>object[] that contains values for each field</returns>
		public string[] Scan(string text, string pattern)
		{
			// string pattern;
			string[] targets = null;
			try
			{
				ArrayList targetMatchGroups = new ArrayList();
				ArrayList targetTypes = new ArrayList();

				//masterPattern is going to hold a "big" regex pattern that will be ran against the original text
				string masterPattern = pattern.Trim();
				string matchingPattern = @"({[\w\d]+})";
				masterPattern = Regex.Replace(masterPattern, matchingPattern, "($1)");		//insert grouping parens
                
				//store the group location of the format tags so that we can select the correct group values later.
				matchingPattern = @"(\([{}\w\d]+\))";
				Regex reggie = new Regex(matchingPattern);
				MatchCollection matches = reggie.Matches(masterPattern);
				for (int i = 0; i < matches.Count; i++)
				{
					Match m = matches[i];
					string sVal = m.Groups[1].Captures[0].Value;

					//is this value a {n} value. We will determine this by checking for {
					if (sVal.IndexOf('{') >= 0)
					{
						targetMatchGroups.Add(i);
						string p = @"\(\{(\w*)\}\)";	//pull out the type
						sVal = Regex.Replace(sVal, p, "$1");
						targetTypes.Add(sVal);
					}
				}

				//Replace all of the types with the pattern that matches that type
				masterPattern = Regex.Replace(masterPattern, @"\{String\}", (String)patterns["String"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Int16\}", (String)patterns["Int16"]);
				masterPattern = Regex.Replace(masterPattern, @"\{UInt16\}", (String)patterns["UInt16"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Int32\}", (String)patterns["Int32"]);
				masterPattern = Regex.Replace(masterPattern, @"\{UInt32\}", (String)patterns["UInt32"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Int64\}", (String)patterns["Int64"]);
				masterPattern = Regex.Replace(masterPattern, @"\{UInt64\}", (String)patterns["UInt64"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Single\}", (String)patterns["Single"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Double\}", (String)patterns["Double"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Boolean\}", (String)patterns["Boolean"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Byte\}", (String)patterns["Byte"]);
				masterPattern = Regex.Replace(masterPattern, @"\{SByte\}", (String)patterns["SByte"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Char\}", (String)patterns["Char"]);
				masterPattern = Regex.Replace(masterPattern, @"\{Decimal\}", (String)patterns["Decimal"]);

				masterPattern = Regex.Replace(masterPattern, @"\s+", "\\s+");	//replace the white space with the pattern for white space

				//run our generated pattern against the original text.
				reggie = new Regex(masterPattern);
				matches = reggie.Matches(text);
				//PrintMatches(matches);

				//allocate the targets
				targets = new string[targetMatchGroups.Count];
				for (int x = 0; x < targetMatchGroups.Count; x++)
				{
					int i = (int)targetMatchGroups[x];
					string tName = (string)targetTypes[x];
					if (matches.Count > 0)
					{
						if (i < matches[0].Groups.Count)
						{
							//add 1 to i because i is a result of serveral matches each resulting in one group.
							//this query is one match resulting in serveral groups.
							string matched = matches[0].Groups[i + 1].Captures[0].Value;
							targets[x] = matched;
						}
					}

				}
			}
			catch (Exception ex)
			{
				// throw new ScannerExeption("Scan exception", ex);
			}

			return targets;
		}

		/// Scan memics scanf.
		/// A master regular expression pattern is created that will group each "word" in the text and using regex grouping
		/// extract the values for the field specifications.
		/// Example text: "Hello true 6.5"  fieldSpecification: "{0} {1} {2}" and the target array has objects of these types: "String, ,Boolean, Double"
		/// The targets are scanned and each target type is extracted in order to build a master pattern based on these types
		/// The fieldSpecification and target types will result in the generation of a master Pattern:
		/// ([\w\d\S]+)\s+(true|false)\s+([-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+)
		/// This masterPattern is ran against the text string and the groups are extracted and placed back into the targets
		/// <param name="text"></param>
		/// <param name="fieldSpecification"></param>
		/// <param name="targets"></param>
		public void Scan(string text, string fieldSpecification, params object[] targets)
		{
			try
			{
				ArrayList targetMatchGroups = new ArrayList();

				string matchingPattern = "";
				Regex reggie = null;
				MatchCollection matches = null;

				//masterPattern is going to hold a "big" regex pattern that will be ran against the original text
				string masterPattern = fieldSpecification.Trim();
				matchingPattern = @"(\S+)";
				masterPattern = Regex.Replace(masterPattern, matchingPattern, "($1)");		//insert grouping parens

				//store the group location of the format tags so that we can select the correct group values later.
				matchingPattern = @"(\([\w\d\S]+\))";
				reggie = new Regex(matchingPattern);
				matches = reggie.Matches(masterPattern);
				for (int i = 0; i < matches.Count; i++)
				{
					Match m = matches[i];
					string sVal = m.Groups[1].Captures[0].Value;

					//is this value a {n} value. We will determine this by checking for {
					if (sVal.IndexOf('{') >= 0)
					{
						targetMatchGroups.Add(i);
					}
				}

				matchingPattern = @"(\{\S+\})";	//match each paramter tag of the format {n} where n is a digit
				reggie = new Regex(matchingPattern);
				matches = reggie.Matches(masterPattern);

				for (int i = 0; i < targets.Length && i < matches.Count; i++)
				{
					string groupID = String.Format("${0}", (i + 1));
					string innerPattern = "";

					Type t = targets[i].GetType();
					innerPattern = ReturnPattern(t.Name);

					//replace the {n} with the type's pattern
					string groupPattern = "\\{" + i + "\\}";
					masterPattern = Regex.Replace(masterPattern, groupPattern, innerPattern);
				}

				masterPattern = Regex.Replace(masterPattern, @"\s+", "\\s+");	//replace white space with the whitespace pattern

				//run our generated pattern against the original text.
				reggie = new Regex(masterPattern);
				matches = reggie.Matches(text);
				for (int x = 0; x < targetMatchGroups.Count; x++)
				{
					int i = (int)targetMatchGroups[x];
					if (i < matches[0].Groups.Count)
					{
						//add 1 to i because i is a result of serveral matches each resulting in one group.
						//this query is one match resulting in serveral groups.
						string sValue = matches[0].Groups[i + 1].Captures[0].Value;
						Type t = targets[x].GetType();
						targets[x] = ReturnValue(t.Name, sValue);
					}
				}
			}
			catch (Exception ex)
			{
				throw new ScannerExeption("Scan exception", ex);
			}
		}	//Scan

		/// <summary>
		/// Return the Value inside of an object that boxes the built in type or references the string
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="sValue"></param>
		/// <returns></returns>
		private object ReturnValue(string typeName, string sValue)
		{
			object o = null;
			switch (typeName)
			{
				case "String":
					o = sValue;
					break;

				case "Int16":
					o = Int16.Parse(sValue);
					break;

				case "UInt16":
					o = UInt16.Parse(sValue);
					break;

				case "Int32":
					o = Int32.Parse(sValue);
					break;

				case "UInt32":
					o = UInt32.Parse(sValue);
					break;

				case "Int64":
					o = Int64.Parse(sValue);
					break;

				case "UInt64":
					o = UInt64.Parse(sValue);
					break;

				case "Single":
					o = Single.Parse(sValue);
					break;

				case "Double":
					o = Double.Parse(sValue);
					break;

				case "Boolean":
					o = Boolean.Parse(sValue);
					break;

				case "Byte":
					o = Byte.Parse(sValue);
					break;

				case "SByte":
					o = SByte.Parse(sValue);
					break;

				case "Char":
					o = Char.Parse(sValue);
					break;

				case "Decimal":
					o = Decimal.Parse(sValue);
					break;
			}
			return o;
		}//ReturnValue

		/// <summary>
		/// Return a pattern for regular expressions that will match the built in type specified by name
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		private string ReturnPattern(string typeName)
		{
			string innerPattern = "";
			switch (typeName)
			{
				case "Int16":
					innerPattern = (String)patterns["Int16"];
					break;

				case "UInt16":
					innerPattern = (String)patterns["UInt16"];
					break;

				case "Int32":
					innerPattern = (String)patterns["Int32"];
					break;

				case "UInt32":
					innerPattern = (String)patterns["UInt32"];
					break;

				case "Int64":
					innerPattern = (String)patterns["Int64"];
					break;

				case "UInt64":
					innerPattern = (String)patterns["UInt64"];
					break;

				case "Single":
					innerPattern = (String)patterns["Single"];
					break;

				case "Double":
					innerPattern = (String)patterns["Double"];
					break;

				case "Boolean":
					innerPattern = (String)patterns["Boolean"];
					break;

				case "Byte":
					innerPattern = (String)patterns["Byte"];
					break;

				case "SByte":
					innerPattern = (String)patterns["SByte"];
					break;

				case "Char":
					innerPattern = (String)patterns["Char"];
					break;

				case "Decimal":
					innerPattern = (String)patterns["Decimal"];
					break;

				case "String":
					innerPattern = (String)patterns["String"];
					break;
			}
			return innerPattern;
		}	//ReturnPattern

		static void PrintMatches(MatchCollection matches)
		{
			Console.WriteLine("===---===---===---===");
			int matchCount = 0;
			Console.WriteLine("Match Count = " + matches.Count);
			foreach (Match m in matches)
			{
				if (m == Match.Empty) Console.WriteLine("Empty match");
				Console.WriteLine("Match" + (++matchCount));
				for (int i = 0; i < m.Groups.Count; i++)
				{
					Group g = m.Groups[i];
					Console.WriteLine("Group" + i + "='" + g + "'");
					CaptureCollection cc = g.Captures;
					for (int j = 0; j < cc.Count; j++)
					{
						Capture c = cc[j];
						System.Console.Write("Capture" + j + "='" + c + "', Position=" + c.Index + "   <");
						for (int k = 0; k < c.ToString().Length; k++)
						{
							Console.Write(((Int32)(c.ToString()[k])));
						}
						Console.WriteLine(">");
					}
				}
			}
		}
	}

	/// <summary>
	/// Exceptions that are thrown by this namespace and the Scanner Class
	/// </summary>
	class ScannerExeption : Exception
	{
		public ScannerExeption()
			: base()
		{
		}

		public ScannerExeption(string message)
			: base(message)
		{
		}

		public ScannerExeption(string message, Exception inner)
			: base(message, inner)
		{
		}

		public ScannerExeption(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
