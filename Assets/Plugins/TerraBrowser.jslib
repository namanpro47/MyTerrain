mergeInto(LibraryManager.library, {
  TerraPixelRatio: function() { return window.devicePixelRatio || 1; },
  TerraLandscapeReady: function() {
    console.info('TERRA_INTERACTIVE_MS', Math.round(performance.now()));
  },
  TerraMetricsStart: function() {
    var previous = window.terraMetrics;
    if (previous) {
      document.removeEventListener('visibilitychange', previous.visibilityListener);
      window.removeEventListener('blur', previous.blurListener);
    }
    var metrics = window.terraMetrics = { samples: [], discardedStale: 0,
      hiddenAtStart: document.hidden, focusedAtStart: document.hasFocus(), visibilityChanges: [], blurEvents: 0 };
    metrics.visibilityListener = function() { metrics.visibilityChanges.push({at:performance.now(),hidden:document.hidden}); };
    metrics.blurListener = function() { metrics.blurEvents++; };
    document.addEventListener('visibilitychange', metrics.visibilityListener);
    window.addEventListener('blur', metrics.blurListener);
    window.terraInputPending = null;
  },
  TerraFeedbackReady: function(kind) {
    var metrics = window.terraMetrics, input = window.terraInputPending;
    if (!metrics || !input) return;
    window.terraInputPending = null;
    if (performance.now() - input.at > 500) { metrics.discardedStale++; return; }
    // The next rAF is a browser presentation opportunity, not a GPU/display measurement.
    requestAnimationFrame(function() {
      metrics.samples.push({kind:kind === 1 ? 'placement' : 'preview', source:input.source, milliseconds:performance.now()-input.at});
    });
  },
  TerraMetricsEnd: function() {
    var metrics = window.terraMetrics;
    window.terraMetrics = null;
    if (!metrics) return;
    document.removeEventListener('visibilitychange', metrics.visibilityListener);
    window.removeEventListener('blur', metrics.blurListener);
    var visibility = {hiddenAtStart:metrics.hiddenAtStart,focusedAtStart:metrics.focusedAtStart,
      hiddenAtEnd:document.hidden,focusedAtEnd:document.hasFocus(),changes:metrics.visibilityChanges,blurEvents:metrics.blurEvents};
    requestAnimationFrame(function() {
      console.info('TERRA_INPUT_LATENCY', JSON.stringify({method:'DOM input to first rAF after Unity feedback; presentation opportunity, not physical display latency',samples:metrics.samples,discardedStale:metrics.discardedStale,visibility:visibility}));
    });
  }
});
