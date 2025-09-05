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
    public class CreatePropertyTests
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
            var v = new CreatePropertyValidator();
            var invalidId = Guid.Empty;

            v.TestValidate(new CreatePropertyCommand("", "Dir", 10m, "C1", 2024, Guid.NewGuid()))
             .ShouldHaveValidationErrorFor(x => x.Name);

            v.TestValidate(new CreatePropertyCommand("House", "", 10m, "C1", 2024, Guid.NewGuid()))
             .ShouldHaveValidationErrorFor(x => x.Address);

            v.TestValidate(new CreatePropertyCommand("House", "Dir", 0m, "C1", 2024, Guid.NewGuid()))
             .ShouldHaveValidationErrorFor(x => x.Price);

            v.TestValidate(new CreatePropertyCommand("House", "Dir", 10m, "", 2024, Guid.NewGuid()))
             .ShouldHaveValidationErrorFor(x => x.CodeInternal);

            v.TestValidate(new CreatePropertyCommand("House", "Dir", 10m, "C1", 1800, Guid.NewGuid()))
             .ShouldHaveValidationErrorFor(x => x.Year);

            v.TestValidate(new CreatePropertyCommand("House", "Dir", 10m, "C1", DateTime.UtcNow.Year + 2, Guid.NewGuid()))
             .ShouldHaveValidationErrorFor(x => x.Year);

            v.TestValidate(new CreatePropertyCommand("House", "Dir", 10m, "C1", 2024, invalidId))
             .ShouldHaveValidationErrorFor(x => x.IdOwner);
        }

        [Test]
        public void Validator_should_pass_on_valid_input() {
            var v = new CreatePropertyValidator();
            var ok = new CreatePropertyCommand(
                "House", "Calle 100", 123.45m, "C-001",
                DateTime.UtcNow.Year, Guid.NewGuid());

            v.TestValidate(ok).ShouldNotHaveAnyValidationErrors();
        }


        [Test]
        public async Task Handler_should_throw_NotFound_when_CodeInternal_already_exists() {
            var existing = new List<Property>
            {
                new Property { IdProperty = Guid.NewGuid(), CodeInternal = "C-001" }
            };

            var propRepo = new Mock<IGenericRepository<Property>>();
            propRepo.Setup(r => r.Query())
                    .Returns(existing.AsQueryable().BuildMock());

            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.Repository<Property>()).Returns(propRepo.Object);

            uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

            var handler = new CreatePropertyHandler(uow.Object, _mapper);

            var cmd = new CreatePropertyCommand(
                "New", "Addr", 200m, "C-001", 2024, Guid.NewGuid());

            // Act
            Func<Task> act = async () => await handler.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task Handler_should_create_property_and_return_dto_when_CodeInternal_is_unique() {
            // Arrange
            var existing = new List<Property>
            {
                new Property { IdProperty = Guid.NewGuid(), CodeInternal = "C-002" }
            };

            var propRepo = new Mock<IGenericRepository<Property>>();
            propRepo.Setup(r => r.Query())
                    .Returns(existing.AsQueryable().BuildMock());

            Property? captured = null;
            propRepo.Setup(r => r.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()))
                    .Callback<Property, CancellationToken>((e, _) => captured = e)
                    .Returns(Task.CompletedTask);

            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.Repository<Property>()).Returns(propRepo.Object);
            uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

            var handler = new CreatePropertyHandler(uow.Object, _mapper);

            var ownerId = Guid.NewGuid();
            var cmd = new CreatePropertyCommand(
                "Casa Bonita", "Calle 9", 999.99m, "C-003",
                DateTime.UtcNow.Year, ownerId);

            // Act
            var dto = await handler.Handle(cmd, CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.Name.Should().Be("Casa Bonita");
            captured.Address.Should().Be("Calle 9");
            captured.Price.Should().Be(999.99m);
            captured.CodeInternal.Should().Be("C-003");
            captured.Year.Should().Be(DateTime.UtcNow.Year);
            captured.IdOwner.Should().Be(ownerId);

            dto.Should().NotBeNull();
            dto.Name.Should().Be("Casa Bonita");
            dto.Address.Should().Be("Calle 9");
            dto.Price.Should().Be(999.99m);
            dto.CodeInternal.Should().Be("C-003");
            dto.Year.Should().Be(DateTime.UtcNow.Year);
            dto.IdOwner.Should().Be(ownerId);

            propRepo.Verify(r => r.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()), Times.Once);
            uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
