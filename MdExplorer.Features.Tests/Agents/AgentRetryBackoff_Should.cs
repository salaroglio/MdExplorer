using System;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentRetryBackoff_Should
    {
        [TestMethod]
        public void Grow_the_delay_exponentially_from_the_base()
        {
            Assert.AreEqual(TimeSpan.FromSeconds(30), AgentRetryBackoff.DelayFor(1));
            Assert.AreEqual(TimeSpan.FromSeconds(60), AgentRetryBackoff.DelayFor(2));
            Assert.AreEqual(TimeSpan.FromSeconds(120), AgentRetryBackoff.DelayFor(3));
        }

        [TestMethod]
        public void Never_wait_less_than_the_base_even_for_a_zero_or_negative_attempt()
        {
            Assert.AreEqual(TimeSpan.FromSeconds(30), AgentRetryBackoff.DelayFor(0));
            Assert.AreEqual(TimeSpan.FromSeconds(30), AgentRetryBackoff.DelayFor(-5));
        }

        [TestMethod]
        public void Cap_the_delay_and_never_overflow_for_huge_attempts()
        {
            Assert.AreEqual(AgentRetryBackoff.Max, AgentRetryBackoff.DelayFor(100));
            Assert.AreEqual(AgentRetryBackoff.Max, AgentRetryBackoff.DelayFor(int.MaxValue));
        }
    }
}
