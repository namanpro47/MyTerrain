mergeInto(LibraryManager.library, {
  TerraNarrationSet: function(text, enabled, volume) {
    if(window.terraNarration)window.terraNarration.set(UTF8ToString(text),enabled,volume);
  },
  TerraNarrationReplay: function() { if(window.terraNarration)window.terraNarration.replay(); },
  TerraNarrationStatus: function() { return window.terraNarration ? window.terraNarration.status() : 4; }
});
