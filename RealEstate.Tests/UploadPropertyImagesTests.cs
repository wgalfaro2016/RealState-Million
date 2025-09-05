using FluentAssertions;
using MediatR;
using Moq;
using RealEstate.Application.Commands;
using RealEstate.Application.Dtos;
using RealEstate.Application.Interfaces;

namespace RealEstate.Tests
{
    [TestFixture]
    public class UploadPropertyImagesTests
    {
        private static Mock<IFileResource> FileMock(long length, string name = "f.jpg") {
            var m = new Mock<IFileResource>();
            m.SetupGet(x => x.Length).Returns(length);
            return m;
        }

        [Test]
        public async Task Should_return_0_and_not_call_dependencies_when_files_is_null() {
            // Arrange
            var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
            var mediator = new Mock<IMediator>(MockBehavior.Strict);

            var handler = new UploadPropertyImagesHandler(storage.Object, mediator.Object);

            var cmd = new UploadPropertyImagesCommand(Guid.NewGuid(), null!, null);

            // Act
            var processed = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            processed.Should().Be(0);
            storage.VerifyNoOtherCalls();
            mediator.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Should_return_0_and_not_call_dependencies_when_files_is_empty() {
            var storage = new Mock<IFileStorageService>(MockBehavior.Strict);
            var mediator = new Mock<IMediator>(MockBehavior.Strict);

            var handler = new UploadPropertyImagesHandler(storage.Object, mediator.Object);

            var cmd = new UploadPropertyImagesCommand(Guid.NewGuid(), new List<IFileResource>(), null);

            var processed = await handler.Handle(cmd, CancellationToken.None);

            processed.Should().Be(0);
            storage.VerifyNoOtherCalls();
            mediator.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Should_process_only_valid_files_and_mark_cover_by_index() {
            // Arrange
            var propertyId = Guid.NewGuid();

            var f0 = FileMock(length: 10).Object; 
            var f1 = FileMock(length: 0).Object;  
            var f2 = FileMock(length: 5).Object;  

            var files = new List<IFileResource> { f0, f1, f2 };

            var storage = new Mock<IFileStorageService>();
            storage.SetupSequence(s => s.SavePropertyImageAsync(propertyId, It.IsAny<IFileResource>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync("url-0") 
                   .ReturnsAsync("url-2"); 

            var mediator = new Mock<IMediator>();
            mediator.Setup(m => m.Send(It.IsAny<AddPropertyImageCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PropertyImageDto)null!);

            var handler = new UploadPropertyImagesHandler(storage.Object, mediator.Object);

            var cmd = new UploadPropertyImagesCommand(propertyId, files, CoverIndex: 0);

            // Act
            var processed = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            processed.Should().Be(2); 

            storage.Verify(s => s.SavePropertyImageAsync(propertyId, f0, It.IsAny<CancellationToken>()), Times.Once);
            storage.Verify(s => s.SavePropertyImageAsync(propertyId, f2, It.IsAny<CancellationToken>()), Times.Once);
            storage.Verify(s => s.SavePropertyImageAsync(propertyId, f1, It.IsAny<CancellationToken>()), Times.Never);

            mediator.Verify(m => m.Send(
                It.Is<AddPropertyImageCommand>(c =>
                    c.PropertyId == propertyId &&
                    c.Url == "url-0" &&
                    c.IsCover == true),
                It.IsAny<CancellationToken>()), Times.Once);

            mediator.Verify(m => m.Send(
                It.Is<AddPropertyImageCommand>(c =>
                    c.PropertyId == propertyId &&
                    c.Url == "url-2" &&
                    c.IsCover == false),
                It.IsAny<CancellationToken>()), Times.Once);

            mediator.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Should_not_mark_any_cover_when_cover_index_points_to_invalid_file() {
            // Arrange
            var propertyId = Guid.NewGuid();

            var f0 = FileMock(0).Object;  
            var f1 = FileMock(10).Object;  
            var files = new List<IFileResource> { f0, f1 };

            var storage = new Mock<IFileStorageService>();
            storage.Setup(s => s.SavePropertyImageAsync(propertyId, f1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync("url-1");

            var mediator = new Mock<IMediator>();
            mediator.Setup(m => m.Send(It.IsAny<AddPropertyImageCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PropertyImageDto)null!);

            var handler = new UploadPropertyImagesHandler(storage.Object, mediator.Object);

            var cmd = new UploadPropertyImagesCommand(propertyId, files, CoverIndex: 0);

            // Act
            var processed = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            processed.Should().Be(1);

            mediator.Verify(m => m.Send(
                It.Is<AddPropertyImageCommand>(c =>
                    c.PropertyId == propertyId &&
                    c.Url == "url-1" &&
                    c.IsCover == false), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Should_handle_cover_index_out_of_range_and_process_normally() {
            // Arrange
            var propertyId = Guid.NewGuid();
            var f0 = FileMock(10).Object;
            var f1 = FileMock(10).Object;

            var files = new List<IFileResource> { f0, f1 };

            var storage = new Mock<IFileStorageService>();
            storage.SetupSequence(s => s.SavePropertyImageAsync(propertyId, It.IsAny<IFileResource>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync("u0")
                   .ReturnsAsync("u1");

            var mediator = new Mock<IMediator>();
            mediator.Setup(m => m.Send(It.IsAny<AddPropertyImageCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PropertyImageDto)null!);

            var handler = new UploadPropertyImagesHandler(storage.Object, mediator.Object);

            var cmd = new UploadPropertyImagesCommand(propertyId, files, CoverIndex: 5);

            // Act
            var processed = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            processed.Should().Be(2);

            mediator.Verify(m => m.Send(It.Is<AddPropertyImageCommand>(c => c.Url == "u0" && c.IsCover == false), It.IsAny<CancellationToken>()), Times.Once);
            mediator.Verify(m => m.Send(It.Is<AddPropertyImageCommand>(c => c.Url == "u1" && c.IsCover == false), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
