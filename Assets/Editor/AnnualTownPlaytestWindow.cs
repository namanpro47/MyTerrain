using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Temporary developer controls. Separate from TerraMain, TerraHud and build scenes while UI is rebuilt.
/// TODO(game UI): bind the same AnnualTownSession transactions and summary to the real planning screen.
/// </summary>
public sealed class AnnualTownPlaytestWindow : EditorWindow
{
    AnnualTownSession session;
    AnnualTownDefinition definition;
    int selectedSite;
    Vector2 scroll;
    string error;
    bool showRules;

    [MenuItem("Terra/Annual Town/Playtest Year Loop")]
    public static void Open()
    {
        var window = GetWindow<AnnualTownPlaytestWindow>("Annual playtest");
        window.minSize = new Vector2(720, 600);
        window.Show();
    }

    void NewRun()
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../worlds/meadowbrook-v1"));
        var world = JsonUtility.FromJson<WorldData>(File.ReadAllText(Path.Combine(root, "world.json")));
        var scenario = JsonUtility.FromJson<Scenario>(File.ReadAllText(Path.Combine(root, "scenario.json")));
        var nextDefinition = AnnualTownDemo.FromWorldPack(world, scenario, "meadowbrook-v1");
        var next = new GameObject("Annual playtest session") { hideFlags = HideFlags.HideAndDontSave };
        try
        {
            var nextSession = next.AddComponent<AnnualTownSession>();
            nextSession.Initialize(nextDefinition, "playtest-" + Guid.NewGuid().ToString("N"));
            if (session != null) DestroyImmediate(session.gameObject);
            definition = nextDefinition; session = nextSession;
            selectedSite = 0; scroll = Vector2.zero;
        }
        catch { DestroyImmediate(next); throw; }
    }

    void OnDisable() { if (session != null) DestroyImmediate(session.gameObject); }
    void Attempt(Action action)
    {
        try { action(); error = null; GUI.FocusControl(null); }
        catch (Exception exception) { error = exception.Message; }
        Repaint();
    }

    void OnGUI()
    {
        float previousLabelWidth = EditorGUIUtility.labelWidth;
        try
        {
            EditorGUIUtility.labelWidth = Math.Min(340, position.width * .46f);
            DrawWindow();
        }
        finally { EditorGUIUtility.labelWidth = previousLabelWidth; }
    }

    void DrawWindow()
    {
        GUILayout.Label("Meadowbrook Valley / annual loop test", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Developer playtest only. Uses the selected rural map's sites; leaves the main scene and HUD untouched. Closing this window discards the test run; export first to keep it.", MessageType.Info);
        EditorGUILayout.HelpBox(AnnualTownSummary.PlaceholderNotice, MessageType.Warning);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(session == null ? "Start test run" : "Reset test run")) Attempt(NewRun);
            using (new EditorGUI.DisabledScope(session == null))
            {
                if (GUILayout.Button("Export save JSON")) Attempt(Export);
                if (GUILayout.Button("Import save JSON")) Attempt(Import);
            }
        }
        if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Error);
        if (session == null) return;
        var state = session.State;
        EditorGUILayout.LabelField("Year " + state.year + " / " + definition.years, state.phase.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Public budget", definition.currency + " / " + definition.priceYear + " prices (authored demo)");
        EditorGUILayout.LabelField("Cash / uncommitted", Money(state.cashCents) + " / " + Money(session.AvailableCents));
        EditorGUILayout.LabelField("Existing obligations / queued commitments", Money(session.CommittedCents) + " / " + Money(session.QueuedCommitmentCents));
        EditorGUILayout.LabelField("Arrears (included in obligations)", Money(state.arrearsCents));
        showRules = EditorGUILayout.Foldout(showRules, "Demo rules and price assumptions");
        if (showRules) EditorGUILayout.HelpBox(definition.assumptions + " " + definition.rubric.explanation, MessageType.None);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (state.phase == TownPhase.Planning) Planning(state);
        else if (state.phase == TownPhase.Resolving)
        {
            EditorGUILayout.HelpBox("The year's request is frozen. Retry keeps the same input; no budget is charged before acceptance.", MessageType.Info);
            if (GUILayout.Button("Retry placeholder simulation")) Attempt(() => session.EndYearOffline(state.pendingRequest.requestId));
            if (GUILayout.Button("Return to unsubmitted plan")) Attempt(() => session.CancelResolution(state.revision));
        }
        else Review(state);
        EditorGUILayout.EndScrollView();
        // Stable footer: the next-year control stays reachable even with many site details.
        if (state.phase == TownPhase.Reviewing && GUILayout.Button("Continue to Year " + (state.year + 1), GUILayout.Height(32)))
            Attempt(() => { session.ContinueYear(state.revision); scroll = Vector2.zero; });
    }

    void Planning(AnnualTownSnapshot state)
    {
        GUILayout.Label("Choose actions, then conclude this year", EditorStyles.boldLabel);
        selectedSite = EditorGUILayout.Popup("Site", selectedSite, definition.sites.Select(s => s.name).ToArray());
        var site = definition.sites[selectedSite];
        EditorGUILayout.LabelField("Area", (site.areaM2 / 10000).ToString("0.##", CultureInfo.InvariantCulture) + " ha");
        var existing = state.assets.FirstOrDefault(a => a.siteId == site.id);
        if (site.protectedSite) EditorGUILayout.HelpBox("Protected site: no actions available.", MessageType.Info);
        else if (existing != null) EditorGUILayout.HelpBox("Already committed: " + ActionTitle(existing.siteId, existing.actionId) + ". Continues next year; historical spending cannot be undone.", MessageType.Info);
        else foreach (var offer in definition.actions.Where(a => a.siteId == site.id))
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(offer.title, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Upfront / annual program / annual upkeep", Money(offer.upfrontPublicCostCents) + " / " + Money(offer.annualPublicPaymentCents) + " / " + Money(offer.annualPublicOperatingCents));
                EditorGUILayout.LabelField("Timing", offer.constructionYears + " construction years; " + offer.establishmentYears + " establishment years; " + offer.operatingYears + " operating years");
                long remaining = session.Preview(site.id, offer.actionId);
                EditorGUILayout.LabelField("Uncommitted after selecting (full term)", Money(remaining));
                using (new EditorGUI.DisabledScope(remaining < 0))
                    if (GUILayout.Button(state.queue.Any(q => q.siteId == site.id && q.actionId == offer.actionId) ? "Queued" : "Queue / replace this year's choice"))
                        Attempt(() => session.QueueAction(site.id, offer.actionId, state.revision));
            }
        }
        GUILayout.Space(10);
        GUILayout.Label("This year's unsubmitted plan", EditorStyles.boldLabel);
        if (state.queue.Length == 0) EditorGUILayout.HelpBox("No new action. Waiting still advances existing projects, annual costs and conditions.", MessageType.None);
        foreach (var item in state.queue)
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(definition.sites.First(s => s.id == item.siteId).name + " / " + ActionTitle(item.siteId, item.actionId));
                if (GUILayout.Button("Remove", GUILayout.Width(80))) Attempt(() => session.RemoveQueuedAction(item.siteId, state.revision));
            }
        using (new EditorGUI.DisabledScope(!session.CanUndo))
            if (GUILayout.Button("Undo last plan edit")) Attempt(() => session.UndoPlan(state.revision));
        if (GUILayout.Button(state.queue.Length == 0 ? "Conclude year / wait" : "Conclude year / commit plan", GUILayout.Height(32)))
            Attempt(() => { session.EndYearOffline(state.runId + "/year-" + state.year); scroll = Vector2.zero; });
    }

    void Review(AnnualTownSnapshot state)
    {
        var summary = session.LatestSummary;
        GUILayout.Label(summary.finalYear ? "Year " + summary.year + " / final results" : "Year " + summary.year + " / end-year summary", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(summary.headline + " " + summary.continuingProjects + " existing projects carried forward.\n" + summary.scenario, MessageType.Info);
        var ledger = summary.ledger;
        EditorGUILayout.LabelField("Opening cash", Money(ledger.openingCashCents));
        EditorGUILayout.LabelField("Upfront / program / upkeep paid", Money(ledger.upfrontPaidCents) + " / " + Money(ledger.programPaidCents) + " / " + Money(ledger.operatingPaidCents));
        EditorGUILayout.LabelField("Repairs / old arrears paid", Money(ledger.repairPaidCents) + " / " + Money(ledger.arrearsPaidCents));
        EditorGUILayout.LabelField("Appropriation / public receipts", Money(ledger.appropriationCents) + " / " + Money(ledger.publicReceiptsCents));
        EditorGUILayout.LabelField("Total spent / received", Money(summary.spentCents) + " / " + Money(summary.receivedCents));
        EditorGUILayout.LabelField("Closing cash / change", Money(ledger.closingCashCents) + " / " + Money(summary.cashChangeCents));
        EditorGUILayout.LabelField("Future obligations incl. arrears / free cash", Money(ledger.futureCommitmentsCents) + " / " + Money(summary.freeAfterCommitmentsCents));
        foreach (var metric in new[] { summary.economic, summary.social, summary.environmental })
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(metric.label + "  " + metric.score.ToString("0.0") + " / 100"
                    + (metric.hasPreviousYear ? "  (" + metric.change.ToString("+0.0;-0.0;0.0") + " vs last year)" : "  (first annual result)"), EditorStyles.boldLabel);
                EditorGUILayout.LabelField(metric.evidence, EditorStyles.wordWrappedLabel);
                if (showRules) EditorGUILayout.LabelField(metric.rubric, EditorStyles.wordWrappedLabel);
            }
        }
        GUILayout.Label("What happened on the land", EditorStyles.boldLabel);
        foreach (var line in summary.siteChanges) EditorGUILayout.LabelField(line, EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);
        // TODO(game UI): animate this accepted state; never let animation or AI change the ledger.
        if (state.phase == TownPhase.Completed) EditorGUILayout.HelpBox("Ten years complete. Export the accepted history or reset to try a different plan. No Year 11 is created.", MessageType.Info);
    }

    string ActionTitle(string site, string action) => definition.actions.First(a => a.siteId == site && a.actionId == action).title;
    static string Money(long cents) => (cents / 100m).ToString("C2", CultureInfo.GetCultureInfo("en-US"));
    void Export()
    {
        string path = EditorUtility.SaveFilePanel("Export annual playtest", "", "terra-annual-save.json", "json");
        if (!string.IsNullOrEmpty(path)) File.WriteAllText(path, session.ExportSave());
    }
    void Import()
    {
        string path = EditorUtility.OpenFilePanel("Import annual playtest", "", "json");
        if (!string.IsNullOrEmpty(path)) { session.ImportSave(File.ReadAllText(path)); scroll = Vector2.zero; }
    }
}
