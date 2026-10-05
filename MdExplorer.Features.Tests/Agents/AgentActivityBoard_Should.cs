using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentActivityBoard_Should
    {
        [TestMethod]
        public void Show_a_turn_while_it_runs_and_not_after()
        {
            var board = new AgentActivityBoard();

            var turn = board.Begin("account-manager", "/home/carlo/prj", "Copilot");
            var running = board.Running("/home/carlo/prj");
            Assert.AreEqual(1, running.Count);
            Assert.AreEqual("account-manager", running[0].AgentName);
            Assert.AreEqual("Copilot", running[0].Engine);

            turn.Dispose();
            Assert.AreEqual(0, board.Running("/home/carlo/prj").Count);
        }

        [TestMethod]
        public void Keep_the_turns_of_another_project_out()
        {
            var board = new AgentActivityBoard();
            board.Begin("tecnico", "/home/carlo/prj", "Copilot");
            board.Begin("legale", "/home/carlo/altro", "Copilot");

            CollectionAssert.AreEqual(new[] { "tecnico" }, board.Running("/home/carlo/prj/").Select(a => a.AgentName).ToList());
        }

        [TestMethod]
        public void Show_the_same_agent_twice_when_two_of_its_turns_run_together()
        {
            var board = new AgentActivityBoard();
            var first = board.Begin("tecnico", "/home/carlo/prj", "Copilot");
            board.Begin("tecnico", "/home/carlo/prj", "Copilot");

            Assert.AreEqual(2, board.Running("/home/carlo/prj").Count);
            first.Dispose();
            Assert.AreEqual(1, board.Running("/home/carlo/prj").Count, "ending one turn does not end the other");
        }

        [TestMethod]
        public void Tell_the_project_at_every_start_and_end_once()
        {
            var board = new AgentActivityBoard();
            var told = new List<string>();
            board.Changed += told.Add;

            var turn = board.Begin("tecnico", "/home/carlo/prj", "Copilot");
            turn.Dispose();
            turn.Dispose();

            CollectionAssert.AreEqual(new[] { "/home/carlo/prj", "/home/carlo/prj" }, told, "a turn disposed twice ends once");
        }
    }
}
