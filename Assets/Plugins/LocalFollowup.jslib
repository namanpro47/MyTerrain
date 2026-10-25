mergeInto(LibraryManager.library, {
  LocalFollowupDownload: function(name, mime, content) {
    var blob = new Blob([UTF8ToString(content)], {type: UTF8ToString(mime)});
    var url = URL.createObjectURL(blob);
    var link = document.createElement('a');
    link.href = url; link.download = UTF8ToString(name);
    document.body.appendChild(link); link.click(); link.remove();
    setTimeout(function() { URL.revokeObjectURL(url); }, 60000);
  },
  LocalFollowupCopy: function(content) {
    var text = UTF8ToString(content);
    function fallback() {
      var field = document.createElement('textarea');
      field.value = text; field.style.position = 'fixed'; field.style.top = '0';
      document.body.appendChild(field); field.focus(); field.select();
      if (document.execCommand('copy')) field.remove();
      else { field.setAttribute('aria-label', 'Copy this text, then press Escape to return'); field.onkeydown = function(e) { if(e.key === 'Escape') field.remove(); }; }
    }
    if (navigator.clipboard && navigator.clipboard.writeText) navigator.clipboard.writeText(text).catch(fallback);
    else fallback();
  }
});
