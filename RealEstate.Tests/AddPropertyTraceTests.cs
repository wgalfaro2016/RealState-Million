using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using MockQueryable.Moq;
using Moq;
using RealEstate.Application.Commands;
using RealEstate.Application.Common.Exceptions;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Tests
{
    [TestFixture]
    public class AddPropertyTraceTests
    {
        private IMapper _mapper = null!;

        [OneTimeSetUp]
        public void OneTimeSetup() {
            var cfg = new MapperConfiguration(c =>
                c.CreateMap<PropertyTrace, PropertyTraceDto>());
            cfg.AssertConfigurationIsValid();
            _mapper = cfg.CreateMapper();
        }


        [Test]
        public void Validator_should_fail_on_empty_or_invalid_fields() {
            var v = new AddPropertyTraceValidator();

            var cmd1 = new AddPropertyTraceCommand(Guid.Empty, "trade", 10m, 1m);
            v.TestValidate(cmd1).ShouldHaveValidationErrorFor(x => x.PropertyId);

            var cmd2 = new AddPropertyTraceCommand(Guid.NewGuid(), "", 10m, 1m);
            v.TestValidate(cmd2).ShouldHaveValidationErrorFor(x => x.Name);

            var cmd3 = new AddPropertyTraceCommand(Guid.NewGuid(), "trade", -1m, 1m);
            v.TestValidate(cmd3).ShouldHaveValidationErrorFor(x => x.Value);

            var cmd4 = new AddPropertyTraceCommand(Guid.NewGuid(), "trade", 10m, -0.01m);
            v.TestValidate(cmd4).ShouldHaveValidationErrorFor(x => x.Tax);

            var cmd5 = new AddPropertyTraceCommand(Guid.NewGuid(), "trade", 12.345m, 0.5m);
            v.TestValidate(cmd5).ShouldHaveValidationErrorFor(x => x.Value)
             .WithErrorMessage("Value must have up to 2 decimals.");

            var cmd6 = new AddPropertyTraceCommand(Guid.NewGuid(), "trade", 10.00m, 0.123m);
            v.TestValidate(cmd6).ShouldHaveValidationErrorFor(x => x.Tax)
             .WithErrorMessage("Tax must have up to 2 decimals.");
        }

        [Test]
        public void Validator_should_pass_on_valid_input() {
            var v = new AddPropertyTraceValidator();
            var ok = new AddPropertyTraceCommand(Guid.NewGuid(), "trade", 100.25m, 7.50m, DateTime.UtcNow);
            v.TestValidate(ok).ShouldNotHaveAnyValidationErrors();
        }


        [Test]
        public async Task Handler_should_throw_NotFound_when_property_does_not_exist() {
            var propRepo = new Mock<IGenericRepository<Property>>();
            propRepo.Setup(r => r.Query())
                    .Returns(new List<Property>().AsQueryable().BuildMock());

            var traceRepo = new Mock<IGenericRepository<PropertyTrace>>();

            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.Repository<Property>()).Returns(propRepo.Object);
            uow.Setup(u => u.Repository<PropertyTrace>()).Returns(traceRepo.Object);

            var handler = new AddPropertyTraceHandler(uow.Object, _mapper);

            var cmd = new AddPropertyTraceCommand(Guid.NewGuid(), "trade", 10m, 1m);

            // Act
            Func<Task> act = async () => await handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
