mergeInto(LibraryManager.library, {
  TerraPixelRatio: function() { return window.devicePixelRatio || 1; },
  TerraLandscapeReady: function() {
    console.info('TERRA_INTERACTIVE_MS', Math.round(performance.now()));
  },
  TerraMetricsStart: function() {
    window.terraMetrics = { samples: [], discardedStale: 0 };
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
    requestAnimationFrame(function() {
      console.info('TERRA_INPUT_LATENCY', JSON.stringify({method:'DOM input to first rAF after Unity feedback; presentation opportunity, not physical display latency',samples:metrics.samples,discardedStale:metrics.discardedStale}));
    });
  }
});
