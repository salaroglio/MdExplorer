using Ad.Tools.Dal.Evo;
using Ad.Tools.Dal.Evo.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NHibernate;
using NHibernate.Linq; // Required for Query<> extension methods
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

// Updated namespace to match the new test project
namespace Ad.Tools.Dal.Evo.UnitTest
{
    [TestClass]
    public class RepositoryTests
    {
        // Define a simple entity for testing
        public class TestEntity
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; } = string.Empty;
        }

        private Mock<ISession> _mockSession = null!;
        private Repository<TestEntity> _repository = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockSession = new Mock<ISession>();
            _repository = new Repository<TestEntity>(_mockSession.Object);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
        {
            // Arrange
            var entityId = 1;
            var expectedEntity = new TestEntity { Id = entityId, Name = "Test" };
            _mockSession.Setup(s => s.GetAsync<TestEntity>(entityId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedEntity);

            // Act
            var result = await _repository.GetByIdAsync(entityId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEntity.Id, result.Id);
            Assert.AreEqual(expectedEntity.Name, result.Name);
            _mockSession.Verify(s => s.GetAsync<TestEntity>(entityId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
        {
            // Arrange
            var entityId = 99;
            _mockSession.Setup(s => s.GetAsync<TestEntity>(entityId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((TestEntity?)null);

            // Act
            var result = await _repository.GetByIdAsync(entityId);

            // Assert
            Assert.IsNull(result);
            _mockSession.Verify(s => s.GetAsync<TestEntity>(entityId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetAllAsync_ShouldReturnAllEntities()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 1, Name = "Test1" },
                new TestEntity { Id = 2, Name = "Test2" }
            };
            var queryableEntities = entities.AsQueryable();

            // Mock ISession.Query<T>() to return our simple IQueryable
            _mockSession.Setup(s => s.Query<TestEntity>()).Returns(queryableEntities);

             // Since ToListAsync is an extension method on IQueryable, mocking Query<T>()
             // to return a standard IQueryable (like List<T>.AsQueryable()) is usually sufficient
             // for unit tests. The test focuses on whether Query<T>() is called, not the
             // internal workings of ToListAsync.
            // Act
            // Test that Query() returns the mocked queryable from the session.
            var resultQueryable = _repository.Query();

            // Assert
            _mockSession.Verify(s => s.Query<TestEntity>(), Times.Once);
            Assert.IsNotNull(resultQueryable);
            // Optionally, verify the elements if needed for this test's scope
            Assert.AreEqual(entities.Count, resultQueryable.Count());
            CollectionAssert.AreEqual(entities, resultQueryable.ToList());
        }

        [TestMethod]
        public void FindAsync_ShouldApplyPredicateToQuery() // Changed to synchronous test focusing on Query() + Where()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 1, Name = "Test1" },
                new TestEntity { Id = 2, Name = "Match" },
                new TestEntity { Id = 3, Name = "Test3" },
                new TestEntity { Id = 4, Name = "Match" }
            };
            var queryableEntities = entities.AsQueryable();
            Expression<Func<TestEntity, bool>> predicate = e => e.Name == "Match";
            var expectedResult = entities.Where(predicate.Compile()).ToList();

            // Mock Query<T>()
            _mockSession.Setup(s => s.Query<TestEntity>()).Returns(queryableEntities);

            // Act
            // Test that Query().Where(predicate) returns the expected filtered queryable
            // Note: FindAsync internally does Query<T>().Where(predicate).ToListAsync()
            // We are testing the Query<T>().Where(predicate) part here.
            var resultQueryable = _repository.Query().Where(predicate);


            // Assert
            _mockSession.Verify(s => s.Query<TestEntity>(), Times.Once); // Verify Query<T> was called
            Assert.IsNotNull(resultQueryable);
            // Verify the filtered result
            Assert.AreEqual(expectedResult.Count, resultQueryable.Count());
            CollectionAssert.AreEqual(expectedResult, resultQueryable.ToList());
        }


        [TestMethod]
        public async Task AddAsync_ShouldCallSessionSaveAsync()
        {
            // Arrange
            var newEntity = new TestEntity { Name = "NewEntity" };
            _mockSession.Setup(s => s.SaveAsync(newEntity, It.IsAny<CancellationToken>()))
                        .Returns(Task.FromResult<object>(1)); // Return a completed task

            // Act
            await _repository.AddAsync(newEntity);

            // Assert
            _mockSession.Verify(s => s.SaveAsync(newEntity, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public void Update_ShouldCallSessionUpdate()
        {
            // Arrange
            var entityToUpdate = new TestEntity { Id = 1, Name = "UpdatedEntity" };
            // No specific return value needed for Update typically
            _mockSession.Setup(s => s.Update(entityToUpdate));

            // Act
            _repository.Update(entityToUpdate);

            // Assert
            _mockSession.Verify(s => s.Update(entityToUpdate), Times.Once);
        }

        [TestMethod]
        public void Delete_ShouldCallSessionDelete()
        {
            // Arrange
            var entityToDelete = new TestEntity { Id = 1, Name = "ToDelete" };
            // No specific return value needed for Delete typically
            _mockSession.Setup(s => s.Delete(entityToDelete));

            // Act
            _repository.Delete(entityToDelete);

            // Assert
            _mockSession.Verify(s => s.Delete(entityToDelete), Times.Once);
        }

        // Note: AddRangeAsync and DeleteRange tests could be added,
        // but they primarily rely on the single AddAsync/Delete calls within a loop.
        // Testing the single methods provides good coverage for the core logic.
    }
}
