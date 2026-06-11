using MdExplorer.Features.Services.Atlassian;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MdExplorer.Features.Tests.Atlassian
{
    [TestClass]
    public class JqlBuilderShould
    {
        [TestMethod]
        public void ScopeToCurrentUserAndOpenIssuesOrderedByUrgency()
        {
            var jql = JqlBuilder.MyOpenIssuesByUrgency(null);

            StringAssert.Contains(jql, "assignee = currentUser()");
            StringAssert.Contains(jql, "statusCategory != Done");
            StringAssert.Contains(jql, "ORDER BY priority DESC, duedate ASC");
        }

        [TestMethod]
        public void NotEmitProjectClauseWhenNoKeys()
        {
            var jql = JqlBuilder.MyOpenIssuesByUrgency(new List<string>());
            Assert.IsFalse(jql.Contains("project IN"), "no project clause when there are no keys");
        }

        [TestMethod]
        public void ScopeToProjectKeysWhenProvided()
        {
            var jql = JqlBuilder.MyOpenIssuesByUrgency(new[] { "BCO", "OFELIA" });
            StringAssert.Contains(jql, "project IN (\"BCO\", \"OFELIA\")");
        }

        [TestMethod]
        public void IgnoreBlankKeysAndTrim()
        {
            var jql = JqlBuilder.MyOpenIssuesByUrgency(new[] { "  BCO  ", "", "   ", null });
            StringAssert.Contains(jql, "project IN (\"BCO\")");
        }
    }
}
