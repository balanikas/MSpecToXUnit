using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MSpecToXUnit
{
    //todo:
    //convert Should extension to xunit asserts/fluentassertions?
    //preserve comments, not working 100%
    //convert methods of non test classes to non static?
    //bonus: remove tags with bugid "COm-" ?
    //a few tests are tagged as "NoTestsFound", because they are nested. handle it.
    class Program
    {
        static void Main(string[] args)
        {
            var folderToConvert = @"C:\Users\Kristoffer\Documents\xunitconverter\MSpecTests";
            var recursive = true;

            var potentialTestFiles = GetPotentialTestFiles(folderToConvert, recursive);
            Console.WriteLine("LISTING POTENTIAL TEST FILES");
            foreach (var potentialTestFile in potentialTestFiles)
            {
                Console.WriteLine(potentialTestFile);
            }

            var conversionList = CreateConversionList(potentialTestFiles);

            ParseAndConvert(conversionList);
            PrintResults(conversionList);
            WriteTests(conversionList);
        }

        static List<ConversionInfo> CreateConversionList(IEnumerable<string> potentialTestFiles)
        {
            var newFilePrefix = "XUNIT_";

            var conversionList = new List<ConversionInfo>();
            foreach (var potentialTestFile in potentialTestFiles)
            {
                conversionList.Add(new ConversionInfo
                {
                    FileToConvert = potentialTestFile,
                    OutputFile = Path.Combine(Path.GetDirectoryName(potentialTestFile), newFilePrefix + Path.GetFileName(potentialTestFile))
                });
            }

            return conversionList;
        }

        private static void PrintResults(List<ConversionInfo> conversions)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("CONVERTED TESTS: " + conversions.SelectMany(x => x.ParsedClasses).Count(x => x.IsTest()));

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("STATUS:");
            foreach (var conversion in conversions.OrderBy(x => x.Status))
            {
                Console.WriteLine(conversion.Status + ": " + conversion.FileToConvert);
            }
        }

        static void WriteTests(List<ConversionInfo> conversions)
        {
            Console.WriteLine(Environment.NewLine);
            foreach (var conversion in conversions.Where(x=> x.Status == ConversionStatus.SuccessfulConversion))
            {
                Console.WriteLine("WRITING " + conversion.FileToConvert);
                WriteTest(conversion);
            }
        }

        static void WriteTest(ConversionInfo conversionInfo)
        {
            File.WriteAllText(conversionInfo.OutputFile, conversionInfo.ToFormattedString());
        }

        static void ParseAndConvert(List<ConversionInfo> conversions)
        {
            foreach (var conversion in conversions)
            {
                Console.WriteLine(Environment.NewLine);
                try
                {
                    Console.WriteLine("PARSING " + conversion.FileToConvert);
                    conversion.ParsedClasses = ParseClasses(conversion);
                }
                catch (Exception e)
                {
                    conversion.Status = ConversionStatus.ParsingError;
                    Console.WriteLine("FAILED TO PARSE " +conversion.FileToConvert);
                    Console.Error.WriteLine(e);
                    continue;
                }

                if (conversion.ParsedClasses.Count(x => x.IsTest() || x.IsTestBaseClass()) == 0)
                {
                    conversion.Status = ConversionStatus.NoTestsFound;
                    Console.WriteLine("NO TEST OR TESTBASE DETECTED IN " + conversion.FileToConvert + " SKIPPING");
                    continue;
                }

                try
                {
                    Console.WriteLine("CONVERTING " + conversion.FileToConvert);
                    conversion.ConvertedClasses = ConvertClasses(conversion.ParsedClasses);
                    conversion.Status = ConversionStatus.SuccessfulConversion;
                }
                catch (Exception e)
                {
                    conversion.Status = ConversionStatus.ConversionError;
                    Console.WriteLine("FAILED TO CONVERT " + conversion.FileToConvert);
                    Console.Error.WriteLine(e);
                    continue;
                }
            }
        }

        static List<TestMetaData> ParseClasses(ConversionInfo conversionInfo)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(conversionInfo.FileToConvert));
            var root = syntaxTree.GetRoot();
            conversionInfo.Usings = ((CompilationUnitSyntax) root).Usings.ToString();
            var ns = root.ChildNodes().Single(x => x.Kind() == SyntaxKind.NamespaceDeclaration);
            conversionInfo.NameSpace = ((NamespaceDeclarationSyntax) ns).Name.ToString();
            var classes = ns.ChildNodes().Where(x => x.Kind() == SyntaxKind.ClassDeclaration).Cast<ClassDeclarationSyntax>();
            
            return classes.Select(TestParser.ParseClass).ToList();
        }

        static List<string> ConvertClasses(List<TestMetaData> testMetaData)
        {
            var classesToWrite = new List<string>();
            foreach (var metaData in testMetaData)
            {
                if (metaData.IsTest())
                {
                    classesToWrite.Add(TestConverter.ConvertTest(metaData));
                }
                else if (metaData.IsTestBaseClass())
                {
                    classesToWrite.Add(TestConverter.ConvertTestBase(metaData));
                }
                else
                {
                    classesToWrite.Add("public " + metaData.Syntax.ToString());
                }
            }
            
            return classesToWrite;
        }

        static IEnumerable<string> GetPotentialTestFiles(string targetDirectory, bool deepSearch)
        {
            var fileEntries = new List<string>();
            fileEntries.AddRange(Directory.GetFiles(targetDirectory, "*.cs"));

            if (!deepSearch)
            {
                return fileEntries;
            }

            var subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                fileEntries.AddRange(GetPotentialTestFiles(subdirectory, deepSearch));
            }

            return fileEntries;
        }

        static void WriteToExistingFile(List<string> tests, string filePath)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
            var root = tree.GetRoot();

            var classes = root.DescendantNodes().Where(x => x.Kind() == SyntaxKind.ClassDeclaration).Cast<ClassDeclarationSyntax>();

            var newClasses = CSharpSyntaxTree.ParseText(tests.First()).GetRoot().DescendantNodes().Where(x => x.Kind() == SyntaxKind.ClassDeclaration).ToList();

            root = root.InsertNodesAfter(classes.Single(), newClasses);

            File.WriteAllText(filePath, root.ToString());

        }
        
    }
}
