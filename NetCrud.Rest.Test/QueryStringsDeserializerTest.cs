using NetCrud.Rest.Core;
using System;
using Xunit;

namespace NetCrud.Rest.Test
{
    public class QueryStringsDeserializerTest
    {
        [Fact]
        public void Verify_valid_func()
        {
            Assert.Throws<Exception>(() =>
            {
                QueryStringDeserialzer.Deserialize("xxx(a,b)");
            });
        }

        [Fact]
        public void And_a_b()
        {
            var result = QueryStringDeserialzer.Deserialize("and(a,b)");
            Assert.Equal("(A && B)", result);
        }

        [Fact]
        public void And_a_b_c()
        {
            var result = QueryStringDeserialzer.Deserialize("and(a,and(b,c))");
            Assert.Equal("(A && (B && C))", result);
        }

        [Fact]
        public void And_or_a_b_c_d()
        {
            var result = QueryStringDeserialzer.Deserialize("and(or(a,d),or(b,c))");
            Assert.Equal("((A || D) && (B || C))", result);
        }

        [Fact]
        public void And_or_a_b_c_d_g()
        {
            var result = QueryStringDeserialzer.Deserialize("and(g,and(or(a,d),or(b,c)))");
            Assert.Equal("(G && ((A || D) && (B || C)))", result);
        }

        [Fact]
        public void Equals_displayName_with_null()
        {
            var result = QueryStringDeserialzer.Deserialize("equals(displayName,null)");
            Assert.Equal("(DisplayName == Null)", result);
        }

        [Fact]
        public void Equals_displayName_with_lastName()
        {
            var result = QueryStringDeserialzer.Deserialize("equals(displayName,lastName)");
            Assert.Equal("(DisplayName == LastName)", result);
        }

        [Fact]
        public void Equals_displayName_with_string()
        {
            var result = QueryStringDeserialzer.Deserialize("equals(displayName,'Brian OConnor')");
            Assert.Equal("(DisplayName == \"Brian OConnor\")", result);
        }

        [Fact]
        public void Equals_And_Equals()
        {
            var result = QueryStringDeserialzer.Deserialize("and(equals(firstName,'brian'),equals(lastName,'OConnor'))");
            Assert.Equal("((FirstName == \"brian\") && (LastName == \"OConnor\"))", result);
        }

        [Fact]
        public void Verify_LessThan_Function()
        {
            var result = QueryStringDeserialzer.Deserialize("lessThan(publishTime,'2005-05-05')");
            Assert.Equal("(PublishTime < \"2005-05-05\")", result);
        }
    }
}
