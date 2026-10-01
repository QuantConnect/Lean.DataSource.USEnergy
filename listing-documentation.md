## Requesting Data

To add US Electricity data to your algorithm, call the **AddData** method. Save a reference to the dataset **Symbol** so you can access the data later in your algorithm.

```python
class EIAElectricityDataAlgorithm(QCAlgorithm):
    def initialize(self) -> None:
        self.set_start_date(2020, 6, 1)
        self.set_end_date(2020, 9, 1)
        self.set_cash(100000)

        self._spy = self.add_equity("SPY", Resolution.DAILY).symbol
        self._pjm = self.add_data(EIAElectricity, EIA.BalancingAuthorities.PJM, Resolution.DAILY).symbol
```

```csharp
public class EIAElectricityDataAlgorithm : QCAlgorithm
{
    private Symbol _spy, _pjm;

    public override void Initialize()
    {
        SetStartDate(2020, 6, 1);
        SetEndDate(2020, 9, 1);
        SetCash(100000);

        _spy = AddEquity("SPY", Resolution.Daily).Symbol;
        _pjm = AddData<EIAElectricity>(EIA.BalancingAuthorities.PJM, Resolution.Daily).Symbol;
    }
}
```

## Accessing Data

To get the current US Electricity data, index the current [**Slice**](https://www.quantconnect.com/docs/v2/writing-algorithms/key-concepts/time-modeling/timeslices) with the dataset **Symbol**. **Slice** objects deliver unique events to your algorithm as they happen, but the **Slice** may not contain data for your dataset at every time step. To avoid issues, check if the **Slice** contains the data you want before you index it.

```python
def on_data(self, slice: Slice) -> None:
    if slice.contains_key(self._pjm):
        data_point = slice[self._pjm]
        self.log(f"{self._pjm} demand at {slice.time}: {data_point.demand}")
```

```csharp
public override void OnData(Slice slice)
{
    if (slice.ContainsKey(_pjm))
    {
        var dataPoint = slice[_pjm];
        Log($"{_pjm} demand at {slice.Time}: {dataPoint.Demand}");
    }
}
```

To iterate through all of the dataset objects in the current **Slice**, call the **Get** method.

```python
def on_data(self, slice: Slice) -> None:
    for dataset_symbol, data_point in slice.get(EIAElectricity).items():
        self.log(f"{dataset_symbol} at {slice.time}: demand {data_point.demand}, net generation {data_point.net_generation}")
```

```csharp
public override void OnData(Slice slice)
{
    foreach (var kvp in slice.Get<EIAElectricity>())
    {
        var datasetSymbol = kvp.Key;
        var dataPoint = kvp.Value;
        Log($"{datasetSymbol} at {slice.Time}: demand {dataPoint.Demand}, net generation {dataPoint.NetGeneration}");
    }
}
```

## Historical Data

To get historical US Electricity data, call the **History** method with the dataset **Symbol**. If there is no data in the period you request, the history result is empty.

```python
# DataFrame
history_df = self.history(self._pjm, 100, Resolution.DAILY)

# Dataset objects
history_bars = self.history[EIAElectricity](self._pjm, 100, Resolution.DAILY)
```

```csharp
var history = History<EIAElectricity>(_pjm, 100, Resolution.Daily);
```

For more information about historical data, see [History Requests](https://www.quantconnect.com/docs/v2/writing-algorithms/historical-data/history-requests).

## Remove Subscriptions

To remove your subscription to US Electricity data, call the **RemoveSecurity** method.

```python
self.remove_security(self._pjm)
```

```csharp
RemoveSecurity(_pjm);
```
