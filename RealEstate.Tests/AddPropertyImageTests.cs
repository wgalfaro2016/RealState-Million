using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using MockQueryable.Moq;
using Moq;
using RealEstate.Application.Commands;
using RealEstate.Application.Interfaces;
using RealEstate.Application.Mapping;
using RealEstate.Domain.Entities;

namespace RealEstate.Tests
{
    [TestFixture]
    public class AddPropertyImageTests
    {

        private IMapper _mapper = null!;

        [OneTimeSetUp]
        public void OneTimeSetup() {
            var cfg = new MapperConfiguration(c => c.AddProfile(new PropertyImageProfile()));
            cfg.AssertConfigurationIsValid();
            _mapper = cfg.CreateMapper();
        }

        [Test]
        public void Validator_should_fail_on_empty_fields() {
            var v = new AddPropertyImageValidator();

            var cmd1 = new AddPropertyImageCommand(Guid.Empty, "http://x/MyImagen.jpg");
            v.TestValidate(cmd1).ShouldHaveValidationErrorFor(x => x.PropertyId);

            var cmd2 = new AddPropertyImageCommand(Guid.NewGuid(), "");
            v.TestValidate(cmd2).ShouldHaveValidationErrorFor(x => x.Url);
        }

        [Test]
        public void Validator_should_pass_on_valid_input() {
            var v = new AddPropertyImageValidator();
            var ok = new AddPropertyImageCommand(Guid.NewGuid(), "https://upload/images/MyImagen.png", true);
            v.TestValidate(ok).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Handler_should_throw_when_property_not_found() {
            // Arrange
            var propRepo = new Mock<IGenericRepository<Property>>();
            propRepo
                .Setup(r => r.Query())
                .Returns(new List<Property>().AsQueryable().BuildMock()); 

            var imgRepo = new Mock<IGenericRepository<PropertyImage>>();

            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.Repository<Property>()).Returns(propRepo.Object);
            uow.Setup(u => u.Repository<PropertyImage>()).Returns(imgRepo.Object);

            var handler = new AddPropertyImageHandler(uow.Object, _mapper);

            // Act
            Func<Task> act = async () => await handler.Handle(
                new AddPropertyImageCommand(Guid.NewGuid(), "https://upload/images/MyImagen.png", false),
                CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
               .WithMessage("Property*does not exist*");
        }


    }
}
