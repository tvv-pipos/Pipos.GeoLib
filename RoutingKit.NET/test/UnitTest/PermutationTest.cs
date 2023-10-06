using RoutingKit;

namespace UnitTest;

public class PermutationTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.IsTrue(Permutation.IsPermutation(new List<int> { 1, 5, 2, 0, 3, 6, 4 }));
        Assert.IsFalse(Permutation.IsPermutation(new List<int> { 1, 5, 2, 3, 6, 4 }));
        Assert.IsFalse(Permutation.IsPermutation(new List<int> { 1, 5, 2, 3, 6, 2, 4 }));
        Assert.IsFalse(Permutation.IsPermutation(new List<int> { 1, 5, 0, 2, 3, 6, 2, 4 }));

        Assert.IsTrue(Permutation.IsPermutation(new List<int> { }));
        Assert.IsTrue(Permutation.IsPermutation(new List<int> { 0 }));

        CollectionAssert.AreEqual(Permutation.IdentityPermutation(0), new List<int>());
        CollectionAssert.AreEqual(Permutation.IdentityPermutation(1), new List<int> { 0 });
        CollectionAssert.AreEqual(Permutation.IdentityPermutation(2), new List<int> { 0, 1 });
        CollectionAssert.AreEqual(Permutation.IdentityPermutation(10), new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    }

    [Test]
    public void Test2()
    {
        var p = new List<int> {0, 3, 1, 2};
		var inv_p = new List<int>{0, 2, 3, 1};

        CollectionAssert.AreEqual(Permutation.InvertPermutation(p), inv_p);
        CollectionAssert.AreEqual(Permutation.InvertPermutation(inv_p), p);

        CollectionAssert.AreEqual(Permutation.ApplyInversePermutation(new int[0], new List<int>()), new List<int>());
        CollectionAssert.AreEqual(Permutation.ApplyPermutation(new int[0], new List<int>()), new List<int>());
    }

    [Test]
    public void Test3()
    {
        var q = Permutation.RandomPermutation(10, new Random(42));

        Assert.IsTrue(Permutation.IsPermutation(q));

        CollectionAssert.AreEqual(
            Permutation.ApplyPermutation(q, Permutation.InvertPermutation(q).ToList()),
            Permutation.IdentityPermutation(10));
    }

    [Test]
    public void Test4()
    {
        var p = Permutation.RandomPermutation(100, new Random(1));
        var q = Permutation.RandomPermutation(100, new Random(2));
        var elements = Permutation.RandomPermutation(100, new Random(3));

        CollectionAssert.AreEqual(
            Permutation.ApplyPermutation<int>(q, Permutation.ApplyPermutation<int>(p, elements)),
            Permutation.ApplyPermutation<int>(Permutation.ChainPermutationFirstLeftThenRight(p, q), elements)
        );
    }
}