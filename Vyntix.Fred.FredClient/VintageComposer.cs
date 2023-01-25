namespace LeaderAnalytics.Vyntix.Fred.FredClient;

public class VintageComposer : IVintageComposer
{
    public List<IVintage> MakeDense(List<IVintage> vintages)
    {
        foreach (Vintage v in vintages.Where(x => x.Observations != null))
            foreach (Observation o in v.Observations)
                o.Vintage = v;

        IList<IObservation> denseObservations = MakeDense(vintages.SelectMany(x => x.Observations).ToList());
        List<IVintage> denseVintages = new List<IVintage>();

        foreach (var grp in denseObservations.GroupBy(x => new { x.Vintage.VintageDate, x.Vintage.Symbol }))
        {
            IVintage v = grp.First().Vintage;
            v.Observations = grp.ToList();
            denseVintages.Add(v);
        }

        foreach (IVintage emptyVintage in vintages.Where(x => !(x.Observations?.Any() ?? false)).OrderBy(x => x.VintageDate))
        {
            IVintage copyFrom = denseVintages.OrderByDescending(x => x.VintageDate).FirstOrDefault(x => x.Symbol == emptyVintage.Symbol && x.VintageDate < emptyVintage.VintageDate);

            if (copyFrom != null)
                emptyVintage.Observations = copyFrom.Observations;

            denseVintages.Add(emptyVintage);
        }

        return denseVintages;
    }

    public List<IObservation> MakeDense(List<IObservation> sparse)
    {
        List<IObservation> dense = new List<IObservation>();

        if (!(sparse?.Any() ?? false))
            return dense;

        foreach (var grp in sparse.GroupBy(x => x.Symbol))
        {
            List<IObservation> sparseGrp = grp.OrderBy(x => x.VintageDate).ThenBy(x => x.ObsDate).ToList();
            IVintage vintage = new Vintage { VintageDate = DateTime.MinValue };
            Dictionary<DateTime, IObservation> dict = new Dictionary<DateTime, IObservation>();

            for (int i = 0; i < sparseGrp.Count; i++)
            {
                IObservation o = sparseGrp[i];

                if (o.VintageDate != vintage.VintageDate)
                {
                    CopyDictToDenseObs(vintage, dict, dense);
                    vintage.VintageDate = o.VintageDate;
                }

                dict.TryGetValue(o.ObsDate, out IObservation existing);

                if (existing == null || existing.Value != o.Value)
                    dict[o.ObsDate] = o;
            }
            CopyDictToDenseObs(vintage, dict, dense);
        }
        return dense;
    }

    public List<IObservation> MakeSparse(List<IObservation> dense)
    {
        if (!(dense?.Any() ?? false))
            throw new ArgumentNullException(nameof(dense));

        List<IObservation> sparse = new(dense.Count);
        string lastSymbol = null;
        DateTime lastObsDate = DateTime.MinValue;
        string lastValue = null;

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

    private void CopyDictToDenseObs(IVintage vintage, Dictionary<DateTime, IObservation> dict, List<IObservation> denseObs)
    {
        foreach (IObservation d in dict.Values)
        {
            IObservation newObs;
            // obs is new if vintage date is != vintage.VintageDate

            if (d.VintageDate != vintage.VintageDate)
                newObs = new Observation { Symbol = d.Symbol, VintageDate = vintage.VintageDate, ObsDate = d.ObsDate, Value = d.Value };
            else
                newObs = d;

            denseObs.Add(newObs);
        }
    }
}
