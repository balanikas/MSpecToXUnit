using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;

namespace MSpecToXUnit
{
    class TestConverter
    {
        public static string ConvertTest(TestMetaData testMetaData)
        {
            var result = string.IsNullOrEmpty(testMetaData.CleanupBody.Name) ? _testClassTemplate : _testClassWithCleanupTemplate;
            result = ReplacePart(result, "[CLASSNAME]", testMetaData.ClassName);
            result = ReplacePart(result, "[BASECLASSNAME]", testMetaData.GetBaseClassAsString());
            result = ReplacePart(result, "[CLASSATTRIBUTES]", testMetaData.GetClassAttributesAsString());
            result = ReplacePart(result, "[FIELDS]", string.Join("\n", testMetaData.Fields));
            result = ReplacePart(result, "[BECAUSE]", testMetaData.BecauseBody.Body);
            result = ReplacePart(result, "[ESTABLISH]", testMetaData.EstablishBody.Body);
            result = ReplacePart(result, "[ASSERT]", testMetaData.GetAssertsAsString());
            result = ReplacePart(result, "[METHODS]", string.Join("\n", testMetaData.Methods));
            result = ReplacePart(result, "[INNERCLASSES]", string.Join("\n", testMetaData.Classes));
            result = ReplacePart(result, "[DISPOSE]", testMetaData.CleanupBody.Body);
            result = ReplacePart(result, "[CLASSNAME]", testMetaData.ClassName);
            result = ReplacePart(result, "[TRAITS]", testMetaData.GetTagsAsString());
            result = ReplacePart(result, "[IGNORE]", testMetaData.GetIgnoreAsString());

            return Prettify(result);
        }

        public static string ConvertTestBase(TestMetaData testMetaData)
        {
            var result = string.IsNullOrEmpty(testMetaData.CleanupBody.Name) ? _testBaseClassTemplate : _testBaseClassWithCleanupTemplate;
            result = ReplacePart(result, "[CLASSNAME]", testMetaData.ClassName);
            result = ReplacePart(result, "[BASECLASSNAME]", testMetaData.GetBaseClassAsString());
            result = ReplacePart(result, "[CLASSATTRIBUTES]", testMetaData.GetClassAttributesAsString());
            result = ReplacePart(result, "[FIELDS]", string.Join("\n", testMetaData.Fields));
            result = ReplacePart(result, "[BECAUSE]", testMetaData.BecauseBody.Body);
            result = ReplacePart(result, "[ESTABLISH]", testMetaData.EstablishBody.Body);
            result = ReplacePart(result, "[METHODS]", string.Join("\n", testMetaData.Methods));
            result = ReplacePart(result, "[INNERCLASSES]", string.Join("\n", testMetaData.Classes));
            result = ReplacePart(result, "[DISPOSE]", testMetaData.CleanupBody.Body);
            result = ReplacePart(result, "[CLASSNAME]", testMetaData.ClassName);

            return Prettify(result);
        }

        public static string ReplacePart(string text, string part, string newPart)
        {
            newPart = newPart ?? "";
            var regex = new Regex(Regex.Escape(part));
            return regex.Replace(text, newPart, 1);
        }

        public static string Prettify(string code)
        {
            var workspace = new AdhocWorkspace();
            var syntaxNode = CSharpSyntaxTree.ParseText(code);
            var options = workspace.Options;
            options = options.WithChangedOption(CSharpFormattingOptions.IndentBlock, true);
            options = options.WithChangedOption(CSharpFormattingOptions.IndentBraces, false);

            var formattedNode = Formatter.Format(syntaxNode.GetRoot(), workspace, options);
            var formattedString = formattedNode.ToFullString();


            return Regex.Replace(formattedString, @"^\s+$[\r\n]*", "\r\n", RegexOptions.Multiline);
        }

        static string _testClassTemplate = @"
            [CLASSATTRIBUTES]
            public class [CLASSNAME] [BASECLASSNAME]
            {
                [FIELDS]

                [Fact([IGNORE]DisplayName = ""[CLASSNAME]"")]
                [TRAITS]
                public void Test()
                {
                    [ESTABLISH]
        
                    [BECAUSE]
        
                    [ASSERT]
                }

                [METHODS]
        
                [INNERCLASSES]
            }
        ";

        static string _testClassWithCleanupTemplate = @"
            [CLASSATTRIBUTES]
            public class [CLASSNAME] [BASECLASSNAME], IDisposable
            {
                [FIELDS]

                [Fact([IGNORE]DisplayName = ""[CLASSNAME]"")]
                [TRAITS]
                public void Test()
                {
                    [ESTABLISH]
        
                    [BECAUSE]
        
                    [ASSERT]
                }
        
                [METHODS]

                [INNERCLASSES]

                public void Dispose()
                {
                    [DISPOSE]
                }
            }
        ";

        static string _testBaseClassTemplate = @"
            [CLASSATTRIBUTES]
            public class [CLASSNAME] [BASECLASSNAME]
            {
                [FIELDS]
                
                public [CLASSNAME]()
                {
                    [ESTABLISH]
        
                    [BECAUSE]
                }
        
                [METHODS]

                [INNERCLASSES]
            }
        ";

        static string _testBaseClassWithCleanupTemplate = @"
            [CLASSATTRIBUTES]
            public class [CLASSNAME] [BASECLASSNAME], IDisposable
            {
                [FIELDS]
                
                public [CLASSNAME]()
                {
                    [ESTABLISH]
        
                    [BECAUSE]
                }
        
                [METHODS]

                [INNERCLASSES]

                public void Dispose()
                {
                    [DISPOSE]
                }
            }
        ";
       
    }
}