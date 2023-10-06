using System.Diagnostics;
using NUnit.Framework.Internal;
using Utils;

namespace UnitTest;

[TestFixture]
public class KDIndexTest
{
    int[][] _points = new int[][] {
        new int[] {54, 1}, new int[] {97, 21}, new int[] {65, 35}, new int[] {33, 54},
        new int[] {95, 39}, new int[] {54, 3}, new int[] {53, 54}, new int[] {84, 72},
        new int[] {33, 34}, new int[] {43, 15}, new int[] {52, 83}, new int[] {81, 23},
        new int[] {1, 61}, new int[] {38, 74}, new int[] {11, 91}, new int[] {24, 56},
        new int[] {90, 31}, new int[] {25, 57}, new int[] {46, 61}, new int[] {29, 69},
        new int[] {49, 60}, new int[] {4, 98}, new int[] {71, 15}, new int[] {60, 25},
        new int[] {38, 84}, new int[] {52, 38}, new int[] {94, 51}, new int[] {13, 25},
        new int[] {77, 73}, new int[] {88, 87}, new int[] {6, 27}, new int[] {58, 22},
        new int[] {53, 28}, new int[] {27, 91}, new int[] {96, 98}, new int[] {93, 14},
        new int[] {22, 93}, new int[] {45, 94}, new int[] {18, 28}, new int[] {35, 15},
        new int[] {19, 81}, new int[] {20, 81}, new int[] {67, 53}, new int[] {43, 3},
        new int[] {47, 66}, new int[] {48, 34}, new int[] {46, 12}, new int[] {32, 38},
        new int[] {43, 12}, new int[] {39, 94}, new int[] {88, 62}, new int[] {66, 14},
        new int[] {84, 30}, new int[] {72, 81}, new int[] {41, 92}, new int[] {26, 4},
        new int[] {6, 76}, new int[] {47, 21}, new int[] {57, 70}, new int[] {71, 82},
        new int[] {50, 68}, new int[] {96, 18}, new int[] {40, 31}, new int[] {78, 53},
        new int[] {71, 90}, new int[] {32, 14}, new int[] {55, 6}, new int[] {32, 88},
        new int[] {62, 32}, new int[] {21, 67}, new int[] {73, 81}, new int[] {44, 64},
        new int[] {29, 50}, new int[] {70, 5}, new int[] {6, 22}, new int[] {68, 3},
        new int[] {11, 23}, new int[] {20, 42}, new int[] {21, 73}, new int[] {63, 86},
        new int[] {9, 40}, new int[] {99, 2}, new int[] {99, 76}, new int[] {56, 77},
        new int[] {83, 6}, new int[] {21, 72}, new int[] {78, 30}, new int[] {75, 53},
        new int[] {41, 11}, new int[] {95, 20}, new int[] {30, 38}, new int[] {96, 82},
        new int[] {65, 48}, new int[] {33, 18}, new int[] {87, 28}, new int[] {10, 10},
        new int[] {40, 34}, new int[] {10, 20}, new int[] {47, 29}, new int[] {46, 78}
    };


    int[] _ids = new int[] {
        97,74,95,30,77,38,76,27,80,55,72,90,88,48,43,46,65,39,62,93,9,96,47,8,3,12,15,14,21,41,36,40,69,56,85,78,17,71,44,
        19,18,13,99,24,67,33,37,49,54,57,98,45,23,31,66,68,0,32,5,51,75,73,84,35,81,22,61,89,1,11,86,52,94,16,2,6,25,92,
        42,20,60,58,83,79,64,10,59,53,26,87,4,63,50,7,28,82,70,29,34,91
    };

    int[] _coords = new int[] {
        10,20,6,22,10,10,6,27,20,42,18,28,11,23,13,25,9,40,26,4,29,50,30,38,41,11,43,12,43,3,46,12,32,14,35,15,40,31,33,18,
        43,15,40,34,32,38,33,34,33,54,1,61,24,56,11,91,4,98,20,81,22,93,19,81,21,67,6,76,21,72,21,73,25,57,44,64,47,66,29,
        69,46,61,38,74,46,78,38,84,32,88,27,91,45,94,39,94,41,92,47,21,47,29,48,34,60,25,58,22,55,6,62,32,54,1,53,28,54,3,
        66,14,68,3,70,5,83,6,93,14,99,2,71,15,96,18,95,20,97,21,81,23,78,30,84,30,87,28,90,31,65,35,53,54,52,38,65,48,67,
        53,49,60,50,68,57,70,56,77,63,86,71,90,52,83,71,82,72,81,94,51,75,53,95,39,78,53,88,62,84,72,77,73,99,76,73,81,88,
        87,96,98,96,82
    };

    [SetUp]
    public void Setup()
    {

    }

    [OneTimeSetUp]
    public void StartTest()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    [Test]
    public void CreateIndexTest()
    {
        var index = MakeIndex();
        CollectionAssert.AreEqual(index.Ids, _ids);
        CollectionAssert.AreEqual(index.Coords, _coords);
    }

    [Test]
    public void RangeSearchTest()
    {
        var index = MakeIndex();
        var result = index.Range(20, 30, 50, 70);
        
        CollectionAssert.AreEqual(result, new int[] {60,20,45,3,17,71,44,19,18,15,69,90,62,96,47,8,77,72}, "returns ids");

        foreach (var id in result) 
        {
            var p = _points[id];
            if (p[0] < 20 || p[0] > 50 || p[1] < 30 || p[1] > 70)
            {
                Assert.Fail("result point in range");
            }
        }
        // result points in range

        foreach (var id in _ids) 
        {
            var p = _points[id];
            if (result.IndexOf(id) < 0 && p[0] >= 20 && p[0] <= 50 && p[1] >= 30 && p[1] <= 70)
            {
                Assert.Fail("outside point not in range");
            }
        }
    }

    [Test]
    public void RadiusSearchTest()
    {
        var index = MakeIndex();
        var qp = new int[] {50, 50};
        var r = 20;
        var r2 = 20 * 20;

        var result = index.Within(qp[0], qp[1], r);

        CollectionAssert.AreEqual(result, new int[] {60,6,25,92,42,20,45,3,71,44,18,96}, "returns ids");

        foreach (var id in result) 
        {
            var p = _points[id];
            if (SqDist(p, qp) > r2) 
            {
                Assert.Fail("result point in range");
            }
        }
        // result points in range

        foreach (var id in _ids) 
        {
            var p = _points[id];
            if (result.IndexOf(id) < 0 && SqDist(p, qp) <= r2)
            {
                Assert.Fail("outside point not in range");
            }
        }
        // outside points not in range
    }

    [Test]
    public void LessItemsTest()
    {
        var index = new KDIndex(_points.Length);

        Assert.Throws<Exception>(() => {
            index.Range(0, 0, 20, 20);
        });

        Assert.Throws<Exception>(() => {
            index.Within(10, 10, 20);
        });
    }

    [Test]
    public void NoComplainTest()
    {
        Assert.DoesNotThrow(() => {
            var index = new KDIndex(0);
            index.Finish();
            CollectionAssert.AreEqual(index.Range(0, 0, 10, 10), new List<int>());
            CollectionAssert.AreEqual(index.Within(0, 0, 10), new List<int>());
        });
    }

    KDIndex MakeIndex()
    {
        var index = new KDIndex(_points.Length, 10);
        foreach (var point in _points) index.Add(point[0], point[1]);
        return index.Finish();
    }

    int SqDist(int[] a, int[] b) 
    {
        var dx = a[0] - b[0];
        var dy = a[1] - b[1];
        return dx * dx + dy * dy;
    }
}