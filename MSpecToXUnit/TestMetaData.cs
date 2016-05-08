using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MSpecToXUnit
{
    class TestMetaData
    {
        public ClassDeclarationSyntax Syntax { get; set; }
        public string ClassName { get; set; }
        public string BaseClassName { get; set; }
        public DelegateMetaData EstablishBody { get; set; }
        public DelegateMetaData CleanupBody { get; set; }
        public List<DelegateMetaData> Asserts { get; set; }
        public DelegateMetaData BecauseBody { get; set; }
        public List<string> Fields { get; set; }
        public List<string> Methods { get; set; }
        public List<string> Classes { get; set; }
        public List<string> Tags { get; set; }
        public List<string> ClassAttributes { get; set; }
        public string Ignore { get; set; }
        public string ClassModifiers { get; set; }

        public TestMetaData()
        {
            Fields = new List<string>();
            Asserts = new List<DelegateMetaData>();
            Methods = new List<string>();
            Tags = new List<string>();
            Classes = new List<string>();
            ClassAttributes = new List<string>();
            BaseClassName = "";
        }

        public bool IsTest()
        {
            return Asserts.Any();
        }

        public bool IsTestBaseClass()
        {
            return !Asserts.Any() &&
                   (BecauseBody.Name != "" || EstablishBody.Name != "" || CleanupBody.Name != "");
        }

        public string GetBaseClassAsString()
        {
            return BaseClassName != null ? " : " + BaseClassName : null;
        }

        public string GetTagsAsString()
        {
            return Tags.Aggregate("", (current, tag) => current + (string.Format(@"[Trait(""Category"",""{0}"")]", tag) + Environment.NewLine));
        }

        public string GetIgnoreAsString()
        {
            return Ignore != null ? string.Format(@"Skip = ""{0}"", ", Ignore) : "";
        }

        public string GetAssertsAsString()
        {
            return Asserts.Aggregate("", (current, assert) => current + (string.Format(@"new Action(() => {{{0}}}).ShouldNotThrow(""{1}"");", assert.Body, assert.Name) + Environment.NewLine));
        }

        public string GetClassAttributesAsString()
        {
            return ClassAttributes.Aggregate("", (current, classAttribute) => current + ("[" + classAttribute + "]" + Environment.NewLine));
        }
    }
}