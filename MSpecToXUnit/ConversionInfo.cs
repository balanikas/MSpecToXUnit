using System;
using System.Collections.Generic;
using System.Text;

namespace MSpecToXUnit
{
    enum ConversionStatus
    {
        ParsingError,
        ConversionError,
        NoTestsFound,
        SuccessfulConversion
    }

    class ConversionInfo
    {
        public string FileToConvert { get; set; }
        public string OutputFile { get; set; }
        public ConversionStatus Status { get; set; }

        public int TestCount { get; set; }
        public List<TestMetaData> ParsedClasses { get; set; }
        public List<string> ConvertedClasses { get; set; }
        public string Usings { get; set; }
        public string NameSpace { get; set; }

        public ConversionInfo()
        {
            ParsedClasses = new List<TestMetaData>();
            ConvertedClasses = new List<string>();
        }

        public string ToFormattedString()
        {
            var sb = new StringBuilder();
            var allCode = sb
                .Append(Usings)
                .Append("using Xunit;")
                .Append("using FluentAssertions;")
                .Append("namespace " + NameSpace)
                .Append("{\n")
                .Append(String.Join(String.Empty, ConvertedClasses))
                .Append("}")
                .ToString();

            return TestConverter.Prettify(allCode);
        }
    }
}