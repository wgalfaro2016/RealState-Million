using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using RealEstate.Application.Commands;
using RealEstate.Application.Dtos;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Tests
{
    [TestFixture]
    public class CreateOwnerTests
    {
        private IMapper _mapper = null!;

        [OneTimeSetUp]
        public void OneTimeSetup() {
            var cfg = new MapperConfiguration(c => {
                c.CreateMap<Owner, OwnerDto>();
            });
            cfg.AssertConfigurationIsValid();
            _mapper = cfg.CreateMapper();
        }


        [Test]
        public void Validator_should_fail_on_invalid_fields() {
            var v = new CreateOwnerValidator();

            v.TestValidate(new CreateOwnerCommand("", null, null, null))
             .ShouldHaveValidationErrorFor(x => x.Name);

            v.TestValidate(new CreateOwnerCommand("Wilmar", new string('c', 251), null, null))
             .ShouldHaveValidationErrorFor(x => x.Address);

            v.TestValidate(new CreateOwnerCommand("Wilmar", null, new string('b', 301), null))
             .ShouldHaveValidationErrorFor(x => x.Photo);

            v.TestValidate(new CreateOwnerCommand("Wilmar", null, null, DateTime.Today.AddDays(1)))
             .ShouldHaveValidationErrorFor(x => x.Birthday);
        }


        [Test]
        public void Validator_should_pass_on_valid_input() {
            var v = new CreateOwnerValidator();
            var ok = new CreateOwnerCommand("Wilmar Alfaro", "Cra 5 # 300", "https://images/property.jpg", DateTime.Today.AddYears(-20));
            v.TestValidate(ok).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public async Task Handler_should_create_owner_with_defaults_when_nulls() {
            var repo = new Mock<IGenericRepository<Owner>>();
            Owner? captured = null;

            repo.Setup(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()))
                .Callback<Owner, CancellationToken>((e, _) => captured = e)
                .Returns(Task.CompletedTask);

            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.Repository<Owner>()).Returns(repo.Object);
            uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

            var handler = new CreateOwnerHandler(uow.Object, _mapper);

            var cmd = new CreateOwnerCommand("Wilmar", null, null, null);

            var dto = await handler.Handle(cmd, CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.Name.Should().Be("Wilmar");
            captured.Address.Should().Be(string.Empty);
            captured.Photo.Should().Be(string.Empty);
            captured.Birthday.Should().BeNull();

            dto.Name.Should().Be("Wilmar");
            dto.Address.Should().Be(string.Empty);
            dto.Photo.Should().Be(string.Empty);
            dto.Birthday.Should().BeNull();

            repo.Verify(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Once);
            uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Test]
        public async Task Handler_should_create_owner_with_provided_values() {
            var repo = new Mock<IGenericRepository<Owner>>();
            Owner? captured = null;

            repo.Setup(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()))
                .Callback<Owner, CancellationToken>((e, _) => captured = e)
                .Returns(Task.CompletedTask);

            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.Repository<Owner>()).Returns(repo.Object);

       
            uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

            var handler = new CreateOwnerHandler(uow.Object, _mapper);

            var bday = new DateTime(1990, 1, 1);
            var cmd = new CreateOwnerCommand("Lorena", "Calle 9", "https://images/property.jpg", bday);

            var dto = await handler.Handle(cmd, CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.Name.Should().Be("Lorena");
            captured.Address.Should().Be("Calle 9");
            captured.Photo.Should().Be("https://images/property.jpg");
            captured.Birthday.Should().Be(bday);

            dto.Name.Should().Be("Lorena");
            dto.Address.Should().Be("Calle 9");
            dto.Photo.Should().Be("https://images/property.jpg");
            dto.Birthday.Should().Be(bday);

            repo.Verify(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Once);
            uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


    }
}
