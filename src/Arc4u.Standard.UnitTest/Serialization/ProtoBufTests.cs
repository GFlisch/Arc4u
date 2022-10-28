using Arc4u.Serializer;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Arc4u.Standard.UnitTest.Serialization
{
    public class ProtoBufTests
    {

        private sealed class TestType
        {
            public int Property { get; set; }
        }

        /// <summary>
        /// Test race condition between model type updates for the same type. See https://github.com/GFlisch/Arc4u.Guidance.Doc/issues/44.
        /// </summary>
        [Fact]
        public void CheckRuntimeTypeModelConcurrencyAsync()
        {
            // arrange
            const int ParallelTasks = 5;
            var testInstances = new TestType[ParallelTasks];
            for (int i = 0; i < ParallelTasks; ++i)
                testInstances[i] = new TestType { Property = i };

            var serializer = new ProtoBufSerialization();
            var actions = new Action[ParallelTasks];
            for (int i = 0; i < ParallelTasks; ++i)
            {
                int index = i;  // binding context for the action is local to this loop
                actions[i] =
                    () =>
                      {
                          var blob = serializer.Serialize(testInstances[index]);
                          testInstances[index] = serializer.Deserialize<TestType>(blob);
                      };
            }


            // act
            Parallel.Invoke(actions);

            // assert
            for (int i = 0; i < ParallelTasks; ++i)
            {
                testInstances[i].Should().NotBeNull();
                testInstances[i].Property.Should().Be(i);
            }
        }

    }
}
