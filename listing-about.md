## Introduction

The US Electricity dataset by the Energy Information Administration (EIA) tracks daily demand and generation on the US power grid, with generation broken down by fuel type. The data covers 81 balancing authorities, starts in January 2019, and is delivered on a daily frequency. This dataset is created by processing the Form EIA-930 data that balancing authorities report to the EIA. Some balancing authorities do not report the fuel mix.

## About the Provider

The U.S. Energy Information Administration (EIA) is the statistical agency of the US Department of Energy, created by Congress in 1977. It collects and publishes energy data that is independent of policy. Policymakers, markets, and the public use its data to understand energy in the United States.

## Getting Started

The following snippet demonstrates how to request data from the US Electricity dataset:

```python
self.dataset_symbol = self.add_data(EIAElectricity, EIA.BalancingAuthorities.PJM, Resolution.DAILY).symbol
```

```csharp
_datasetSymbol = AddData<EIAElectricity>(EIA.BalancingAuthorities.PJM, Resolution.Daily).Symbol;
```

## Data Summary

The following table describes the dataset properties:

| Property | Value |
| --- | --- |
| Start Date | January 2019 |
| Asset Coverage | 81 US Balancing Authorities |
| Data Density | Regular |
| Resolution | Daily |
| Timezone | New York |

## Example Applications

The US Electricity dataset enables you to trade on the daily state of the US power grid. Examples include the following strategies:

- Trading utility stocks when actual demand runs above the day-ahead forecast
- Trading natural gas based on how much power comes from gas plants
- Trading coal and gas producers as their share of generation shifts

For more example algorithms, see [Examples](/datasets/eia-us-electricity/examples).

## Supported Balancing Authorities

The following table shows the accessor code you need to add each major balancing authority to your algorithm:

| Constant | Balancing Authority |
| --- | --- |
| EIA.BalancingAuthorities.PJM | PJM Interconnection, the largest balancing authority in the country |
| EIA.BalancingAuthorities.ERCOT | Electric Reliability Council of Texas |
| EIA.BalancingAuthorities.CAISO | California Independent System Operator |
| EIA.BalancingAuthorities.MISO | Midcontinent Independent System Operator |
| EIA.BalancingAuthorities.NYISO | New York Independent System Operator |
| EIA.BalancingAuthorities.ISONE | ISO New England |
| EIA.BalancingAuthorities.SPP | Southwest Power Pool |
| EIA.BalancingAuthorities.BPA | Bonneville Power Administration |

## Meta

| Field | Value |
| --- | --- |
| name | US Electricity |
| url | eia-us-electricity |
| vendorName | Energy Information Administration |
| website | https://www.eia.gov/ |
| history | January 2019 |
| reach | 81 Balancing Authorities |
| shortDescription | Demand and generation information for the US power grid. |
| priceCTA | Free in Cloud |
| delivery | cloud & download |
