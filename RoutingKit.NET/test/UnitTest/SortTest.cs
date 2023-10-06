using RoutingKit;

namespace UnitTest;

public class SortTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        int[] rangeList = { 100, 1000000000 };

        Random rng = new Random(42);

        foreach (int range in rangeList)
        {
            List<int> v = new List<int>(10000);
            for (int i = 0; i < 10000; i++)
            {
                int x = rng.Next(0, range);
                v.Add(x);
            }

            var q = Sort.ComputeStableSortPermutationUsingKey(v, range, x => x);
            Assert.IsTrue(Sort.IsSortedUsingLess(Permutation.ApplyPermutation(q, v)));
            CollectionAssert.AreEqual(q, Sort.ComputeStableSortPermutationUsingLess(v));
            CollectionAssert.AreEqual(q, Sort.ComputeStableSortPermutationUsingComparator(v, (l, r) => l.CompareTo(r)));
        }
    }

    [Test]
    public void Test2()
    {
        int[] rangeList = { 100, 1000000000 };

        Random rng = new Random(42);

        foreach (int range in rangeList)
        {
            List<int> v = new List<int>(10000);
            for (int i = 0; i < 10000; i++)
            {
                int x = rng.Next(0, range);
                v.Add(x);
            }

            var q = Sort.ComputeInverseStableSortPermutationUsingKey(v, range, x => x);
            Assert.IsTrue(Sort.IsSortedUsingLess(Permutation.ApplyInversePermutation(q, v)));
            CollectionAssert.AreEqual(q, Sort.ComputeInverseStableSortPermutationUsingLess(v));
            CollectionAssert.AreEqual(q, Sort.ComputeInverseStableSortPermutationUsingComparator(v, (l, r) => l.CompareTo(r)));
        }
    }

    [Test]
    public void Test3()
    {
        int[] rangeList = { 100, 1000000000 };

        Random rng = new Random(42);

        foreach (int range in rangeList)
        {
            List<int> v = new List<int>(10000);
            for (int i = 0; i < 10000; i++)
            {
                int x = rng.Next(0, range);
                v.Add(x);
            }

            Assert.IsTrue(Sort.IsSortedUsingLess(Permutation.ApplyPermutation(
                Sort.ComputeSortPermutationUsingKey(v, range, x => x), v
            )));
            Assert.IsTrue(Sort.IsSortedUsingLess(Permutation.ApplyPermutation(
                Sort.ComputeSortPermutationUsingLess(v), v
            )));
            Assert.IsTrue(Sort.IsSortedUsingLess(Permutation.ApplyPermutation(
                Sort.ComputeSortPermutationUsingComparator(v, (l, r) => l.CompareTo(r)), v
            )));
        }
    }

    [Test]
    public void Test4()
    {
        int[] rangeList = { 100, 1000000000 };

        Random rng = new Random(42);

        foreach (int range in rangeList)
        {
            List<int> v = new List<int>(10000);
            for (int i = 0; i < 10000; i++)
            {
                int x = rng.Next(0, range);
                v.Add(x);
            }

            Assert.IsTrue(Sort.IsSortedUsingLess(Permutation.ApplyInversePermutation(
                Sort.ComputeInverseSortPermutationUsingKey(v, range, x => x), v
            )));
            Assert.IsTrue(Sort.IsSortedUsingLess(Permutation.ApplyInversePermutation(
                Sort.ComputeInverseSortPermutationUsingLess(v), v
            )));
            Assert.IsTrue(Sort.IsSortedUsingLess(Permutation.ApplyInversePermutation(
                Sort.ComputeInverseSortPermutationUsingComparator(v, (l, r) => l.CompareTo(r)), v
            )));
        }
    }
}