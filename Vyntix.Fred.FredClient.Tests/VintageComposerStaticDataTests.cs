namespace LeaderAnalytics.Vyntix.Fred.FredClient.Tests;

[TestFixture()]
public class VintageComposerStaticDataTests
{

    [Test]
    public void HappyPathDenseTest()
    {
        IVintageComposer composer = new VintageComposer();
        List<FredVintage> testData = CreateHappyPathDenseData();
        Assert.AreEqual(3, testData.Count);
        Assert.AreEqual(3, testData[0].Observations.Count);
        Assert.AreEqual(4, testData[1].Observations.Count);
        Assert.AreEqual(5, testData[2].Observations.Count);
        List<IFredObservation> dense = testData.SelectMany(x => x.Observations).ToList();
        List<IFredObservation> sparse = composer.MakeSparse(dense);
        Assert.AreEqual(7, sparse.Count);
        // 
        sparse = composer.MakeSparse(sparse);
        Assert.AreEqual(7, sparse.Count);
    }


    [Test]
    public void VerifyStaticDataTest()
    {
        List<FredVintage> testData = CreateTestData();
        Assert.AreEqual(3, testData.Count);
        Assert.AreEqual(3, testData[0].Observations.Count);
        Assert.AreEqual(5, testData[1].Observations.Count);
        Assert.AreEqual(6, testData[2].Observations.Count);
    }
    

    [Test]
    public void MakeSparseTest()
    {
        IVintageComposer composer = new VintageComposer();
        List<FredVintage> testData = CreateTestData();
        List<IFredObservation> dense = testData.SelectMany(x => x.Observations).ToList();
        List<IFredObservation> sparse = composer.MakeSparse(dense);
        Assert.AreEqual(8, sparse.Count);
    }

    [Test]
    public void MakeDenseTest()
    {
        IVintageComposer composer = new VintageComposer();
        List<FredVintage> testData = CreateTestData();
        List<IFredObservation> dense = testData.SelectMany(x => x.Observations).ToList();
        List<IFredObservation> sparse = composer.MakeSparse(dense);
        dense = composer.MakeDense(sparse);
        Assert.AreEqual(14, dense.Count);
    }




    private List<FredVintage> CreateTestData()
    {
        List<FredVintage> vintages = new List<FredVintage>();
        vintages.Add(new FredVintage { Symbol = "1", VintageDate = new DateTime(2000, 1, 1), Observations = new List<IFredObservation>() });
        vintages.Add(new FredVintage { Symbol = "1", VintageDate = new DateTime(2000, 2, 1), Observations = new List<IFredObservation>() });
        vintages.Add(new FredVintage { Symbol = "1", VintageDate = new DateTime(2000, 3, 1), Observations = new List<IFredObservation>() });

        FredVintage v1 = vintages[0];
        v1.Observations.Add(new FredObservation { VintageDate = v1.VintageDate, Vintage = v1, ObsDate = new DateTime(1999, 1, 1), Value = 1 });
        v1.Observations.Add(new FredObservation { VintageDate = v1.VintageDate, Vintage = v1, ObsDate = new DateTime(1999, 2, 1), Value = 2 });
        v1.Observations.Add(new FredObservation { VintageDate = v1.VintageDate, Vintage = v1, ObsDate = new DateTime(1999, 3, 1), Value = 3 });


        FredVintage v2 = vintages[1];
        v2.Observations.Add(new FredObservation { VintageDate = v2.VintageDate, Vintage = v2, ObsDate = new DateTime(1999, 1, 1), Value = 1 });
        v2.Observations.Add(new FredObservation { VintageDate = v2.VintageDate, Vintage = v2, ObsDate = new DateTime(1999, 2, 1), Value = 2 });
        v2.Observations.Add(new FredObservation { VintageDate = v2.VintageDate, Vintage = v2, ObsDate = new DateTime(1999, 2, 2), Value = 2.2m }); //Missing in Vintage 1
        v2.Observations.Add(new FredObservation { VintageDate = v2.VintageDate, Vintage = v2, ObsDate = new DateTime(1999, 3, 1), Value = 3.5m });
        v2.Observations.Add(new FredObservation { VintageDate = v2.VintageDate, Vintage = v2, ObsDate = new DateTime(1999, 4, 1), Value = 4 });


        FredVintage v3 = vintages[2];
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 1, 1), Value = 1 });
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 2, 1), Value = 2 });
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 2, 2), Value = 2.2m }); // must exist here as it cannot be removed
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 3, 1), Value = 3 });
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 4, 1), Value = 4 });
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 5, 1), Value = 5 });
        return vintages;
    }

    private List<FredVintage> CreateHappyPathDenseData()
    {
        List<FredVintage> vintages = new List<FredVintage>();
        vintages.Add(new FredVintage { Symbol = "1", VintageDate = new DateTime(2000, 1, 1), Observations = new List<IFredObservation>() });
        vintages.Add(new FredVintage { Symbol = "1", VintageDate = new DateTime(2000, 2, 1), Observations = new List<IFredObservation>() });
        vintages.Add(new FredVintage { Symbol = "1", VintageDate = new DateTime(2000, 3, 1), Observations = new List<IFredObservation>() });

        FredVintage v1 = vintages[0];
        v1.Observations.Add(new FredObservation { VintageDate = v1.VintageDate, Vintage = v1, ObsDate = new DateTime(1999, 1, 1), Value = 1 });
        v1.Observations.Add(new FredObservation { VintageDate = v1.VintageDate, Vintage = v1, ObsDate = new DateTime(1999, 2, 1), Value = 2 });
        v1.Observations.Add(new FredObservation { VintageDate = v1.VintageDate, Vintage = v1, ObsDate = new DateTime(1999, 3, 1), Value = 3 });


        FredVintage v2 = vintages[1];
        v2.Observations.Add(new FredObservation { VintageDate = v2.VintageDate, Vintage = v2, ObsDate = new DateTime(1999, 1, 1), Value = 1 });
        v2.Observations.Add(new FredObservation { VintageDate = v2.VintageDate, Vintage = v2, ObsDate = new DateTime(1999, 2, 1), Value = 2 });
        v2.Observations.Add(new FredObservation { VintageDate = v2.VintageDate, Vintage = v2, ObsDate = new DateTime(1999, 3, 1), Value = 3.5m });
        v2.Observations.Add(new FredObservation { VintageDate = v2.VintageDate, Vintage = v2, ObsDate = new DateTime(1999, 4, 1), Value = 4 });


        FredVintage v3 = vintages[2];
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 1, 1), Value = 1 });
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 2, 1), Value = 2 });
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 3, 1), Value = 3 });
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 4, 1), Value = 4 });
        v3.Observations.Add(new FredObservation { VintageDate = v3.VintageDate, Vintage = v3, ObsDate = new DateTime(1999, 5, 1), Value = 5 });
        return vintages;
    }
}
