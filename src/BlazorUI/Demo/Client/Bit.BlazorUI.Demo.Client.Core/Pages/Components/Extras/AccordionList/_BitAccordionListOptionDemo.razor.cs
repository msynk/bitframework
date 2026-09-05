namespace Bit.BlazorUI.Demo.Client.Core.Pages.Components.Extras.AccordionList;

public partial class _BitAccordionListOptionDemo
{
    private int clickCounter;
    private int readOnlyClickCount;
    private bool lockToggling;
    private string? expandedTitle;
    private string? collapsedTitle;
    private string? toggledTitle;
    private string? actionedTitle;
    private string? togglingReport;
    private string? boundExpandedKey = "users";
    private IEnumerable<string> boundExpandedKeys = ["general"];
    private IEnumerable<string> programmaticKeys = [];
    private BitAccordionList<BitAccordionListOption>? accordionListRef;

    private List<BitButtonGroupItem> bindingButtons =>
    [
        new() { Key = "general", Text = "General" },
        new() { Key = "users", Text = "Users" },
        new() { Key = "advanced", Text = "Advanced" },
    ];

    private void HandleOnToggling(BitAccordionListToggleArgs<BitAccordionListOption> args)
    {
        togglingReport = $"{args.Item.Title} is {(args.IsExpanding ? "expanding" : "collapsing")} ({args.Reason})";

        args.Cancel = lockToggling;
    }
}
