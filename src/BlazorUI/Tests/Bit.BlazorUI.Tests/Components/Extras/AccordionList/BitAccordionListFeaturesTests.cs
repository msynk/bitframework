using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bit.BlazorUI.Tests.Components.Extras.AccordionList;

[TestClass]
public class BitAccordionListFeaturesTests : BunitTestContext
{
    private static List<BitAccordionListItem> GetItems() =>
    [
        new() { Key = "a", Title = "Item A", Body = Content("Body A") },
        new() { Key = "b", Title = "Item B", Body = Content("Body B") },
        new() { Key = "c", Title = "Item C", Body = Content("Body C") },
    ];

    private static RenderFragment<BitAccordionListItem> Content(string text) => item => builder => builder.AddContent(0, text);


    [TestMethod]
    public void BitAccordionListShouldNotCollapseTheLastItemWhenNotCollapsible()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.Collapsible, false);
            parameters.Add(p => p.DefaultExpandedKey, "a");
        });

        component.FindAll(".bit-acd-hdr")[0].Click();

        Assert.IsTrue(component.FindAll(".bit-acd-con")[0].ClassList.Contains("bit-acd-cex"));
        Assert.AreEqual("true", component.FindAll(".bit-acd-hdr")[0].GetAttribute("aria-disabled"));

        // Another item can still take its place, and the one that opens becomes the one that is locked.
        component.FindAll(".bit-acd-hdr")[1].Click();

        Assert.IsFalse(component.FindAll(".bit-acd-con")[0].ClassList.Contains("bit-acd-cex"));
        Assert.IsTrue(component.FindAll(".bit-acd-con")[1].ClassList.Contains("bit-acd-cex"));
        Assert.IsNull(component.FindAll(".bit-acd-hdr")[0].GetAttribute("aria-disabled"));
        Assert.AreEqual("true", component.FindAll(".bit-acd-hdr")[1].GetAttribute("aria-disabled"));
    }

    [TestMethod]
    public void BitAccordionListNotCollapsibleShouldOnlyLockTheLastExpandedItemInMultiple()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Multiple, true);
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.Collapsible, false);
            parameters.Add(p => p.DefaultExpandedKeys, ["a", "b"]);
        });

        // Two are open, so either of them can still be closed.
        component.FindAll(".bit-acd-hdr")[0].Click();
        Assert.IsFalse(component.FindAll(".bit-acd-con")[0].ClassList.Contains("bit-acd-cex"));

        // The one that is left cannot.
        component.FindAll(".bit-acd-hdr")[1].Click();
        Assert.IsTrue(component.FindAll(".bit-acd-con")[1].ClassList.Contains("bit-acd-cex"));
    }

    [TestMethod]
    public async Task BitAccordionListNotCollapsibleShouldStillCollapseFromTheCollapseAllMethod()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.Collapsible, false);
            parameters.Add(p => p.DefaultExpandedKey, "a");
        });

        await component.InvokeAsync(() => component.Instance.CollapseAll());

        component.WaitForAssertion(() => Assert.AreEqual(0, component.FindAll(".bit-acd-con.bit-acd-cex").Count));
    }

    [TestMethod]
    public void BitAccordionListReadOnlyShouldReportTheClickWithoutToggling()
    {
        var clicked = 0;

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.ReadOnly, true);
            parameters.Add(p => p.OnItemClick, (BitAccordionListItem i) => clicked++);
        });

        component.FindAll(".bit-acd-hdr")[0].Click();

        Assert.AreEqual(1, clicked);
        Assert.AreEqual(0, component.FindAll(".bit-acd-con.bit-acd-cex").Count);
        Assert.AreEqual("true", component.FindAll(".bit-acd-hdr")[0].GetAttribute("aria-disabled"));
    }

    [TestMethod]
    public void BitAccordionListItemReadOnlyShouldOverrideTheListValue()
    {
        var items = GetItems();
        items[0].ReadOnly = true;
        items[1].ReadOnly = false;

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Multiple, true);
            parameters.Add(p => p.Items, items);
        });

        component.FindAll(".bit-acd-hdr")[0].Click();
        component.FindAll(".bit-acd-hdr")[1].Click();

        Assert.IsFalse(component.FindAll(".bit-acd-con")[0].ClassList.Contains("bit-acd-cex"));
        Assert.IsTrue(component.FindAll(".bit-acd-con")[1].ClassList.Contains("bit-acd-cex"));
    }

    [TestMethod]
    public void BitAccordionListShouldNotToggleADisabledItem()
    {
        var items = GetItems();
        items[0].IsEnabled = false;

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
        });

        component.FindAll(".bit-acd-hdr")[0].Click();

        Assert.AreEqual(0, component.FindAll(".bit-acd-con.bit-acd-cex").Count);
        Assert.IsTrue(component.FindAll(".bit-acd")[0].ClassList.Contains("bit-dis"));
    }

    [TestMethod]
    public async Task BitAccordionListExpandAllShouldSkipTheDisabledItems()
    {
        var items = GetItems();
        items[1].IsEnabled = false;

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Multiple, true);
            parameters.Add(p => p.Items, items);
        });

        await component.InvokeAsync(() => component.Instance.ExpandAll());

        component.WaitForAssertion(() => Assert.AreEqual(2, component.FindAll(".bit-acd-con.bit-acd-cex").Count));
    }

    [TestMethod]
    public void BitAccordionListShouldApplyTheSizeToEveryItem()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.Size, BitSize.Large);
        });

        Assert.AreEqual(3, component.FindAll(".bit-acd.bit-acd-lg").Count);
    }

    [TestMethod]
    public void BitAccordionListShouldApplyTheExpanderIconPositionToEveryItem()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.ExpanderIconPosition, BitIconPosition.Start);
        });

        Assert.AreEqual(3, component.FindAll(".bit-acd.bit-acd-sei").Count);
    }

    [TestMethod]
    public void BitAccordionListShouldHideTheExpanderIconAndLetAnItemOptOut()
    {
        var items = GetItems();
        items[0].HideExpanderIcon = false;

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
            parameters.Add(p => p.HideExpanderIcon, true);
        });

        Assert.AreEqual(1, component.FindAll(".bit-acd-eiw").Count);
    }

    [TestMethod]
    public void BitAccordionListShouldRenderTheItemIconAndTheExpandedExpanderIcon()
    {
        var items = GetItems();
        items[0].IconName = "Settings";

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
            parameters.Add(p => p.DefaultExpandedKey, "a");
            parameters.Add(p => p.ExpandedExpanderIconName, "Remove");
        });

        Assert.AreEqual(1, component.FindAll(".bit-acd-ico").Count);
        Assert.IsTrue(component.FindAll(".bit-acd-eic")[0].ClassList.Contains("bit-icon--Remove"));
        Assert.IsFalse(component.FindAll(".bit-acd-eic")[1].ClassList.Contains("bit-icon--Remove"));
    }

    [TestMethod]
    public void BitAccordionListShouldRenderTheActionsBesideTheHeader()
    {
        var items = GetItems();
        items[0].Actions = item => builder => builder.AddContent(0, "action");

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
        });

        Assert.AreEqual(1, component.FindAll(".bit-acd-act").Count);
        Assert.AreEqual("action", component.Find(".bit-acd-act").TextContent.Trim());
    }

    [TestMethod]
    public void BitAccordionListShouldRenderTheActionsTemplateForEveryItem()
    {
        RenderFragment<BitAccordionListItem> actionsTemplate = item => builder => builder.AddContent(0, item.Key);

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.ActionsTemplate, actionsTemplate);
        });

        Assert.AreEqual(3, component.FindAll(".bit-acd-act").Count);
    }

    [TestMethod]
    public void BitAccordionListShouldRenderTheTitleAndExpanderTemplates()
    {
        RenderFragment<BitAccordionListItem> titleTemplate = item => builder => builder.AddContent(0, $"T-{item.Key}");
        RenderFragment<BitAccordionListItem> expanderTemplate = item => builder => builder.AddContent(0, $"E-{item.Key}");

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.TitleTemplate, titleTemplate);
            parameters.Add(p => p.ExpanderTemplate, expanderTemplate);
        });

        Assert.AreEqual("T-a", component.FindAll(".bit-acd-ttl")[0].TextContent.Trim());
        Assert.AreEqual("E-a", component.FindAll(".bit-acd-eiw")[0].TextContent.Trim());
    }

    [TestMethod]
    public void BitAccordionListShouldApplyTheHeadingLevelToEveryItem()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.HeadingLevel, 2);
        });

        Assert.AreEqual("2", component.FindAll(".bit-acd-hed")[0].GetAttribute("aria-level"));
    }

    [TestMethod]
    public void BitAccordionListShouldDropTheContentRegionRole()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.NoContentRegion, true);
        });

        Assert.IsNull(component.FindAll(".bit-acd-con")[0].GetAttribute("role"));
    }

    [TestMethod]
    public void BitAccordionListShouldApplyTheLayoutParametersToEveryItem()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.MaxHeight, "100px");
            parameters.Add(p => p.TransitionDuration, 250);
            parameters.Add(p => p.ExpandOnPrint, true);
            parameters.Add(p => p.Gap, 8);
        });

        var first = component.FindAll(".bit-acd")[0];
        Assert.IsTrue(first.ClassList.Contains("bit-acd-mxh"));
        Assert.IsTrue(first.ClassList.Contains("bit-acd-eop"));
        Assert.IsTrue(first.GetAttribute("style")!.Contains("--bit-acd-max-h:100px"));
        Assert.IsTrue(first.GetAttribute("style")!.Contains("--bit-acd-dur-full:250ms"));
        Assert.IsTrue(component.Find(".bit-acl").GetAttribute("style")!.Contains("gap:8px"));
    }

    [TestMethod]
    public void BitAccordionListLazyContentShouldDelayTheFirstRenderOfTheBody()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.LazyContent, true);
        });

        Assert.AreEqual(string.Empty, component.FindAll(".bit-acd-con")[0].TextContent.Trim());

        component.FindAll(".bit-acd-hdr")[0].Click();

        Assert.AreEqual("Body A", component.FindAll(".bit-acd-con")[0].TextContent.Trim());
    }

    [TestMethod]
    public void BitAccordionListUnmountOnCollapseShouldRemoveTheBodyAgain()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.UnmountOnCollapse, true);
        });

        component.FindAll(".bit-acd-hdr")[0].Click();
        Assert.AreEqual("Body A", component.FindAll(".bit-acd-con")[0].TextContent.Trim());

        component.FindAll(".bit-acd-hdr")[0].Click();
        Assert.AreEqual(string.Empty, component.FindAll(".bit-acd-con")[0].TextContent.Trim());
    }

    [TestMethod]
    public void BitAccordionListShouldCancelTheToggleFromOnToggling()
    {
        BitAccordionListToggleArgs<BitAccordionListItem>? received = null;

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.OnToggling, (BitAccordionListToggleArgs<BitAccordionListItem> args) =>
            {
                received = args;
                args.Cancel = true;
            });
        });

        component.FindAll(".bit-acd-hdr")[0].Click();

        Assert.IsNotNull(received);
        Assert.AreEqual("a", received!.Key);
        Assert.IsTrue(received.IsExpanding);
        Assert.AreEqual(BitAccordionToggleReason.Click, received.Reason);
        Assert.AreEqual(0, component.FindAll(".bit-acd-con.bit-acd-cex").Count);
    }

    [TestMethod]
    public void BitAccordionListOnTogglingShouldLeaveThePreviouslyExpandedItemAloneWhenCancelled()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.DefaultExpandedKey, "a");
            parameters.Add(p => p.OnToggling, (BitAccordionListToggleArgs<BitAccordionListItem> args) => args.Cancel = true);
        });

        component.FindAll(".bit-acd-hdr")[1].Click();

        Assert.IsTrue(component.FindAll(".bit-acd-con")[0].ClassList.Contains("bit-acd-cex"));
        Assert.IsFalse(component.FindAll(".bit-acd-con")[1].ClassList.Contains("bit-acd-cex"));
    }

    [TestMethod]
    public async Task BitAccordionListOnTogglingShouldReportTheMethodReason()
    {
        BitAccordionToggleReason? reason = null;

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.OnToggling, (BitAccordionListToggleArgs<BitAccordionListItem> args) => reason = args.Reason);
        });

        await component.InvokeAsync(() => component.Instance.Expand("b"));

        Assert.AreEqual(BitAccordionToggleReason.Method, reason);
    }

    [TestMethod]
    public async Task BitAccordionListShouldExpandCollapseAndToggleByKey()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
        });

        await component.InvokeAsync(() => component.Instance.Expand("b"));
        Assert.IsTrue(component.Instance.IsExpanded("b"));
        Assert.IsTrue(component.FindAll(".bit-acd-con")[1].ClassList.Contains("bit-acd-cex"));

        // Single-expand mode collapses the previously expanded item along the way.
        await component.InvokeAsync(() => component.Instance.Expand("c"));
        Assert.IsFalse(component.Instance.IsExpanded("b"));
        CollectionAssert.AreEqual(new[] { "c" }, component.Instance.GetExpandedKeys().ToArray());

        await component.InvokeAsync(() => component.Instance.Toggle("c"));
        Assert.IsFalse(component.Instance.IsExpanded("c"));

        await component.InvokeAsync(() => component.Instance.Toggle("c"));
        Assert.IsTrue(component.Instance.IsExpanded("c"));

        await component.InvokeAsync(() => component.Instance.Collapse("c"));
        Assert.AreEqual(0, component.Instance.GetExpandedKeys().Count);
    }

    [TestMethod]
    public async Task BitAccordionListShouldIgnoreAnUnknownKeyInTheMethods()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
        });

        await component.InvokeAsync(() => component.Instance.Expand("nope"));

        Assert.AreEqual(0, component.Instance.GetExpandedKeys().Count);
    }

    [TestMethod]
    public async Task BitAccordionListCollapseAllShouldDropTheKeysThatMapToNoItem()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Multiple, true);
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.DefaultExpandedKeys, ["a", "orphan"]);
        });

        CollectionAssert.AreEqual(new[] { "a", "orphan" }, component.Instance.GetExpandedKeys().ToArray());

        await component.InvokeAsync(() => component.Instance.CollapseAll());

        Assert.AreEqual(0, component.Instance.GetExpandedKeys().Count);
    }

    [TestMethod]
    public void BitAccordionListShouldNoticeAMutationOfTheVeryCollectionItWasGiven()
    {
        var items = GetItems();

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
        });

        items.Add(new BitAccordionListItem { Key = "d", Title = "Item D" });

        component.Render();

        Assert.AreEqual(4, component.FindAll(".bit-acd").Count);
    }

    [TestMethod]
    public void BitAccordionListShouldKeepTheExpandedStateWhenTheItemsChange()
    {
        var items = GetItems();

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
        });

        component.FindAll(".bit-acd-hdr")[1].Click();
        Assert.IsTrue(component.Instance.IsExpanded("b"));

        items.Add(new BitAccordionListItem { Key = "d", Title = "Item D" });
        component.Render();

        Assert.IsTrue(component.Instance.IsExpanded("b"));
        Assert.IsTrue(component.FindAll(".bit-acd-con")[1].ClassList.Contains("bit-acd-cex"));
    }

    [TestMethod]
    public void BitAccordionListShouldWorkWithACustomTypeThatCarriesNoKey()
    {
        var items = new List<KeylessItem> { new() { Name = "X" }, new() { Name = "Y" } };

        var component = RenderComponent<BitAccordionList<KeylessItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
            parameters.Add(p => p.NameSelectors, new BitAccordionListNameSelectors<KeylessItem>
            {
                Title = { Selector = i => i.Name },
            });
        });

        component.FindAll(".bit-acd-hdr")[1].Click();

        Assert.IsTrue(component.FindAll(".bit-acd-con")[1].ClassList.Contains("bit-acd-cex"));
        Assert.IsFalse(component.FindAll(".bit-acd-con")[0].ClassList.Contains("bit-acd-cex"));
    }

    [TestMethod]
    public void BitAccordionListShouldWorkWithACustomTypeWhoseStateIsReadOnly()
    {
        var items = new List<ReadOnlyStateItem> { new() { Name = "X" }, new() { Name = "Y" } };

        var component = RenderComponent<BitAccordionList<ReadOnlyStateItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
            parameters.Add(p => p.NameSelectors, new BitAccordionListNameSelectors<ReadOnlyStateItem>
            {
                Title = { Selector = i => i.Name },
            });
        });

        component.FindAll(".bit-acd-hdr")[0].Click();

        Assert.IsTrue(component.FindAll(".bit-acd-con")[0].ClassList.Contains("bit-acd-cex"));
        Assert.IsTrue(component.Instance.IsExpanded("X"));
    }

    [TestMethod]
    public void BitAccordionListShouldMoveTheFocusBetweenTheHeadersWithTheArrowKeys()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
        });

        var wrappers = component.FindAll(".bit-acl-itm");
        Assert.AreEqual(3, wrappers.Count);

        // The focus itself is a JS call the loose interop swallows, so what is asserted here is that the
        // navigation runs over the headers without throwing and leaves the expanded state alone.
        wrappers[0].KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        wrappers[0].KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        wrappers[0].KeyDown(new KeyboardEventArgs { Key = "Home" });
        wrappers[0].KeyDown(new KeyboardEventArgs { Key = "End" });

        Assert.AreEqual(0, component.FindAll(".bit-acd-con.bit-acd-cex").Count);
    }

    [TestMethod]
    public void BitAccordionListShouldIgnoreTheNavigationKeysWhenNotNavigable()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.Navigable, false);
        });

        component.FindAll(".bit-acl-itm")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        Assert.AreEqual(0, component.FindAll(".bit-acd-con.bit-acd-cex").Count);
    }

    [TestMethod]
    public async Task BitAccordionListShouldFocusAnItemByKey()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
        });

        await component.InvokeAsync(() => component.Instance.FocusItem("b"));
        await component.InvokeAsync(() => component.Instance.FocusAsync());
    }

    [TestMethod]
    public void BitAccordionListShouldRenderTheAriaLabelOnTheRoot()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.AriaLabel, "settings");
        });

        Assert.AreEqual("settings", component.Find(".bit-acl").GetAttribute("aria-label"));
    }

    [TestMethod]
    public void BitAccordionListShouldPassTheClassesAndStylesToEveryPartOfTheItems()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.Classes, new BitAccordionListClassStyles
            {
                ItemHeaderWrapper = "custom-hwr",
                ItemHeading = "custom-hed",
                ItemContentWrapper = "custom-cwr",
            });
            parameters.Add(p => p.Styles, new BitAccordionListClassStyles
            {
                ItemTitle = "color: red;",
            });
        });

        Assert.AreEqual(3, component.FindAll(".bit-acd-hwr.custom-hwr").Count);
        Assert.AreEqual(3, component.FindAll(".bit-acd-hed.custom-hed").Count);
        Assert.AreEqual(3, component.FindAll(".bit-acd-cwr.custom-cwr").Count);
        Assert.AreEqual("color: red;", component.FindAll(".bit-acd-ttl")[0].GetAttribute("style"));
    }

    [TestMethod]
    public void BitAccordionListOptionShouldExpandFromItsOwnHeaderClick()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListOption>>(parameters =>
        {
            parameters.AddChildContent<BitAccordionListOption>(p => p.Add(o => o.Title, "Option A"));
            parameters.AddChildContent<BitAccordionListOption>(p => p.Add(o => o.Title, "Option B"));
        });

        component.WaitForAssertion(() => Assert.AreEqual(2, component.FindAll(".bit-acd-hdr").Count));

        component.FindAll(".bit-acd-hdr")[1].Click();

        component.WaitForAssertion(() => Assert.IsTrue(component.FindAll(".bit-acd-con")[1].ClassList.Contains("bit-acd-cex")));
    }

    [TestMethod]
    public void BitAccordionListOptionShouldRenderItsOwnIconAndActions()
    {
        RenderFragment<BitAccordionListOption> actions = option => builder => builder.AddContent(0, "act");

        var component = RenderComponent<BitAccordionList<BitAccordionListOption>>(parameters =>
        {
            parameters.AddChildContent<BitAccordionListOption>(p =>
            {
                p.Add(o => o.Title, "Option A");
                p.Add(o => o.IconName, "Settings");
                p.Add(o => o.Actions, actions);
            });
        });

        component.WaitForAssertion(() =>
        {
            Assert.AreEqual(1, component.FindAll(".bit-acd-ico").Count);
            Assert.AreEqual("act", component.Find(".bit-acd-act").TextContent.Trim());
        });
    }

    [TestMethod]
    public void BitAccordionListOptionReadOnlyShouldKeepThePanelWhereItIs()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListOption>>(parameters =>
        {
            parameters.AddChildContent<BitAccordionListOption>(p =>
            {
                p.Add(o => o.Title, "Option A");
                p.Add(o => o.ReadOnly, true);
            });
        });

        component.WaitForAssertion(() => Assert.AreEqual(1, component.FindAll(".bit-acd-hdr").Count));

        component.FindAll(".bit-acd-hdr")[0].Click();

        Assert.AreEqual(0, component.FindAll(".bit-acd-con.bit-acd-cex").Count);
    }


    [TestMethod]
    public void BitAccordionListShouldNameAHeaderThatDoesNotNameItself()
    {
        var items = GetItems();
        items[0].HeaderAriaLabel = "Notifications";

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
        });

        Assert.AreEqual("Notifications", component.FindAll(".bit-acd-hdr")[0].GetAttribute("aria-label"));
        Assert.IsNull(component.FindAll(".bit-acd-hdr")[1].GetAttribute("aria-label"));
    }

    [TestMethod]
    public void BitAccordionListShouldRenderTheHeaderAndBodyTemplatesOfTheList()
    {
        RenderFragment<BitAccordionListItem> headerTemplate = item => builder => builder.AddContent(0, $"H-{item.Key}");
        RenderFragment<BitAccordionListItem> bodyTemplate = item => builder => builder.AddContent(0, $"B-{item.Key}");

        // The templates of the list stand in for the items that bring none of their own.
        List<BitAccordionListItem> items = [new() { Key = "a", Title = "Item A" }, new() { Key = "b", Title = "Item B" }];

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
            parameters.Add(p => p.HeaderTemplate, headerTemplate);
            parameters.Add(p => p.BodyTemplate, bodyTemplate);
        });

        Assert.AreEqual("H-a", component.FindAll(".bit-acd-hdr")[0].TextContent.Trim());
        Assert.AreEqual("B-a", component.FindAll(".bit-acd-con")[0].TextContent.Trim());
    }

    [TestMethod]
    public void BitAccordionListItemTemplatesShouldWinOverTheTemplatesOfTheList()
    {
        RenderFragment<BitAccordionListItem> listTemplate = item => builder => builder.AddContent(0, "from-the-list");

        var items = GetItems();
        items[0].HeaderTemplate = item => builder => builder.AddContent(0, "from-the-item");
        items[0].Actions = item => builder => builder.AddContent(0, "item-actions");

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
            parameters.Add(p => p.HeaderTemplate, listTemplate);
            parameters.Add(p => p.ActionsTemplate, listTemplate);
        });

        Assert.AreEqual("from-the-item", component.FindAll(".bit-acd-hdr")[0].TextContent.Trim());
        Assert.AreEqual("from-the-list", component.FindAll(".bit-acd-hdr")[1].TextContent.Trim());
        Assert.AreEqual("item-actions", component.FindAll(".bit-acd-act")[0].TextContent.Trim());
        Assert.AreEqual("from-the-list", component.FindAll(".bit-acd-act")[1].TextContent.Trim());
    }

    [TestMethod]
    public void BitAccordionListShouldKeepTheExpanderIconStill()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
            parameters.Add(p => p.NoExpanderRotation, true);
            parameters.Add(p => p.DefaultExpandedKey, "a");
        });

        Assert.IsFalse(component.FindAll(".bit-acd-eiw")[0].ClassList.Contains("bit-ico--r180"));
    }

    [TestMethod]
    public void BitAccordionListShouldRenderTheOptionsAlias()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListOption>>(parameters =>
        {
            parameters.Add(p => p.Options, builder =>
            {
                builder.OpenComponent<BitAccordionListOption>(0);
                builder.AddComponentParameter(1, nameof(BitAccordionListOption.Title), "Option A");
                builder.CloseComponent();
            });
        });

        component.WaitForAssertion(() => Assert.AreEqual(1, component.FindAll(".bit-acd").Count));
    }

    [TestMethod]
    public void BitAccordionListShouldReadTheNewMembersOfACustomTypeThroughNameSelectors()
    {
        var items = new List<RichItem>
        {
            new() { Id = "x", Name = "X", Glyph = "Settings", Locked = true, NoChevron = true },
            new() { Id = "y", Name = "Y" },
        };

        var component = RenderComponent<BitAccordionList<RichItem>>(parameters =>
        {
            parameters.Add(p => p.Items, items);
            parameters.Add(p => p.NameSelectors, new BitAccordionListNameSelectors<RichItem>
            {
                Key = { Selector = i => i.Id },
                Title = { Selector = i => i.Name },
                IconName = { Selector = i => i.Glyph },
                ReadOnly = { Selector = i => i.Locked },
                HideExpanderIcon = { Selector = i => i.NoChevron },
            });
        });

        Assert.AreEqual(1, component.FindAll(".bit-acd-ico").Count);
        Assert.AreEqual(1, component.FindAll(".bit-acd-eiw").Count);

        // The read-only item does not answer the click, the other one does.
        component.FindAll(".bit-acd-hdr")[0].Click();
        Assert.AreEqual(0, component.FindAll(".bit-acd-con.bit-acd-cex").Count);

        component.FindAll(".bit-acd-hdr")[1].Click();
        Assert.AreEqual(1, component.FindAll(".bit-acd-con.bit-acd-cex").Count);
    }

    [TestMethod]
    public async Task BitAccordionListShouldIgnoreAnEmptyKeyInTheMethods()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Items, GetItems());
        });

        await component.InvokeAsync(() => component.Instance.Expand(string.Empty));
        await component.InvokeAsync(() => component.Instance.Toggle(string.Empty));

        Assert.AreEqual(0, component.Instance.GetExpandedKeys().Count);
        Assert.IsFalse(component.Instance.IsExpanded(null));
    }


    public class RichItem
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Glyph { get; set; }

        public bool? Locked { get; set; }

        public bool? NoChevron { get; set; }
    }

    [TestMethod]
    public void BitAccordionListShouldKeepOnlyOnePanelOpenWhenLeavingTheMultipleExpandMode()
    {
        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Multiple, true);
            parameters.Add(p => p.Items, GetItems());
        });

        component.FindAll(".bit-acd-hdr")[0].Click();
        component.FindAll(".bit-acd-hdr")[2].Click();
        Assert.AreEqual(2, component.FindAll(".bit-acd-con.bit-acd-cex").Count);

        component.Render(parameters => parameters.Add(p => p.Multiple, false));

        Assert.AreEqual(1, component.FindAll(".bit-acd-con.bit-acd-cex").Count);
        CollectionAssert.AreEqual(new[] { "a" }, component.Instance.GetExpandedKeys().ToArray());
    }

    [TestMethod]
    public async Task BitAccordionListCollapseAllShouldCloseTheDisabledItemsAsWell()
    {
        var items = GetItems();
        items[1].IsEnabled = false;

        var component = RenderComponent<BitAccordionList<BitAccordionListItem>>(parameters =>
        {
            parameters.Add(p => p.Multiple, true);
            parameters.Add(p => p.Items, items);
            parameters.Add(p => p.DefaultExpandedKeys, ["a", "b"]);
        });

        Assert.AreEqual(2, component.FindAll(".bit-acd-con.bit-acd-cex").Count);

        await component.InvokeAsync(() => component.Instance.CollapseAll());

        component.WaitForAssertion(() => Assert.AreEqual(0, component.FindAll(".bit-acd-con.bit-acd-cex").Count));
    }

    public class KeylessItem
    {
        public string? Name { get; set; }
    }

    public class ReadOnlyStateItem
    {
        public string? Name { get; set; }

        public string? Key => Name;

        public bool IsExpanded => false;
    }
}
