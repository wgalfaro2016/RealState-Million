using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using MockQueryable.Moq;
using Moq;
using RealEstate.Application.Commands;
using RealEstate.Application.Common.Exceptions;
using RealEstate.Application.Dtos;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Tests
{
    [TestFixture]
    public class ChangePropertyPriceTests
    {
        private IMapper _mapper = null!;

        [OneTimeSetUp]
        public void OneTimeSetup() {
            var cfg = new MapperConfiguration(c => {
                c.CreateMap<Property, PropertyDto>();
            });
            cfg.AssertConfigurationIsValid();
            _mapper = cfg.CreateMapper();
        }


        [Test]
        public void Validator_should_fail_on_invalid_fields() {
            var v = new ChangePropertyPriceValidator();

            var cmd1 = new ChangePropertyPriceCommand(Guid.Empty, 10m);
            v.TestValidate(cmd1).ShouldHaveValidationErrorFor(x => x.PropertyId);

            var cmd2 = new ChangePropertyPriceCommand(Guid.NewGuid(), 0m);
            v.TestValidate(cmd2).ShouldHaveValidationErrorFor(x => x.NewPrice);

            var cmd3 = new ChangePropertyPriceCommand(Guid.NewGuid(), -1m);
            v.TestValidate(cmd3).ShouldHaveValidationErrorFor(x => x.NewPrice);

            var cmd4 = new ChangePropertyPriceCommand(Guid.NewGuid(), 12.345m);
            v.TestValidate(cmd4).ShouldHaveValidationErrorFor(x => x.NewPrice)
             .WithErrorMessage("NewPrice must have up to 2 decimals.");
        }

        [Test]
        public void Validator_should_pass_on_valid_input() {
            var v = new ChangePropertyPriceValidator();
            var ok = new ChangePropertyPriceCommand(Guid.NewGuid(), 199.99m);
            v.TestValidate(ok).ShouldNotHaveAnyValidationErrors();
        }


        [Test]
        public async Task Handler_should_throw_NotFound_when_property_does_not_exist() {
            var propRepo = new Mock<IGenericRepository<Property>>();
            propRepo.Setup(r => r.Query())
                    .Returns(new List<Property>().AsQueryable().BuildMock());

            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.Repository<Property>()).Returns(propRepo.Object);

            var handler = new ChangePropertyPriceHandler(uow.Object, _mapper);

            var cmd = new ChangePropertyPriceCommand(Guid.NewGuid(), 123.45m);

            // Act
            Func<Task> act = async () => await handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
