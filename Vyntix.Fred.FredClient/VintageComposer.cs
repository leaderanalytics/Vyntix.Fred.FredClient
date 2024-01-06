namespace LeaderAnalytics.Vyntix.Fred.FredClient;

public class VintageComposer : IVintageComposer
{
    public List<IFredVintage> MakeDense(List<IFredVintage> vintages)
    {
        foreach (IFredVintage v in vintages.Where(x => x.Observations != null))
            foreach (FredObservation o in v.Observations)
                o.Vintage = v;

        IList<IFredObservation> denseObservations = MakeDense(vintages.SelectMany(x => x.Observations).ToList());
        List<IFredVintage> denseVintages = new List<IFredVintage>();

        foreach (var grp in denseObservations.GroupBy(x => new { x.Vintage.VintageDate, x.Vintage.Symbol }))
        {
            IFredVintage v = grp.First().Vintage;
            v.Observations = grp.ToList();
            denseVintages.Add(v);
        }

        foreach (IFredVintage emptyVintage in vintages.Where(x => !(x.Observations?.Any() ?? false)).OrderBy(x => x.VintageDate))
        {
            IFredVintage copyFrom = denseVintages.OrderByDescending(x => x.VintageDate).FirstOrDefault(x => x.Symbol == emptyVintage.Symbol && x.VintageDate < emptyVintage.VintageDate);

            if (copyFrom != null)
                emptyVintage.Observations = copyFrom.Observations;

            denseVintages.Add(emptyVintage);
        }

        return denseVintages;
    }

    public List<IFredObservation> MakeDense(List<IFredObservation> sparse)
    {
        List<IFredObservation> dense = new List<IFredObservation>();

        if (!(sparse?.Any() ?? false))
            return dense;

        foreach (var grp in sparse.GroupBy(x => x.Symbol))
        {
            List<IFredObservation> sparseGrp = grp.OrderBy(x => x.VintageDate).ThenBy(x => x.ObsDate).ToList();
            IFredVintage vintage = new FredVintage { VintageDate = DateTime.MinValue };
            Dictionary<DateTime, IFredObservation> dict = new Dictionary<DateTime, IFredObservation>();

            for (int i = 0; i < sparseGrp.Count; i++)
            {
                IFredObservation o = sparseGrp[i];

                if (o.VintageDate != vintage.VintageDate)
                {
                    CopyDictToDenseObs(vintage, dict, dense);
                    vintage.VintageDate = o.VintageDate;
                }

                dict.TryGetValue(o.ObsDate, out IFredObservation existing);

                if (existing == null || existing.Value != o.Value)
                    dict[o.ObsDate] = o;
            }
            CopyDictToDenseObs(vintage, dict, dense);
        }
        return dense;
    }

    public List<IFredObservation> MakeSparse(List<IFredObservation> dense)
    {
        if (!(dense?.Any() ?? false))
            throw new ArgumentNullException(nameof(dense));

        List<IFredObservation> sparse = new(dense.Count);
        string lastSymbol = null;
        DateTime lastObsDate = DateTime.MinValue;
        decimal? lastValue = null;

        foreach (var obs in dense.OrderBy(x => x.Symbol).ThenBy(x => x.ObsDate).ThenBy(x => x.VintageDate))
        {
            if (obs.Symbol != lastSymbol || obs.ObsDate != lastObsDate || obs.Value != lastValue)
                sparse.Add(obs);

            lastSymbol= obs.Symbol;
            lastObsDate = obs.ObsDate;
            lastValue = obs.Value;
        }
        return sparse;
    }

    private void CopyDictToDenseObs(IFredVintage vintage, Dictionary<DateTime, IFredObservation> dict, List<IFredObservation> denseObs)
    {
        foreach (IFredObservation d in dict.Values)
        {
            IFredObservation newObs;
            // obs is new if vintage date is != vintage.VintageDate

            if (d.VintageDate != vintage.VintageDate)
                newObs = new FredObservation { Symbol = d.Symbol, VintageDate = vintage.VintageDate, ObsDate = d.ObsDate, Value = d.Value };
            else
                newObs = d;

            denseObs.Add(newObs);
        }
    }
}
