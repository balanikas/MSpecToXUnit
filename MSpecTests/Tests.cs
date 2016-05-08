#if !DEBUG
using System;
using Machine.Specifications;

namespace MSpecTests
{
    /// <summary>
    /// summary of HelperClass
    /// </summary>
    class HelperClass
    {
        public void Method() { }
    }

    //some comment

    class TestBase1
    {
        protected void Method1() { }
    }

    /// <summary>
    /// summary of TestBase2
    /// </summary>
    public class TestBase2 : TestBase1
    {
        Establish context = () => new object();
        Because of = () => new object();
        Cleanup cleanup = () => new object();

        protected void Method2() { }
    }

    /// <summary>
    /// summary of When_conatining_pragmas
    /// </summary>
    [Subject(typeof(object), "subject2")]
    class When_conatining_pragmas : TestBase2
    {
        Establish context = () => {
#pragma warning disable 618
            var var1 = 0;
#pragma warning restore 618

        };
        It should_something_1 = () => 1.ShouldEqual(1);
    }

    [Subject(typeof(object), "subject2")]
    class When_with_subject_typeof_and_tag : TestBase2
    {
        Establish context = () => new object();
        It should_something_1 = () => 1.ShouldEqual(1);
    }

    [Subject(typeof(object), "subject2, subject2")]
    class When_with_subject_typeof_and_multitag : TestBase2
    {
        Establish context = () => new object();
        It should_something_1 = () => 1.ShouldEqual(1);
    }

    [Subject(typeof(object))]
    class When_something_subject_typeof : TestBase2
    {
        Establish context = () => new object();
        It should_something_1 = () => 1.ShouldEqual(1);
    }

    [Tags("tag1"), Subject(typeof(object))]
    class When_something_tag_and_subject_typeof : TestBase2
    {
        Establish context = () => new object();
        It should_something_1 = () => 1.ShouldEqual(1);
    }

    [Subject(typeof(object))]
    class When_something_subject_tag : TestBase2
    {
        Establish context = () => new object();
        It should_something_1 = () => 1.ShouldEqual(1);
    }

    [Obsolete("msg")]
    [CLSCompliant(true)]
    class When_something_with_non_mspec_attribute 
    {
        Establish context = () => new object();
        It should_something_1 = () => 1.ShouldEqual(1);
    }    

    [Tags("tag1, tag2")]
    [Subject( typeof(object), "subject1, subject2")]
    [Tags("tag3")]
    //c1
    [Ignore("ignore")]
    class When_something_1 : TestBase2
    {
        static object _obj1;
        //c2
        private static object _obj2;
        private const int _obj3 = 1;
        /* c3 */
        Establish context = () =>
        {
            _obj1 = new object();
            _obj2 = new object(); // c4
            Method1();
        };

        private Because of = () =>
        {
            _obj1 = new object();
            _obj2 = new object();
            Catch.Exception(() => { throw new Exception(); });

        };

        It should_something_1 = () => _obj1.ShouldNotBeNull();
        private It should_something_2 = () =>
        {
            int x = 0;
            _obj2.ShouldEqual(x);
        };

        Cleanup cleanup = () =>
        {
            //c5
            //c6
            _obj1 = null;
#pragma warning disable 618
            _obj2 = null;
#pragma warning restore 618
        };

        //c6
        static void Method1() { }

        //c7

        class InternalClass { }
    }
 
}
#endif