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

namespace Ad.Tools.Dal.UnitTest
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
            // The repository method calls Query<T>().ToListAsync(). We expect Query<T>() to be called.
            // The actual result of ToListAsync on the mocked IQueryable isn't the primary focus
            // of this *unit* test, but we can check if the count matches.
            var result = await _repository.GetAllAsync();

            // Assert
            _mockSession.Verify(s => s.Query<TestEntity>(), Times.Once);
            // Verify the result count matches the source list count.
             Assert.AreEqual(entities.Count, result.Count);
        }

        [TestMethod]
        public async Task FindAsync_ShouldReturnMatchingEntities()
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
            var expectedResult = entities.Where(predicate.Compile()).ToList(); // Expected result after filtering

            // Mock Query<T>()
            _mockSession.Setup(s => s.Query<TestEntity>()).Returns(queryableEntities);

            // Act
            // The repository method calls Query<T>().Where(predicate).ToListAsync()
            var result = await _repository.FindAsync(predicate);

            // Assert
            _mockSession.Verify(s => s.Query<TestEntity>(), Times.Once);
            // Verify the result count and content (basic check)
            Assert.AreEqual(expectedResult.Count, result.Count);
            Assert.IsTrue(result.All(r => r.Name == "Match"));
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
