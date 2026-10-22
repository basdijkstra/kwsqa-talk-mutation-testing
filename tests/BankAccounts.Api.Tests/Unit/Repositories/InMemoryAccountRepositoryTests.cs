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
    public void Add_AssignsSequentialIds()
    {
        var ids = Enumerable.Range(0, 3).Select(_ => _repository.Add(NewAccount()).Id);

        Assert.That(ids, Is.EqualTo(new[] { 1, 2, 3 }));
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
