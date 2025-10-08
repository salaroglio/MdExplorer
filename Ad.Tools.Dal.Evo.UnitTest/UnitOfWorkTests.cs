using Ad.Tools.Dal.Evo;
using Ad.Tools.Dal.Evo.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NHibernate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Evo.UnitTest
{
    [TestClass]
    public class UnitOfWorkTests
    {
        // Define a simple entity for testing
        public class TestEntity { public virtual int Id { get; set; } }

        private Mock<ISessionFactory> _mockSessionFactory = null!;
        private Mock<ISession> _mockSession = null!;
        private Mock<ITransaction> _mockTransaction = null!;
        private UnitOfWork _unitOfWork = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockSessionFactory = new Mock<ISessionFactory>();
            _mockSession = new Mock<ISession>();
            _mockTransaction = new Mock<ITransaction>();

            // Setup SessionFactory to return the mock Session
            _mockSessionFactory.Setup(sf => sf.OpenSession()).Returns(_mockSession.Object);

            // Setup Session to return the mock Transaction when BeginTransaction is called
            _mockSession.Setup(s => s.BeginTransaction()).Returns(_mockTransaction.Object);

            // Setup Transaction properties/methods used by UnitOfWork
            _mockTransaction.Setup(t => t.IsActive).Returns(true); // Assume active after BeginTransaction

            _unitOfWork = new UnitOfWork(_mockSessionFactory.Object);
        }

        [TestMethod]
        public void GetRepository_ShouldReturnRepositoryInstance()
        {
            // Act
            var repository = _unitOfWork.GetRepository<TestEntity>();

            // Assert
            Assert.IsNotNull(repository);
            Assert.IsInstanceOfType(repository, typeof(IRepository<TestEntity>));
        }

        [TestMethod]
        public void GetRepository_ShouldReturnSameInstanceForSameType()
        {
            // Act
            var repository1 = _unitOfWork.GetRepository<TestEntity>();
            var repository2 = _unitOfWork.GetRepository<TestEntity>();

            // Assert
            Assert.IsNotNull(repository1);
            Assert.AreSame(repository1, repository2, "Should return the cached repository instance");
        }

        [TestMethod]
        public void BeginTransaction_ShouldBeginTransactionOnSession()
        {
            // Act
            _unitOfWork.BeginTransaction();

            // Assert
            _mockSession.Verify(s => s.BeginTransaction(), Times.Once);
        }

        [TestMethod]
        public async Task CommitAsync_ShouldCommitActiveTransaction()
        {
            // Arrange
            _unitOfWork.BeginTransaction(); // Start an active transaction

            // Act
            await _unitOfWork.CommitAsync();

            // Assert
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockTransaction.Verify(t => t.Rollback(), Times.Never); // Ensure rollback wasn't called
            _mockTransaction.Verify(t => t.Dispose(), Times.Once); // Ensure transaction is disposed after commit
        }

         [TestMethod]
        public async Task CommitAsync_ShouldFlushSession_WhenNoExplicitTransaction()
        {
            // Arrange
            // No call to BeginTransaction()

            // Act
            await _unitOfWork.CommitAsync();

            // Assert
            _mockSession.Verify(s => s.FlushAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never); // No transaction commit
        }

        [TestMethod]
        public async Task CommitAsync_ShouldRollbackAndThrow_WhenCommitFails()
        {
            // Arrange
            _unitOfWork.BeginTransaction();
            var commitException = new InvalidOperationException("Commit failed");
            _mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                            .ThrowsAsync(commitException);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await _unitOfWork.CommitAsync();
            });

            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockTransaction.Verify(t => t.Rollback(), Times.Once); // Ensure rollback was called on failure
            _mockTransaction.Verify(t => t.Dispose(), Times.Once); // Ensure transaction is disposed even on failure
        }

        [TestMethod]
        public void Rollback_ShouldRollbackActiveTransaction()
        {
            // Arrange
            _unitOfWork.BeginTransaction(); // Start an active transaction

            // Act
            _unitOfWork.Rollback();

            // Assert
            _mockTransaction.Verify(t => t.Rollback(), Times.Once);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mockTransaction.Verify(t => t.Dispose(), Times.Once); // Ensure transaction is disposed after rollback
        }

         [TestMethod]
        public void Rollback_ShouldDoNothing_WhenNoActiveTransaction()
        {
            // Arrange
            // No call to BeginTransaction()

            // Act
            _unitOfWork.Rollback();

            // Assert
            _mockTransaction.Verify(t => t.Rollback(), Times.Never);
            _mockTransaction.Verify(t => t.Dispose(), Times.Never); // Transaction wasn't created or disposed
        }


        [TestMethod]
        public void Dispose_ShouldDisposeSessionAndActiveTransaction()
        {
            // Arrange
            _unitOfWork.BeginTransaction(); // Ensure there's an active transaction

            // Act
            _unitOfWork.Dispose();

            // Assert
            _mockTransaction.Verify(t => t.Rollback(), Times.Once); // Dispose should rollback active transactions
            _mockTransaction.Verify(t => t.Dispose(), Times.Once);
            _mockSession.Verify(s => s.Dispose(), Times.Once);
        }

        [TestMethod]
        public void Dispose_ShouldDisposeSession_WhenNoActiveTransaction()
        {
             // Arrange
            // No call to BeginTransaction()

            // Act
            _unitOfWork.Dispose();

            // Assert
            _mockTransaction.Verify(t => t.Rollback(), Times.Never);
            _mockTransaction.Verify(t => t.Dispose(), Times.Never); // Transaction wasn't created
            _mockSession.Verify(s => s.Dispose(), Times.Once);
        }

        [TestMethod]
        public void Dispose_CanBeCalledMultipleTimesSafely()
        {
            // Arrange
            _unitOfWork.Dispose(); // First dispose

            // Act
            Exception? caughtException = null;
            try
            {
                _unitOfWork.Dispose(); // Second dispose
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.IsNull(caughtException, "Dispose should be safe to call multiple times.");
            _mockSession.Verify(s => s.Dispose(), Times.Once); // Verify Dispose was still only called once effectively
        }
    }
}
