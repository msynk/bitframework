using System.Collections.Generic;
using System.Linq;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bit.BlazorUI.Tests.Components.Extras.Chart;

[TestClass]
public class BitChartTests : BunitTestContext
{
    private static BitChartConfig CreateConfig(BitSide legendPosition) => new(
        BitChartType.Bar,
        new BitChartData
        {
            Labels = ["A", "B"],
            Datasets = [new BitChartDataset { Label = "Series", Data = [1, 2] }]
        },
        new BitChartOptions { Plugins = { Legend = { Position = legendPosition } } });

    private IRenderedComponent<BitChart> RenderChart(BitSide legendPosition)
        => RenderComponent<BitChart>(parameters => parameters.Add(p => p.Config, CreateConfig(legendPosition)));

    [TestMethod]
    [DataRow(BitSide.Top, "bc-root")]
    [DataRow(BitSide.Bottom, "bc-root")]
    [DataRow(BitSide.Left, "bc-mid")]
    [DataRow(BitSide.Right, "bc-mid")]
    // A chart is laid out physically, so the logical and combined sides have no edge of their own
    // here - they leave the legend at the top instead of dropping it.
    [DataRow(BitSide.Start, "bc-root")]
    [DataRow(BitSide.End, "bc-root")]
    [DataRow(BitSide.TopAndBottom, "bc-root")]
    [DataRow(BitSide.StartAndEnd, "bc-root")]
    public void LegendPositionFallsBackToTheTop(BitSide position, string expectedParentClass)
    {
        var component = RenderChart(position);

        var legends = component.FindAll(".bc-legend");

        Assert.AreEqual(1, legends.Count);
        Assert.IsTrue(legends[0].ParentElement!.ClassList.Contains(expectedParentClass));
    }

    [TestMethod]
    public void LegendPositionDecidesTheLegendOrientation()
    {
        // Only the two physical inline sides stack the items vertically; every other side is a row.
        foreach (var position in new[] { BitSide.Left, BitSide.Right })
        {
            Assert.IsTrue(RenderChart(position).Find(".bc-legend").ClassList.Contains("bc-legend-v"));
        }

        foreach (var position in new[] { BitSide.Top, BitSide.Bottom, BitSide.Start, BitSide.End, BitSide.TopAndBottom, BitSide.StartAndEnd })
        {
            Assert.IsTrue(RenderChart(position).Find(".bc-legend").ClassList.Contains("bc-legend-h"));
        }
    }
}
