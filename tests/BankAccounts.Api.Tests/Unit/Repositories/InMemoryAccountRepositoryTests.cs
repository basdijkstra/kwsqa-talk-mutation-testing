using BankAccounts.Api.Models;
using BankAccounts.Api.Repositories;

namespace BankAccounts.Api.Tests.Unit.Repositories;

[TestFixture]
public class InMemoryAccountRepositoryTests
{
    private InMemoryAccountRepository _repository = null!;

    [SetUp]
    public void CreateRepository() => _repository = new InMemoryAccountRepository();

    [Test]
    public void Add_FirstAccount_GetsIdOne()
    {
        var account = _repository.Add(NewAccount());

        Assert.That(account.Id, Is.EqualTo(1));
    }

    [Test]
    public void Add_AssignsSequentialIds()
    {
        var ids = Enumerable.Range(0, 3).Select(_ => _repository.Add(NewAccount()).Id);

        Assert.That(ids, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Add_OverwritesIdSetByCaller()
    {
        var account = _repository.Add(new Account { Id = 42, Type = AccountType.Savings });

        Assert.Multiple(() =>
        {
            Assert.That(account.Id, Is.EqualTo(1));
            Assert.That(_repository.GetById(42), Is.Null);
        });
    }

    [Test]
    public void Add_IdsAreNotReusedAfterDelete()
    {
        var first = _repository.Add(NewAccount());
        _repository.Delete(first.Id);

        var second = _repository.Add(NewAccount());

        Assert.That(second.Id, Is.EqualTo(2));
    }

    [Test]
    public void GetById_AddedAccount_ReturnsItWithAllFields()
    {
        var added = _repository.Add(NewAccount(AccountType.Savings, 12.34m));

        var found = _repository.GetById(added.Id);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Id, Is.EqualTo(added.Id));
            Assert.That(found.Type, Is.EqualTo(AccountType.Savings));
            Assert.That(found.Balance, Is.EqualTo(12.34m));
        });
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(-1)]
    public void GetById_EmptyRepository_ReturnsNull(int id)
    {
        Assert.That(_repository.GetById(id), Is.Null);
    }

    [Test]
    public void Update_ReplacesStoredAccount()
    {
        var added = _repository.Add(NewAccount(AccountType.Checking, 0m));

        _repository.Update(new Account { Id = added.Id, Type = AccountType.Checking, Balance = 99m });

        Assert.That(_repository.GetById(added.Id)!.Balance, Is.EqualTo(99m));
    }

    [Test]
    public void Delete_ExistingAccount_ReturnsTrueAndRemovesIt()
    {
        var added = _repository.Add(NewAccount());

        var deleted = _repository.Delete(added.Id);

        Assert.Multiple(() =>
        {
            Assert.That(deleted, Is.True);
            Assert.That(_repository.GetById(added.Id), Is.Null);
        });
    }

    [Test]
    public void Delete_UnknownAccount_ReturnsFalse()
    {
        Assert.That(_repository.Delete(1), Is.False);
    }

    [Test]
    public void Delete_SameAccountTwice_SecondCallReturnsFalse()
    {
        var added = _repository.Add(NewAccount());
        _repository.Delete(added.Id);

        Assert.That(_repository.Delete(added.Id), Is.False);
    }

    [Test]
    public void Add_FromManyThreads_GivesUniqueIds()
    {
        const int count = 1000;

        var ids = Enumerable.Range(0, count)
            .AsParallel()
            .Select(_ => _repository.Add(NewAccount()).Id)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(ids, Is.Unique);
            Assert.That(ids, Is.EquivalentTo(Enumerable.Range(1, count)));
        });
    }

    private static Account NewAccount(AccountType type = AccountType.Checking, decimal balance = 0m) =>
        new() { Type = type, Balance = balance };
}
